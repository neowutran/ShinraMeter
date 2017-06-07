using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using ShinraMeter.D3D9Render.Overlays;
using ShinraMeter.D3D9Render.TeraData;

namespace ShinraMeter.D3D9Render
{
    public class D3D9Render : IDisposable
    {
        // cache overlay
        private readonly List<List<Overlay>> _cacheOverlays = new List<List<Overlay>> {new List<Overlay>(1), new List<Overlay>()};
        // check data for redraw box
        private int _dataUserCount;
        // validate data to redraw box
        private bool _needUpdateBox = true;
        // position X ,Y 
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        // Dps data see ToClassInfo method
        private List<ClassInfo> DpsData { get; set; } = new List<ClassInfo>();
        // destroy all visual on exit or on disposing
        public void Dispose()
        {
            Destroy();
        }
        // redraw box with validate
        private void RedrawBox()
        {
            if (!_needUpdateBox) { return; }
            if (!_cacheOverlays[0].Exists(overlay => overlay.GetType() == typeof(Box)))
            {
                _cacheOverlays[0].Add(new Box(new Rectangle(X, Y, 177, DpsData.Count * 20), Color.FromArgb(170, Color.Black), true));
                return;
            }
            _cacheOverlays[0][0].Destroy();
            _cacheOverlays[0][0] = new Box(new Rectangle(X, Y, 177, DpsData.Count * 20), Color.FromArgb(170, Color.Black), true);
        }
        // check device for ready draw
        private static void ChekForDevice()
        {
            DxOverlay.SetParam("process", "TERA.exe");
        }
        // draw the overlays with caching
        public void Draw(List<ClassInfo> userListData)
        {
            if (Process.GetProcessesByName("TERA").FirstOrDefault() == null) { return; }
            DpsData = userListData;
            _needUpdateBox = _dataUserCount.Equals(userListData.Count);
            _dataUserCount = userListData.Count;
            ChekForDevice();
            RedrawBox();
            if (_cacheOverlays[1].Count / 4 != DpsData.Count)
            {
                for (var i = DpsData.Count - 1; i < _cacheOverlays.Count / 4 - DpsData.Count - 1; i++) { _cacheOverlays[1][i].Destroy(); }
                _cacheOverlays.RemoveRange(DpsData.Count - 1, _cacheOverlays.Count / 4 - DpsData.Count);
            }
            for (var i = 0; i < DpsData.Count; i++)
            {
                if (_cacheOverlays[1].Count < i ||_cacheOverlays[1][0] == null)
                {
                    _cacheOverlays[1].Add(new TextLabel("Arial", 7, TypeFace.None, new Point(X + 5, Y + i * 20 + 5), Color.FromArgb(220, Color.YellowGreen), DpsData[i].PName, false, true));
                }
                else { SetTextLabel(_cacheOverlays[1][0], DpsData[i].PName); }

                if (_cacheOverlays[1].Count < i || _cacheOverlays[1][1] == null)
                {
                    _cacheOverlays[1].Add(new TextLabel("Arial", 7, TypeFace.None, new Point(X + 85, Y + i * 20 + 5), Color.FromArgb(220, Color.White), DpsData[i].PDmg, false, true));
                }
                else { SetTextLabel(_cacheOverlays[1][1], DpsData[i].PDmg); }

                if (_cacheOverlays[1].Count < i || _cacheOverlays[1][2] == null)
                {
                    _cacheOverlays[1].Add(new TextLabel("Arial", 7, TypeFace.None, new Point(X + 115, Y + i * 20 + 5), Color.FromArgb(220, Color.White), DpsData[i].PDsp, false, true));
                }
                else { SetTextLabel(_cacheOverlays[1][2], DpsData[i].PDsp); }

                if (_cacheOverlays[1].Count < i || _cacheOverlays[1][3] == null)
                {
                    _cacheOverlays[1].Add(new TextLabel("Arial", 7, TypeFace.None, new Point(X + 155, Y + i * 20 + 5), Color.FromArgb(220, Color.OrangeRed), DpsData[i].PCrit, false, true));
                }
                else { SetTextLabel(_cacheOverlays[1][3], DpsData[i].PCrit); }
            }
        }
        // update data for texlabel
        public void SetTextLabel(Overlay textLabel, string text)
        {
            ((TextLabel) textLabel).Text = text;
        }
        // destroy all visual and remove from memory
        public void Destroy()
        {
            DxOverlay.DestroyAllVisual();
        }
    }
}