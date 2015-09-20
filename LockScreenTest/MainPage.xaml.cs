using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LockScreenTest.Resources;
using Windows.Phone.System.LockScreenExtensibility;
using Windows.Phone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Media;
using Facet_Lockscreen_Bridge;
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace LockScreenTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        char[] space = new char[] { ' ' };

        IsolatedStorageFile bg = IsolatedStorageFile.GetUserStoreForApplication();
        LockScreenSnapshot lockInfo;
        TextBlock[] txtDetailedTexts;
        Image[] imgBadges;
        TextBlock[] txtBadges;

        MemoryStream buffer, buf2;
        int framePtr = 0;
        BitmapImage bmp;

        MediaElement me = new MediaElement();
        double gestureLength = 0.2;
        double y, yToUnlock = 400;
        DispatcherTimer timer = new DispatcherTimer(), videoTimer = new DispatcherTimer();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            if (IsolatedStorageSettings.ApplicationSettings.Contains("GestureLength"))
            {
                gestureLength = (double)IsolatedStorageSettings.ApplicationSettings["GestureLength"];
            }

            if (System.IO.File.Exists("D:\\Background.mjpg"))
            {
                FileStream file = new FileStream("D:\\Background.mjpg", FileMode.Open, FileAccess.Read);
                buffer = new MemoryStream((int)file.Length);
                file.CopyTo(buffer);
                file.Close();
                bmp = new BitmapImage();
                bmp.SetSource(buffer);
                BackgroundImage.Source = bmp;

                videoTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                videoTimer.Tick += videoTimer_Tick;
                videoTimer.Start();
            }
            else
            {
                if (bg.FileExists("Background.jpg"))
                {
                    IsolatedStorageFileStream stream = bg.OpenFile("Background.jpg", System.IO.FileMode.Open, FileAccess.Read);
                    bmp = new BitmapImage();
                    bmp.SetSource(stream);
                    BackgroundImage.Source = bmp;
                    stream.Close();
                }
            }

            txtDetailedTexts = new TextBlock[] { txtDetailedText1, txtDetailedText2, txtDetailedText3 };
            imgBadges = new Image[] { imgBadge1, imgBadge2, imgBadge3, imgBadge4, imgBadge5 };
            txtBadges = new TextBlock[] { txtBadge1, txtBadge2, txtBadge3, txtBadge4, txtBadge5 };

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += timer_Tick;
            timer.Start();

            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
            MediaPlayer.ActiveSongChanged += MediaPlayer_ActiveSongChanged;

            if (MediaPlayer.State != MediaState.Stopped)
            {
                Artist.Text = MediaPlayer.Queue.ActiveSong.Artist.Name;
                SongName.Text = MediaPlayer.Queue.ActiveSong.Name;
            }
            else
            {
                PlayPanel.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (MediaPlayer.IsRepeating)
            {
                btnLoop.Foreground = new SolidColorBrush((App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color);
            }
            else
            {
                btnLoop.Foreground = new SolidColorBrush((App.Current.Resources["PhoneInactiveBrush"] as SolidColorBrush).Color);
            }
            if (MediaPlayer.IsShuffled)
            {
                btnShuffle.Foreground = new SolidColorBrush((App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color);
            }
            else
            {
                btnShuffle.Foreground = new SolidColorBrush((App.Current.Resources["PhoneInactiveBrush"] as SolidColorBrush).Color);
            }
            timer_Tick(null, null);
        }

        void videoTimer_Tick(object sender, EventArgs e)
        {
            buffer.Position = framePtr + 2;
            for (; ; )
            {
                if (buffer.Position == buffer.Length)
                {
                    buffer.Position = 0;
                }
                if (buffer.ReadByte() == 0xFF)
                {
                    if (buffer.ReadByte() == 0xD8)
                    {
                        buffer.Position -= 2;
                        break;
                    }
                    else
                    {
                        buffer.Position--;
                    }
                }
            }
            if (buffer.Position >= (buffer.Length - 2))
            {
                buffer.Position = 0;
            }
            framePtr = (int)buffer.Position;
            buf2 = new MemoryStream((int)(buffer.Length - framePtr));
            buffer.CopyTo(buf2);
            buf2.Position = 0;
            bmp.SetSource(buf2);
        }

        void MediaPlayer_ActiveSongChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.Queue.Count > 0)
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        Artist.Text = MediaPlayer.Queue.ActiveSong.Artist.Name;
                        SongName.Text = MediaPlayer.Queue.ActiveSong.Name;
                    });
            }
        }

        void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            Visibility v;
            Binding b = new Binding();
            if (MediaPlayer.State == MediaState.Playing)
            {
                b.Source = "M 0,0 L 0,30 L 7,30 L 7,0 M 14,0 L 14,30 L 21,30 L 21,0";
            }
            else
            {
                b.Source = "M 0,0 L 15,15 L 0,30";
            }
            if (MediaPlayer.State == MediaState.Stopped)
            {
                v = System.Windows.Visibility.Collapsed;
            }
            else
            {
                v = System.Windows.Visibility.Visible;
            }
            Dispatcher.BeginInvoke(() =>
                {
                    BindingOperations.SetBinding(PlayPausePath, System.Windows.Shapes.Path.DataProperty, b);
                    PlayPanel.Visibility = v;
                    if (MediaPlayer.IsRepeating)
                    {
                        btnLoop.Foreground = new SolidColorBrush((App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color);
                    }
                    else
                    {
                        btnLoop.Foreground = new SolidColorBrush((App.Current.Resources["PhoneInactiveBrush"] as SolidColorBrush).Color);
                    }
                    if (MediaPlayer.IsShuffled)
                    {
                        btnShuffle.Foreground = new SolidColorBrush((App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color);
                    }
                    else
                    {
                        btnShuffle.Foreground = new SolidColorBrush((App.Current.Resources["PhoneInactiveBrush"] as SolidColorBrush).Color);
                    }
                });
        }

        private void Button_Unlock_Click(object sender, RoutedEventArgs e)
        {
            //((Button)sender).Content = "Yes";
            if (SystemProtection.ScreenLocked)
            {
                SystemProtection.RequestScreenUnlock();
            }
        }

        private void PlayNext(object sender, RoutedEventArgs e)
        {
            MediaPlayer.MoveNext();
        }

        private void PlayPause(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Pause();
            }
            else
            {
                MediaPlayer.Resume();
            }
        }

        private void PlayPrev(object sender, RoutedEventArgs e)
        {
            MediaPlayer.MovePrevious();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            var cont = Application.Current.Host.Content;
            LockScreenSnapshot newLockInfo = new LockScreenSnapshot((int)Math.Ceiling(cont.ActualWidth * cont.ScaleFactor / 100.0), (int)Math.Ceiling(cont.ActualHeight * cont.ScaleFactor / 100.0));
            if (LockInfoChanged(lockInfo, newLockInfo))
            {
                lockInfo = newLockInfo;
                //Badges
                if (lockInfo.HasTrayBadges)
                {
                    grdBadges.Visibility = System.Windows.Visibility.Visible;
                    for (int i = 0; i < 5; i++)
                    {
                        if (!String.IsNullOrEmpty(lockInfo.Badges[i].BadgeValue))
                        {
                            System.Windows.Media.Imaging.BitmapImage badge = new System.Windows.Media.Imaging.BitmapImage();
                            try
                            {
                                if (lockInfo.Badges[i].BadgeIcon != null)
                                {
                                    badge.SetSource(new MemoryStream(lockInfo.Badges[i].BadgeIcon));
                                }
                                else
                                {
                                    var str = lockInfo.Badges[i].BadgeIconURI.Substring(7);
                                    badge.SetSource(new FileStream(lockInfo.Badges[i].BadgeIconURI.Substring(7), FileMode.Open));
                                }
                            }
                            catch
                            {
                                badge.UriSource = new Uri("ms-appx:///Assets/BadgeLogo.png", UriKind.Absolute);
                            }
                            imgBadges[i].Source = badge;
                            txtBadges[i].Text = lockInfo.Badges[i].BadgeValue;
                            imgBadges[i].Visibility = System.Windows.Visibility.Visible;
                            txtBadges[i].Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            imgBadges[i].Visibility = System.Windows.Visibility.Collapsed;
                            txtBadges[i].Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    grdBadges.Visibility = System.Windows.Visibility.Collapsed;
                }
                //Detailed texts
                for (int i = 0; i < lockInfo.DetailedTexts.Count(); i++)
                {
                    if (!String.IsNullOrEmpty(lockInfo.DetailedTexts[i].Text))
                    {
                        txtDetailedTexts[i].Text = lockInfo.DetailedTexts[i].Text;
                        txtDetailedTexts[i].FontWeight = (lockInfo.DetailedTexts[i].IsBold) ? FontWeights.Bold : FontWeights.Normal;
                        txtDetailedTexts[i].Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        txtDetailedTexts[i].Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
            string[] TimeAMPM = DateTime.Now.ToShortTimeString().Split(space, 2, StringSplitOptions.RemoveEmptyEntries);
            HourText.Text = TimeAMPM[0];
            AMPM.Text = TimeAMPM.Count() > 1 ? TimeAMPM[1] : "";
            DatePanel.Text = DateTime.Now.ToShortDateString();
            FrameworkDispatcher.Update();
        }

        private void ButtonStopClick(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
        }

        private void OverlayInformationPanel_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            y = e.CumulativeManipulation.Translation.Y;
            OverlayInformationPanelTranslateTransform.Y = (y < 0.0) ? y : 0.0;
        }

        private void OverlayInformationPanel_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (SystemProtection.ScreenLocked && y < yToUnlock)
            {
                SystemProtection.RequestScreenUnlock();
            }
            else
            {
                OverlayInformationPanelDraggingToLockedStoryboard.Begin();
            }
        }

        private void OverlayInformationPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            yToUnlock = -e.NewSize.Height * gestureLength;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!SystemProtection.ScreenLocked)
            {
                ((App)App.Current).wasLocked = false;
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            }
            else
            {
                ((App)App.Current).wasLocked = true;
                FadeInAnimation.Begin();
            }

            base.OnNavigatedTo(e);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Do nothing
            e.Cancel = true;
        }

        bool LockInfoChanged(LockScreenSnapshot prev, LockScreenSnapshot cur)
        {
            if (prev == null) return true;
            if (prev.Badges.Count() != cur.Badges.Count())
            {
                return true;
            }
            else
            {
                for (int i = 0; i < prev.Badges.Count(); i++)
                {
                    if (prev.Badges[i].BadgeIconURI != cur.Badges[i].BadgeIconURI) return true;
                    if (prev.Badges[i].BadgeValue != cur.Badges[i].BadgeValue) return true;
                }
            }
            if (prev.DetailedTexts.Count() != cur.DetailedTexts.Count())
            {
                return true;
            }
            else
            {
                for (int i = 0; i < prev.DetailedTexts.Count(); i++)
                {
                    if (prev.DetailedTexts[i].IsBold != cur.DetailedTexts[i].IsBold) return true;
                    if (prev.DetailedTexts[i].Text != cur.DetailedTexts[i].Text) return true;
                }
            }
            // TODO: more
            return false;
        }

        private void Loop(object sender, RoutedEventArgs e)
        {
            MediaPlayer.IsRepeating = !MediaPlayer.IsRepeating;
            if (MediaPlayer.IsRepeating)
            {
                btnLoop.Foreground = new SolidColorBrush((App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color);
            }
            else
            {
                btnLoop.Foreground = new SolidColorBrush((App.Current.Resources["PhoneInactiveBrush"] as SolidColorBrush).Color);
            }
        }

        private void Shuffle(object sender, RoutedEventArgs e)
        {
            MediaPlayer.IsShuffled = !MediaPlayer.IsShuffled;
            if (MediaPlayer.IsShuffled)
            {
                btnShuffle.Foreground = new SolidColorBrush((App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color);
            }
            else
            {
                btnShuffle.Foreground = new SolidColorBrush((App.Current.Resources["PhoneInactiveBrush"] as SolidColorBrush).Color);
            }
        }

        private void BackVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Position = TimeSpan.Zero;
            (sender as MediaElement).Play();
        }
    }
}