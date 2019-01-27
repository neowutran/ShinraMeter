using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Data
{
    public class ImageDatabase
    {
        public ImageDatabase(string folder)
        {
            EntityStats = new Image {Source = new BitmapImage(new Uri(folder + "stats.png"))};
            EntityStatsClickThrou = new Image {Source = new BitmapImage(new Uri(folder + "stats_click_throu.png"))};
            Chrono = new Image {Source = new BitmapImage(new Uri(folder + "chrono.png"))};
            Chronobar = new Image {Source = new BitmapImage(new Uri(folder + "chronobar.png"))};
            Close = new Image {Source = new BitmapImage(new Uri(folder + "close.png"))};
            History = new Image {Source = new BitmapImage(new Uri(folder + "historic.png"))};
            Copy = new Image {Source = new BitmapImage(new Uri(folder + "copy.png"))};
            Config = new Image {Source = new BitmapImage(new Uri(folder + "config.png"))};
            Chat = new Image {Source = new BitmapImage(new Uri(folder + "chat.png"))};
            Link = new Image {Source = new BitmapImage(new Uri(folder + "link.png"))};
            BossGage = new Image {Source = new BitmapImage(new Uri(folder + "eye.png"))};
            HideNicknames = new Image { Source = new BitmapImage(new Uri(folder + "blur.png")) };
            Whisper = new Image { Source = new BitmapImage(new Uri(folder + "whisper.png")) };
            Info = new Image { Source = new BitmapImage(new Uri(folder + "info.png")) };
            DoneCircle = new Image { Source = new BitmapImage(new Uri(folder + "done_circle.png")) };
            Group = new Image { Source = new BitmapImage(new Uri(folder + "group.png")) };
            GroupAdd = new Image { Source = new BitmapImage(new Uri(folder + "group_add.png")) };
            Money = new Image { Source = new BitmapImage(new Uri(folder + "money.png")) };
            Credits = new Image { Source = new BitmapImage(new Uri(folder + "credits.png")) };
            Settings = new Image { Source = new BitmapImage(new Uri(folder + "settings.png")) };
            Performance = new Image { Source = new BitmapImage(new Uri(folder + "performance.png")) };
            Action = new Image { Source = new BitmapImage(new Uri(folder + "action.png")) };
            Links = new Image { Source = new BitmapImage(new Uri(folder + "links.png")) };

            Excel = new Image { Source = new BitmapImage(new Uri(folder + "excel.png")) };
            Upload = new Image { Source = new BitmapImage(new Uri(folder + "upload.png")) };
            Reset = new Image { Source = new BitmapImage(new Uri(folder + "reset.png")) };

            GitHub = new Image { Source = new BitmapImage(new Uri(folder + "github.png")) };
            Discord = new Image { Source = new BitmapImage(new Uri(folder + "discord.png")) };
            Cloud = new Image { Source = new BitmapImage(new Uri(folder + "cloud.png")) };
            SiteExport = new Image { Source = new BitmapImage(new Uri(folder + "site_export.png")) };

            AggroTime = new Image { Source = new BitmapImage(new Uri(folder + "eye_time.png")) };
            Skull = new Image { Source = new BitmapImage(new Uri(folder + "skull.png")) };
            SkullTime = new Image { Source = new BitmapImage(new Uri(folder + "skull_time.png")) };

            Delete = new Image { Source = new BitmapImage(new Uri(folder + "delete.png")) };

            Play = new Image { Source = new BitmapImage(new Uri(folder + "play.png")) };
            Pause = new Image { Source = new BitmapImage(new Uri(folder + "pause.png")) };

            Icon = new BitmapImage(new Uri(folder + "shinra.ico"));

            Tray = new Icon(folder + "shinra.ico");
        }

        public BitmapImage Icon { get; }
        public Image Chrono { get; }
        public Image Link { get; }

        public Image Copy { get; }
        public Image Config { get; }
        public Image Chat { get; }
        public Image BossGage { get; }
        public Image HideNicknames { get; }

        public Image History { get; }
        public Image Close { get; }

        public Image Chronobar { get; }

        public Image EntityStats { get; }
        public Image EntityStatsClickThrou { get; }

        public Image Whisper { get; }
        public Image Info { get; }
        public Image DoneCircle { get; }
        public Image Group { get; }
        public Image GroupAdd { get; }
        public Image Money { get; }
        public Image Credits { get; }

        public Image Links { get; }
        public Image Performance { get; }
        public Image Action { get; }
        public Image Settings { get; }

        public Image Excel { get; }
        public Image Upload { get; }
        public Image Reset { get; }
        public Image GitHub { get; }
        public Image Discord { get; }
        public Image Cloud { get; }
        public Image SiteExport { get; }

        public Image Skull { get; }
        public Image SkullTime { get; }
        public Image AggroTime { get; }

        public Image Delete { get; }

        public Image Play { get; }
        public Image Pause { get; }

        public Icon Tray { get; }
    }
}