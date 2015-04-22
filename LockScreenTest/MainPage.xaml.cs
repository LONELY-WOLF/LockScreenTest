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
        IsolatedStorageFile bg = IsolatedStorageFile.GetUserStoreForApplication();
        //LockScreenInfoProvider infoProvider = new LockScreenInfoProvider();
        //DeviceLockscreenSnapshot info;
        LockScreenSnapshot lockInfo;
        TextBlock[] txtDetailedTexts;
        Image[] imgBadges;
        TextBlock[] txtBadges;

        MediaElement me = new MediaElement();

        CompositeTransform transform = new CompositeTransform();
        double y, vy, yToUnlock = 400;
        string hours = "", date = "";
        DispatcherTimer timer = new DispatcherTimer(), phyTimer = new DispatcherTimer();

        //System.Threading.Timer xnaTimer = new System.Threading.Timer(new System.Threading.TimerCallback(xnaTimerProc), null, 0, 100);
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            ImageBrush backPic = new ImageBrush();
            VideoBrush backVid;

            //string tpmdf = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (System.IO.File.Exists("D:\\Background.mp4") && MediaPlayer.Queue.Count <= 0)
            {
                MediaElement md = new MediaElement();
                md.Name = "BackVideo";
                md.IsMuted = true;
                md.IsHitTestVisible = false;
                md.Opacity = 0.0;
                md.MediaEnded += BackVideo_MediaEnded;
                LayoutRoot.Children.Add(md);
                md.Source = new Uri("D:\\Background.mp4", UriKind.Absolute);
                backVid = new VideoBrush();
                backVid.SourceName = "BackVideo";
                backVid.Stretch = Stretch.UniformToFill;
                BackgroundImage.Background = backVid;
            }
            else
            {
                //if (File.Exists("C:\\Data\\Users\\DefApps\\APPDATA\\Local\\Packages\\86dd9094-396f-4cd6-b128-9dfbf7c5808d_48p39djvdh4am\\LocalState\\Background.jpg"))
                if (bg.FileExists("Background.jpg"))
                {
                    IsolatedStorageFileStream stream = bg.OpenFile("Background.jpg", System.IO.FileMode.Open, FileAccess.Read);
                    BitmapImage bmp = new BitmapImage();
                    bmp.SetSource(stream);
                    //backPic.ImageSource = new BitmapImage(new Uri("C:\\Data\\Users\\Public\\Background.jpg", UriKind.Absolute));
                    backPic.ImageSource = bmp;
                    BackgroundImage.Background = backPic;
                    stream.Close();
                }
            }

            txtDetailedTexts = new TextBlock[] { txtDetailedText1, txtDetailedText2, txtDetailedText3 };
            imgBadges = new Image[] { imgBadge1, imgBadge2, imgBadge3, imgBadge4, imgBadge5 };
            txtBadges = new TextBlock[] { txtBadge1, txtBadge2, txtBadge3, txtBadge4, txtBadge5 };

            OverlayInformationPanel.RenderTransform = transform;

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += timer_Tick;
            timer.Start();

            phyTimer.Interval = new TimeSpan(0, 0, 0, 0, 15);
            phyTimer.Tick += phyTimer_Tick;

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
            LockScreenSnapshot newLockInfo = new LockScreenSnapshot(720, 1280);
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
                            if (lockInfo.Badges[i].BadgeIcon != null)
                            {
                                badge.SetSource(new MemoryStream(lockInfo.Badges[i].BadgeIcon));
                            }
                            else
                            {
                                var str = lockInfo.Badges[i].BadgeIconURI.Substring(7);
                                badge.SetSource(new FileStream(lockInfo.Badges[i].BadgeIconURI.Substring(7), FileMode.Open));
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
            if (hours != DateTime.Now.ToString("HH:mm"))
            {
                hours = DateTime.Now.ToString("HH:mm");
                HourText.Text = hours;
            }
            if (date != DateTime.Now.ToString("dd/MM/yy"))
            {
                date = DateTime.Now.ToString("dd/MM/yy");
                DatePanel.Text = date;
            }
            FrameworkDispatcher.Update();
        }

        private void ButtonStopClick(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
        }

        private void OverlayInformationPanel_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            y = e.CumulativeManipulation.Translation.Y;
            vy = e.Velocities.LinearVelocity.Y;
            transform.TranslateY = (y < 0.0) ? y : 0.0;
            //OverlayInformationPanel.RenderTransform = transform;
        }

        private void OverlayInformationPanel_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (SystemProtection.ScreenLocked && y < yToUnlock)
            {
                SystemProtection.RequestScreenUnlock();
            }
            else
            {
                phyTimer.Start();
            }
        }

        void phyTimer_Tick(object sender, EventArgs e)
        {
            vy += 1.0;
            y += vy;
            if (y > 0) //Down end
            {
                transform.TranslateY = 0.0;
                phyTimer.Stop();
                return;
            }
            if (y < yToUnlock)
            {
                phyTimer.Stop();
                if (SystemProtection.ScreenLocked)
                {
                    SystemProtection.RequestScreenUnlock();
                }
                return;
            }
            transform.TranslateY = y;
        }

        private void OverlayInformationPanel_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            phyTimer.Stop();
        }

        private void OverlayInformationPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            yToUnlock = -e.NewSize.Height * 0.2;
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