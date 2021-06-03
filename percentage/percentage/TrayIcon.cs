using System;
using System.Configuration;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using percentage.Properties;
using WMPRichPreviewLauncher;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const string iconFont = "Jersey M54";
        private const int iconFontSize = 30;

        private string batteryPercentage;
        private NotifyIcon notifyIcon;
        private string batteryChargeStatus;
        private int count = 1;
        private string powerLineStatus;
        private DateTime lastExecutedTime = new DateTime(1900, 1, 1);

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            // initialize contextMenu
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            // initialize menuItem
            menuItem.Index = 0;
            menuItem.Text = "E&xit";
            menuItem.Click += new System.EventHandler(menuItem_Click);

            notifyIcon.ContextMenu = contextMenu;

            batteryPercentage = "?";

            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 1000; // in miliseconds
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            batteryPercentage = (powerStatus.BatteryLifePercent * 100).ToString();
            batteryChargeStatus = (powerStatus.BatteryChargeStatus).ToString();
            powerLineStatus = (powerStatus.PowerLineStatus).ToString();

            Color whiteColor = Color.White;
            Color greenColor = System.Drawing.ColorTranslator.FromHtml("#7CFC00");
            Color redColor = Color.Red;

            Color color = whiteColor;

            if (powerLineStatus.ToLower() == "online")
            {
                if (Int32.Parse(batteryPercentage) >= 80)
                {
                    TimeSpan duration = DateTime.Now - lastExecutedTime;

                    if (duration.TotalMinutes >= 5)
                    {                        
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(ConfigurationManager.AppSettings["HighBatterySound"]);
                        player.Play();
                        lastExecutedTime = DateTime.Now;
                    }
                }
                color = greenColor;
            }
            else if (batteryChargeStatus.ToString().ToLower().Contains("not charging"))
            {
                //Should use relative path or through resources.
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(ConfigurationManager.AppSettings["LowBatterySound"]);
                player.Play();
            }
            else
            {
                if (Int32.Parse(batteryPercentage) <= 30)
                {
                    TimeSpan duration = DateTime.Now - lastExecutedTime;

                    if (duration.TotalMinutes >= 5)
                    {
                        //Should use relative path or through resources.
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(ConfigurationManager.AppSettings["LowBatterySound"]);
                        player.Play();
                        lastExecutedTime = DateTime.Now;
                    }
                    color = redColor;
                }
                else
                {
                    color = whiteColor;
                }
            }

            using (Bitmap bitmap = new Bitmap(DrawText(batteryPercentage, new Font(iconFont, iconFontSize, FontStyle.Bold), color, Color.Transparent)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        notifyIcon.Text = batteryPercentage + "%";
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }

        private void menuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            var textSize = GetImageSize(text, font);
            //Harish Pen
            Image image = new Bitmap((int)textSize.Width, (int)textSize.Height);
            //Image image = new Bitmap((int) Math.Max(textSize.Width, textSize.Height), (int)Math.Max(textSize.Width, textSize.Height));
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // paint the background
                graphics.Clear(backColor);

                // create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.DrawString(text, font, textBrush, 0, 0);
                    graphics.Save();
                }
            }

            return image;
        }

        private static SizeF GetImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }
    }
}
