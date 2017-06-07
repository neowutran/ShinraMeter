using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using ShinraMeter.D3D9Render.Overlay;
using ShinraMeter.D3D9Render.TeraData;

namespace ShinraMeter.D3D9Render
{
    public class D3D9Render : IDisposable
    {
        private readonly List<Dictionary<string, Overlay.Overlay>> _cacheOverlays =
            new List<Dictionary<string, Overlay.Overlay>>(2) {new Dictionary<string, Overlay.Overlay>(), new Dictionary<string, Overlay.Overlay>()};

        private int _dataUserCount;
        private bool _needUpdateBox = true;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        private List<ClassInfo> DpsData { get; set; } = new List<ClassInfo>();

        public void Dispose()
        {
            Destroy();
        }

        private void RedrawBox()
        {
            if (!_needUpdateBox) { return; }
            if (!_cacheOverlays[0].ContainsKey("box"))
            {
                _cacheOverlays[0].Add("box", new Box(new Rectangle(X, Y, 177, DpsData.Count * 20), Color.FromArgb(170, Color.Black), true));
                return;
            }
            _cacheOverlays[0]["box"].Destroy();
            _cacheOverlays[0]["box"] = new Box(new Rectangle(X, Y, 177, DpsData.Count * 20), Color.FromArgb(170, Color.Black), true);
        }

        private static void ChekForDevice()
        {
            DxOverlay.SetParam("process", "TERA.exe");
        }

        public void Draw(List<ClassInfo> userListData)
        {
            if(Process.GetProcessesByName("TERA").FirstOrDefault() == null)
                return;
            DpsData = userListData;
            _needUpdateBox = _dataUserCount.Equals(userListData.Count);
            _dataUserCount = userListData.Count;
            ChekForDevice();
            RedrawBox();
            foreach (var keyValuePair in _cacheOverlays[1]) { keyValuePair.Value.Destroy(); }
            _cacheOverlays[1].Clear();
            for (var i = 0; i < DpsData.Count; i++)
            {
                if(!_cacheOverlays[1].ContainsKey($"username_{i}"))
                    _cacheOverlays[1].Add($"username_{i}",
                        new TextLabel("Arial", 7, TypeFace.None, new Point(X + 5, Y + i * 20 + 5), Color.FromArgb(220, Color.YellowGreen), DpsData[i].PName, false, true));
                else
                    SetTextLabel(_cacheOverlays[1][$"username_{i}"], $"username_{i}");
                _cacheOverlays[1].Add($"userdmg_{i}",
                    new TextLabel("Arial", 7, TypeFace.None, new Point(X + 85, Y + i * 20 + 5), Color.FromArgb(220, Color.White), DpsData[i].PDmg, false, true));
                _cacheOverlays[1].Add($"userdps_{i}",
                    new TextLabel("Arial", 7, TypeFace.None, new Point(X + 115, Y + i * 20 + 5), Color.FromArgb(220, Color.White), DpsData[i].PDsp, false, true));
                _cacheOverlays[1].Add($"usercrit_{i}",
                    new TextLabel("Arial", 7, TypeFace.None, new Point(X + 155, Y + i * 20 + 5), Color.FromArgb(220, Color.OrangeRed), DpsData[i].PCrit, false,
                        true));
            }
        }

        public void SetTextLabel(Overlay.Overlay textLabel, string text)
        {
            ((TextLabel)textLabel).Text = text;
        }

        public void Destroy()
        {
            DxOverlay.DestroyAllVisual();
        }
    }
}