using Data;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class HudManager : DependencyObject
    {
        private static HudManager _instance;
        public static HudManager Instance => _instance ?? (_instance = new HudManager());
        public static bool Logged;

        private SynchronizedObservableCollection<Boss> _bosses = new SynchronizedObservableCollection<Boss>();
        public SynchronizedObservableCollection<Boss> CurrentBosses
        {
            get { return _bosses; }
            set
            {
                if (_bosses == value) return;
                _bosses = value;
            }
        }

        public HudManager()
        {
            //_bosses.Add(new Boss(new EntityId(0), 100000, 75000, "Test", Visibility.Visible));
            //_bosses.Add(new Boss(new EntityId(0), 200000, 75000, "Test2", Visibility.Visible));
            //_bosses[0].Buffs.Add(new BuffDuration(
            //    new HotDot(1111, "Endurance", 0, 0, 1, HotDot.DotType.abs, 600000, 1, "Test1", "1", "test tooltip", "icon_status.yellowaura_tex", HotDot.AbnormalityType.Debuff, false)
            //    , 600000, 4));
        }

        public void AddOrUpdateBoss(S_BOSS_GAGE_INFO message)
        {
            var boss = _bosses.FirstOrDefault(x => x.EntityId == message.EntityId);
            if (boss == null)
            {
                var bossEntity = NetworkController.Instance.EntityTracker.GetOrNull(message.EntityId) as NpcEntity;
                if (bossEntity == null) return;
                boss = new Boss(bossEntity, Visibility.Visible);
                _bosses.Add(boss);
            }
            boss.CurrentHP = message.HpRemaining;
        }

        public void RemoveBoss(SDespawnNpc message)
        {
            var boss = _bosses.FirstOrDefault(x => x.EntityId == message.Npc);
            if (boss == null) return;
            _bosses.Remove(boss);
            boss.Dispose();
        }
    }
    public class BuffDuration : TSPropertyChanged, IDisposable
    {
        public HotDot Buff { get; }
        private readonly Timer _timer=new Timer(1000);
        private int _durationLeft;

        public void Refresh()
        {
            _timer.Stop();
            if (Buff.Time!=0) _timer.Start();
            NotifyPropertyChanged("Refresh");
        }
        public int DurationLeft
        {
            get { return _durationLeft; }
            set
            {
                if (value == _durationLeft) return;
                _durationLeft = value;
                NotifyPropertyChanged("DurationLeft");
            }
        }

        private int _duration;
        public int Duration {
            get { return _duration; }
            set
            {
                if (value==_duration)return;
                _duration = value;
                NotifyPropertyChanged("Duration");
            } 
        }

        private int _stacks;
        public int Stacks{
            get { return _stacks; }
            set
            {
                if (value == _stacks) return;
                _stacks = value;
                NotifyPropertyChanged("Stacks");
            }
        }

        public BuffDuration(HotDot b, int d, int s)
        {
            Buff = b;
            Duration = d;
            Stacks = s;
            DurationLeft = d;
            _timer.Elapsed += (se, ev) => { DurationLeft = DurationLeft - 1000; if (DurationLeft<=0) _timer.Stop(); };
            if (b.Time!=0) _timer.Start();
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }

    public class Boss : TSPropertyChanged, IDisposable
    {
        public EntityId EntityId { get; }
        public NpcEntity Entity { get; }
        private string _name;

        public string Name {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private SynchronizedObservableCollection<BuffDuration> _buffs;
        public SynchronizedObservableCollection<BuffDuration> Buffs { 
            get {return _buffs;}
            set
            {
                if (_buffs == value) return;
                _buffs = value;
                NotifyPropertyChanged("Buffs");
            }
        }

        private bool _enraged;
        public bool Enraged {
            get {return _enraged;}
            set
            {
                if (_enraged == value) return;
                _enraged = value;
                NotifyPropertyChanged("Enraged");
            }
        }

        private float _maxHp;
        public float MaxHP {
            get { return _maxHp; } 
            set
            {
                if (_maxHp == value) return;
                _maxHp = value;
                NotifyPropertyChanged("MaxHP");
            }
        }
        private float _currentHp;
        public float CurrentHP {
            get { return _currentHp; } 
            set
            {
                if (_currentHp == value) return;
                _currentHp = value;
                NotifyPropertyChanged("CurrentHP");
                NotifyPropertyChanged("CurrentPercentage");
            }
        }
        public float CurrentPercentage => _maxHp == 0 ? 0 : (_currentHp / _maxHp);

        Visibility visible;
        public Visibility Visible {
            get { return visible; }
            set
            {
                if (visible == value) return;
                visible = value;
                NotifyPropertyChanged("Visible");
            }
        }

        public void AddOrRefresh(Abnormality abnormality)
        {
            if (abnormality.HotDot.Id == (int) HotDotDatabase.StaticallyUsedBuff.Enraged) { Enraged = true; return; }
            var existing = Buffs.FirstOrDefault(x => x.Buff.Id == abnormality.HotDot.Id);
            if (existing == null)
            {
                Buffs.Add(new BuffDuration(abnormality.HotDot,abnormality.Duration,abnormality.Stack));
                return;
            }
            existing.Duration = abnormality.Duration;
            existing.DurationLeft = abnormality.Duration;
            existing.Stacks = abnormality.Stack;
            existing.Refresh();
        }

        public void EndBuff(int id)
        {
            if (id == (int)HotDotDatabase.StaticallyUsedBuff.Enraged) { Enraged = false; return; }
            try
            {
                var buff = Buffs.FirstOrDefault(x => x.Buff.Id == id);
                if (buff == null) return;
                Buffs.Remove(buff);
                buff.Dispose();
            }
            catch (Exception)
            {
                Debug.WriteLine("Cannot remove {0}", id);
            }
        }

        public Boss(EntityId eId, float maxHP, float curHP, string name, Visibility visible)
        {
            EntityId = eId;
            Name = name;
            MaxHP = maxHP;
            CurrentHP = curHP;
            Buffs = new SynchronizedObservableCollection<BuffDuration>();
            Visible = visible;
        }

        public Boss(NpcEntity entity, Visibility visible)
        {
            Entity = entity;
            EntityId = Entity.Id;
            Name = Entity.Info.Name;
            MaxHP = Entity.Info.HP;
            CurrentHP = MaxHP;
            Buffs = new SynchronizedObservableCollection<BuffDuration>();
            Visible = visible;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", EntityId, Name);
        }

        public void Dispose()
        {
            foreach (var buff in _buffs) buff.Dispose();
        }
    }
}
