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
//using LockScreen_Bridge;

namespace LockScreenTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        //LockScreenInfoProvider infoProvider = new LockScreenInfoProvider();
        //DeviceLockscreenSnapshot info;

        CompositeTransform transform = new CompositeTransform();
        double y, vy, yToUnlock = 400;
        string hours = "", date = "";
        DispatcherTimer timer = new DispatcherTimer(), phyTimer = new DispatcherTimer();
        //System.Threading.Timer xnaTimer = new System.Threading.Timer(new System.Threading.TimerCallback(xnaTimerProc), null, 0, 100);
        // Constructor
        public MainPage()
        {
            InitializeComponent();

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
            Dispatcher.BeginInvoke(()=>
                {
                    BindingOperations.SetBinding(PlayPausePath, System.Windows.Shapes.Path.DataProperty, b);
                    PlayPanel.Visibility = v;
                });
        }

        private void Button_Unlock_Click(object sender, RoutedEventArgs e)
        {
            //((Button)sender).Content = "Yes";
            if(SystemProtection.ScreenLocked)
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
            if(MediaPlayer.State == MediaState.Playing)
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
            //infoProvider.GetSnapshot(info);
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
                NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
            }

            base.OnNavigatedTo(e);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Do nothing
            e.Cancel = true;
        }
    }
}