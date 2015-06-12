using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Phone.System.LockScreenExtensibility;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.IO;

namespace LockScreenTest
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        IsolatedStorageFile bg = IsolatedStorageFile.GetUserStoreForApplication();

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationService.RemoveBackEntry();

            base.OnNavigatedTo(e);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            string[] names = bg.GetFileNames();
            if (bg.FileExists("Background.jpg"))
            {
                IsolatedStorageFileStream stream = bg.OpenFile("Background.jpg", FileMode.Open, FileAccess.Read);
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(stream);
                bgImage.Source = bitmap;
                stream.Close();
            }
            useIt.IsChecked = ExtensibilityApp.IsLockScreenApplicationRegistered();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.Completed += chooser_Completed;
            var cont = Application.Current.Host.Content;
            chooser.PixelHeight = (int)Math.Ceiling(cont.ActualHeight * cont.ScaleFactor / 100.0);
            chooser.PixelWidth = (int)Math.Ceiling(cont.ActualWidth * cont.ScaleFactor / 100.0);
            chooser.Show();
        }

        void chooser_Completed(object sender, PhotoResult e)
        {
            if (e.ChosenPhoto != null)
            {
                IsolatedStorageFileStream stream = bg.CreateFile("Background.jpg");
                //FileStream stream = File.Create("C:\\Data\\Users\\Public\\Pictures\\Background.jpg");
                e.ChosenPhoto.CopyTo(stream);
                stream.Close();
                
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(e.ChosenPhoto);
                bgImage.Source = bitmap;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (bg.FileExists("Background.jpg"))
            {
                if (!ExtensibilityApp.IsLockScreenApplicationRegistered())
                {
                    ExtensibilityApp.RegisterLockScreenApplication();
                }
                useIt.IsChecked = ExtensibilityApp.IsLockScreenApplicationRegistered();
            }
            else
            {
                MessageBox.Show("Choose background first");
                useIt.IsChecked = false;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ExtensibilityApp.IsLockScreenApplicationRegistered())
            {
                ExtensibilityApp.UnregisterLockScreenApplication();
            }
            useIt.IsChecked = ExtensibilityApp.IsLockScreenApplicationRegistered();
        }
    }
}