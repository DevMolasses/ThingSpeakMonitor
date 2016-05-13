using System;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace ThingSpeakMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bw = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += BackgroundWorkerOnDoWork;
            bw.RunWorkerAsync();
        }

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs args)
        {
            while (!bw.CancellationPending)
            {
                MonitorThingSpeakChannel();  // Get information from thingspeak and display it
                Debug.WriteLine("Background Thread Sleep");
                Thread.Sleep(15000); // Wait 15 seconds before querying thinkgspeak again
                Debug.WriteLine("Background Thread Wake");
            }
        }

        private void MonitorThingSpeakChannel()
        {
            Debug.WriteLine("Enter MonitorThingSpeakChannel");
            using (WebClient wc = new WebClient())
            {
                Debug.WriteLine("Get JSON string from ThingSpeak");
                var json = wc.DownloadString("http://api.thingspeak.com/channels/81844/feed/last.json");
                Debug.WriteLine("Get the status of the feed");
                bool status = getFeedStatus(json);
                Debug.WriteLine("Update the indicator");
                updateStatusIndicator(status);
            }
            Debug.WriteLine("Exit MonitorThingSpeakChannel");
        }

        private void updateStatusIndicator(bool status)
        {
            Debug.WriteLine("Enter updateStatusIndicator");
            if (status)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Background = new SolidColorBrush(Color.FromRgb(18, 232, 18));
                    this.Icon = new BitmapImage(new Uri("Check.ico", UriKind.Relative));
                }));
                
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.Background = new SolidColorBrush(Color.FromRgb(232, 18, 18));
                    this.Icon = new BitmapImage(new Uri("Error.ico", UriKind.Relative));
                }));
            }
            Debug.WriteLine("Exit updateStatusIndicator");
        }

        /// <summary>
        /// Gets the time stamp from a thingspeak.com channel
        /// </summary>
        /// <param name="json">The json string as retrieved form thingspeak.com</param>
        /// <returns>Whether or not the given string timestamp is in the last 5 minutes</returns>
        private bool getFeedStatus(string json)
        {
            // Get the date string from the json string.  Assumes the date is two charachters after the first ':'
            var startOfDate = json.IndexOf(':') + 2;
            string dateString = json.Substring(startOfDate, 20);

            //Convert the string to DateTime
            DateTime dt = Convert.ToDateTime(dateString);

            //Get the difference between Now and the date string
            TimeSpan timeDifference = DateTime.Now - dt;

            //Display date time on MainWindow
            this.Dispatcher.Invoke((Action)(() =>
                {
                    lastEntryLabel.Content = "Last Entry:\n" + dt.ToString();
                }));
            

            //Return True if the difference is less than 5 minutes
            return timeDifference.Minutes < 5;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("Cancel Background Worker");
            bw.CancelAsync();
        }
    }
}
