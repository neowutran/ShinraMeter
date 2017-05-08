using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DamageMeter.UI
{
    internal static class IconUtils
    {
        public static Icon GetIcon(this Bitmap bitmap)
        {
            // Get an Hicon for myBitmap.
            var Hicon = bitmap.GetHicon();
            var newIcon = Icon.FromHandle(Hicon);
            //DestroyIcon(newIcon.Handle);
            return newIcon;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap)) { throw new Win32Exception(); }

            return wpfBitmap;
        }
    }
}