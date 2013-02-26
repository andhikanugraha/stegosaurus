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
    }
}
