using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using Stegosaurus;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void chooseSourceVideo(object sender, RoutedEventArgs e)
        {
            // See MSDN article
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "Video"; // Default file name
            dlg.DefaultExt = ".avi"; // Default file extension
            dlg.Filter = "AVI Videos|*.avi"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                textBoxSourceVideo.Text = filename;
            }
        }

        private void chooseSourceMessage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "Message"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Documents|*.*"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                textBoxSourceMessage.Text = filename;
            }
        }

        private void loadSourceVideo(object sender, RoutedEventArgs e)
        {
            videoPlayer.Source = new Uri(textBoxSourceVideo.Text);
            // videoPlayer.Play();
        }

        private void alert(string message)
        {
            string messageBoxText = message;
            string caption = "Stegosaurus";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        private void onClickButtonEncrypt(object sender, RoutedEventArgs e)
        {
            string sourceMessageFileName = textBoxSourceMessage.Text;
            string sourceVideoFileName = textBoxSourceVideo.Text;
            string key = textBoxKey.Text;
            int LSB = Convert.ToBoolean(radioButton1bit.IsChecked) ? 1 : 2;

            if (sourceMessageFileName.Length == 0) {
                alert("Please specify a source message file.");
            }
            else if (sourceVideoFileName.Length == 0)
            {
                alert("Please specify a source video file.");
            }
            else if (key.Length == 0)
            {
                alert("Please specify a key.");
            }
            else if (!File.Exists(sourceMessageFileName))
            {
                alert("Source message file not found.");
            }
            else if (!File.Exists(sourceVideoFileName))
            {
                alert("Source video file not found.");
            }
            else
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = "Video"; // Default file name
                dlg.DefaultExt = ".avi"; // Default file extension
                dlg.Filter = "AVI Videos|*.avi"; // Filter files by extension 
                dlg.Title = "Select Output Video File";

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    // Everything valid here. Begin with encryption.

                    // Fill Engine with variables.
                    Engine.SourceMessageFileName = sourceMessageFileName;
                    Engine.SourceVideoFileName = sourceVideoFileName;
                    Engine.Key = key;
                    Engine.LsbMode = LSB;

                    Engine.OutputVideoFileName = dlg.FileName;

                    Engine.EncryptAndSave();
                }
                else
                {
                    alert("Output file not specified. Abort operation.");
                }
            }
        }


        private void onClickButtonDecrypt(object sender, RoutedEventArgs e)
        {
            string sourceMessageFileName = textBoxSourceMessage.Text;
            string sourceVideoFileName = textBoxSourceVideo.Text;
            string key = textBoxKey.Text;
            int LSB = Convert.ToBoolean(radioButton1bit.IsChecked) ? 1 : 2;

            if (sourceVideoFileName.Length == 0)
            {
                alert("Please specify a source video file.");
            }
            else if (key.Length == 0)
            {
                alert("Please specify a key.");
            }
            else if (!File.Exists(sourceVideoFileName))
            {
                alert("Source video file not found.");
            }
            else
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = sourceMessageFileName; // Default file name
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "*.*"; // Filter files by extension 
                dlg.Title = "Select Output Message File";

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    // Everything valid here. Begin with encryption.

                    // Fill Engine with variables.
                    Engine.SourceVideoFileName = sourceVideoFileName;
                    Engine.Key = key;
                    Engine.LsbMode = LSB;

                    Engine.OutputMessageFileName = dlg.FileName;

                    Engine.DecryptAndSave();
                }
                else
                {
                    alert("Output file not specified. Abort operation.");
                }
            }
        }

    }
}
