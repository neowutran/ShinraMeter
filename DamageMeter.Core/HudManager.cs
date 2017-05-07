using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using Tera.Game;
using Tera.Game.Abnormality;
using Tera.Game.Messages;

namespace DamageMeter
{
    public class HudManager : DependencyObject
    {
        private static HudManager _instance;
        public static bool Logged;

        private SynchronizedObservableCollection<Boss> _bosses = new SynchronizedObservableCollection<Boss>();

        public static HudManager Instance => _instance ?? (_instance = new HudManager());

        public SynchronizedObservableCollection<Boss> CurrentBosses
        {
            get => _bosses;
            set
            {
                if (_bosses == value) return;
                _bosses = value;
            }
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
            boss.MaxHP = message.TotalHp;
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
        private readonly Timer _timer = new Timer(1000);

        private int _duration;
        private int _durationLeft;

        private int _stacks;

        public BuffDuration(HotDot b, int d, int s)
        {
            Buff = b;
            Duration = d;
            Stacks = s;
            DurationLeft = d;
            _timer.Elapsed += (se, ev) =>
            {
                DurationLeft = DurationLeft - 1000;
                if (DurationLeft <= 0) _timer.Stop();
            };
            if (b.Time != 0) _timer.Start();
        }

        public HotDot Buff { get; }

        public int DurationLeft
        {
            get => _durationLeft;
            set
            {
                if (value == _durationLeft) return;
                _durationLeft = value;
                NotifyPropertyChanged("DurationLeft");
            }
        }

        public int Duration
        {
            get => _duration;
            set
            {
                if (value == _duration) return;
                _duration = value;
                NotifyPropertyChanged("Duration");
            }
        }

        public int Stacks
        {
            get => _stacks;
            set
            {
                if (value == _stacks) return;
                _stacks = value;
                NotifyPropertyChanged("Stacks");
            }
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public void Refresh()
        {
            _timer.Stop();
            if (Buff.Time != 0) _timer.Start();
            NotifyPropertyChanged("Refresh");
        }
    }

    public class Boss : TSPropertyChanged, IDisposable
    {
        private SynchronizedObservableCollection<BuffDuration> _buffs;
        private float _currentHp;

        private bool _enraged;

        private float _maxHp;
        private string _name;

        private Visibility visible;

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

        public EntityId EntityId { get; }
        public NpcEntity Entity { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public SynchronizedObservableCollection<BuffDuration> Buffs
        {
            get => _buffs;
            set
            {
                if (_buffs == value) return;
                _buffs = value;
                NotifyPropertyChanged("Buffs");
            }
        }

        public bool Enraged
        {
            get => _enraged;
            set
            {
                if (_enraged == value) return;
                _enraged = value;
                NotifyPropertyChanged("Enraged");
            }
        }

        public float MaxHP
        {
            get => _maxHp;
            set
            {
                if (_maxHp == value) return;
                _maxHp = value;
                NotifyPropertyChanged("MaxHP");
            }
        }

        public float CurrentHP
        {
            get => _currentHp;
            set
            {
                if (_currentHp == value) return;
                _currentHp = value;
                NotifyPropertyChanged("CurrentHP");
                NotifyPropertyChanged("CurrentPercentage");
            }
        }

        public float CurrentPercentage => _maxHp == 0 ? 0 : _currentHp / _maxHp;

        public Visibility Visible
        {
            get => visible;
            set
            {
                if (visible == value) return;
                visible = value;
                NotifyPropertyChanged("Visible");
            }
        }

        public void Dispose()
        {
            foreach (var buff in _buffs) buff.Dispose();
        }

        public void AddOrRefresh(Abnormality abnormality)
        {
            if (abnormality.HotDot.Id == (int) HotDotDatabase.StaticallyUsedBuff.Enraged)
            {
                Enraged = true;
                return;
            }
            var existing = Buffs.FirstOrDefault(x => x.Buff.Id == abnormality.HotDot.Id);
            if (existing == null)
            {
                Buffs.Add(new BuffDuration(abnormality.HotDot, abnormality.Duration, abnormality.Stack));
                return;
            }
            existing.Duration = abnormality.Duration;
            existing.DurationLeft = abnormality.Duration;
            existing.Stacks = abnormality.Stack;
            existing.Refresh();
        }

        public void EndBuff(int id)
        {
            if (id == (int) HotDotDatabase.StaticallyUsedBuff.Enraged)
            {
                Enraged = false;
                return;
            }
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

        public override string ToString()
        {
            return $"{EntityId} - {Name}";
        }
    }
}