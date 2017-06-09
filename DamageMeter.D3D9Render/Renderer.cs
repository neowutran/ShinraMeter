using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using DamageMeter.D3D9Render.Overlays;
using DamageMeter.D3D9Render.TeraData;
using Data;

namespace DamageMeter.D3D9Render
{

    internal class DpsRow:IDisposable
    {
        private TextLabel PName;
        private TextLabel PDmg;
        private TextLabel PDsp;
        private TextLabel PCrit;

        public DpsRow(ClassInfo info, int row, int x=0, int y=0)
        {
            PName = new TextLabel("Arial", 7, TypeFace.None, new Point(x + 5, y + row * 20 + 5), Color.FromArgb(220, Color.YellowGreen), info.PName, false, true);
            PDmg = new TextLabel("Arial", 7, TypeFace.None, new Point(x + 85, y + row * 20 + 5), Color.FromArgb(220, Color.White), info.PDmg, false, true);
            PDsp = new TextLabel("Arial", 7, TypeFace.None, new Point(x + 115, y + row * 20 + 5), Color.FromArgb(220, Color.White), info.PDsp, false, true);
            PCrit = new TextLabel("Arial", 7, TypeFace.None, new Point(x + 155, y + row * 20 + 5), Color.FromArgb(220, Color.OrangeRed), info.PCrit, false, true);
        }

        public void UpdateRow(ClassInfo info)
        {
            PName.Text = info.PName;
            PDmg.Text = info.PDmg;
            PDsp.Text = info.PDsp;
            PCrit.Text = info.PCrit;
            PName.Visible = true;
            PDmg.Visible = true;
            PDsp.Visible = true;
            PCrit.Visible = true;
        }

        public void Hide()
        {
            PName.Visible = false;
            PDmg.Visible = false;
            PDsp.Visible = false;
            PCrit.Visible = false;
        }

        public void Dispose()
        {
            PName.Destroy();
            PDmg.Destroy();
            PDsp.Destroy();
            PCrit.Destroy();
        }
    }

    public class Renderer : IDisposable
    {
        // cache overlay
        private readonly List<DpsRow> _cacheOverlays = new List<DpsRow>();
        private Box _box;
        private int _playerCount = 0;

        // position X ,Y 
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        // destroy all visual on exit or on disposing
        public void Dispose()
        {
            Destroy();
        }
        // redraw box with validate
        private void RedrawBox(int count)
        {
            if (_box==null)
            {
                _box= new Box(new Rectangle(X, Y, 182, count * 20), Color.FromArgb(170, Color.Black), true);
                return;
            }
            if (_box.Id == -1)
            {
                _box.Destroy();
                _box = new Box(new Rectangle(X, Y, 182, count * 20), Color.FromArgb(170, Color.Black), true);
            }
            else { _box.Height = count * 20; }
        }
        // check device for ready draw
        private static bool ChekForDevice()
        {
            DxOverlay.SetParam("process", "TERA.exe");
            return DxOverlay.GetScreenSpecs().X < 200;
        }
        // draw the overlays with caching
        public void Draw(List<ClassInfo> userListData)
        {
            if (Process.GetProcessesByName("TERA").FirstOrDefault() == null)
            {
                _cacheOverlays.ForEach(x=>x.Dispose());
                _cacheOverlays.Clear();
                _box?.Destroy();
                _box = null;
                _playerCount=0;
                return;
            }
            if (ChekForDevice()) return;
            var listCount = userListData.Count;
            if (listCount > 30) listCount = 30;
            if (listCount > BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed) listCount = BasicTeraData.Instance.WindowData.NumberOfPlayersDisplayed;
            if (_playerCount != listCount) RedrawBox(listCount);
            _playerCount = listCount;
            if (_cacheOverlays.Count > _playerCount)
            {
                for (var i = _playerCount; i < _cacheOverlays.Count; i++) { _cacheOverlays[i].Hide(); }
            }
            for (var i = 0; i < _playerCount; i++)
            {
                if (_cacheOverlays.Count <= i) _cacheOverlays.Add( new DpsRow(userListData[i],i,X,Y));
                else _cacheOverlays[i].UpdateRow(userListData[i]);
            }
        }

        // destroy all visual and remove from memory
        public void Destroy()
        {
            DxOverlay.DestroyAllVisual();
        }
    }
}