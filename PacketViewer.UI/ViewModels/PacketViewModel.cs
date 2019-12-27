using DamageMeter.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Tera.Game.Messages;

namespace PacketViewer.UI
{
    public class PacketViewModel : INotifyPropertyChanged
    {
        public ParsedMessage Message { get; }
        public int Count { get; }
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string Time => $"{Message.Time.ToString("HH:mm:ss.fff")}";
        public List<List<string>> RowsHex => ParseDataHex(Message.Payload);
        public List<List<string>> RowsText => ParseDataText(Message.Payload);

        private List<List<string>> ParseDataHex(ArraySegment<byte> p)
        {
            var a = p.ToArray();
            var s = BitConverter.ToString(a).Replace("-", string.Empty);
            var rows = (s.Length / 32) + 1;
            var result = new List<List<string>>();
            for (int i = 0; i < rows; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < 32; j += 8)
                {
                    if (j + 32 * i >= s.Length) continue;

                    var chunk = s.Substring(j + (32 * i)).Length <= 8 ? s.Substring(j + (32 * i)) : s.Substring(j + (32 * i), 8);
                    row.Add(chunk);
                }
                for (int j = row.Count; j < 4; j++)
                {
                    row.Add("");
                }
                result.Add(row);
            }
            return result;
        }
        private List<List<string>> ParseDataText(ArraySegment<byte> p)
        {
            var a = p.ToArray();
            var sb = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                var c = (char)a[i];
                if (c > 0x21 && c < 0x80) sb.Append(c);
                else sb.Append("⋅");
                //if ((i + 1) % 16 == 0) sb.Append("\n");
            }
            var s = sb.ToString();

            var rows = (s.Length / 16) + 1;
            var result = new List<List<string>>();
            for (int i = 0; i < rows; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < 16; j += 4)
                {
                    if (j + 16 * i >= s.Length) continue;

                    var chunk = s.Substring(j + (16 * i)).Length <= 4 ? s.Substring(j + (16 * i)) : s.Substring(j + (16 * i), 4);
                    row.Add(chunk);
                }
                for (int j = row.Count; j < 4; j++)
                {
                    row.Add("");
                }
                result.Add(row);
            }
            return result;
        }

        private List<ByteViewModel> _data;
        private bool _isSearched;
        public List<ByteViewModel> Data => _data ?? (_data = BuildByteView());

        public bool IsSearched
        {
            get => _isSearched;
            set
            {
                //if (_isSelected == value) return;
                _isSearched = value;
                OnPropertyChanged(nameof(IsSearched));
            }
        }

        private List<ByteViewModel> BuildByteView()
        {
            var res = new List<ByteViewModel>();
            for (int i = 0; i < Message.Payload.Count; i += 4)
            {
                var count = i + 4 > Message.Payload.Count ? Message.Payload.Count - i : 4;
                var chunk = new ArraySegment<byte>(Message.Payload.ToArray(), i, count);
                var bvm = new ByteViewModel(chunk.ToArray());
                res.Add(bvm);
            }
            return res;
        }

        public PacketViewModel(ParsedMessage message, int c)
        {
            Message = message;
            Count = c;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshName()
        {
            OnPropertyChanged(nameof(Message));
        }

        public void RefreshData(int i)
        {
            Data[i].Refresh();

        }
    }
}