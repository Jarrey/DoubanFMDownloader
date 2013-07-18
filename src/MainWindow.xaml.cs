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

namespace DoubanMusicDownloader
{
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Interop;
    using System.Windows.Threading;
    using System.Xml.Serialization;

    using DoubanMusicDownloader.Properties;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int TaskCount = 0;

        private string DownloadFolder = string.Empty;
        private List<Music> MusicDB;
        private BackgroundWorker bw = new BackgroundWorker()
                                          {
                                              WorkerSupportsCancellation = true
                                          };

        public MusicList DownloadingList { get; set; }
        public int SelectedChannel { get; set; }

        public List<Channel> Channels
        {
            get
            {
                return new List<Channel>()
                           {
                               new Channel(0, "公共 MHz"),
                               new Channel(1, "华语 MHz"),
                               new Channel(2, "欧美 MHz"),
                               new Channel(3, "70 MHz"),
                               new Channel(4, "80 MHz"),
                               new Channel(5, "90 MHz"),
                               new Channel(6, "粤语 MHz"),
                               new Channel(7, "摇滚 MHz"),
                               new Channel(8, "民谣 MHz"),
                               new Channel(9, "轻音乐 MHz"),
                               new Channel(10, "电影原声 MHz")
                           };
            }
        }

        public MainWindow()
        {
            this.DownloadingList = new MusicList();

            this.InitializeComponent();
            this.DataContext = this;

            this.btnDownload.IsEnabled = true;
            this.btnCancel.IsEnabled = false;
            this.LoadDownloadHistory();
        }

        #region Event Handlers

        private void BtnDownload_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true
            };
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            var win = new OldWindow(source.Handle);
            if (dlg.ShowDialog(win) != System.Windows.Forms.DialogResult.OK) return;
            DownloadFolder = dlg.SelectedPath;


            this.bw.DoWork += (o, arg) =>
            {
                while (!bw.CancellationPending)
                {
                    DownloadMusic();
                    if (this.DownloadingList.Count == 0) RefleshList();
                    Thread.Sleep(100);
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.btnCleanHistory.IsEnabled = true;
                    this.btnDownload.IsEnabled = true;
                    this.btnCancel.IsEnabled = false;
                }));
            };
            this.btnCancel.IsEnabled = true;
            this.btnCleanHistory.IsEnabled = false;
            this.btnDownload.IsEnabled = false;
            this.bw.RunWorkerAsync();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.bw.CancelAsync();
        }

        private void BtnCleanHistory_OnClick(object sender, RoutedEventArgs e)
        {
            this.MusicDB.Clear();
            this.SaveDownloadHistory();
        }

        #endregion

        private void DownloadMusic()
        {
            for (int i = 0; i < this.DownloadingList.Count; i++)
            {
                while (TaskCount > Settings.Default.TaskCount)
                {
                    Thread.Sleep(100);
                }

                Music music = this.DownloadingList[i];
                WebClient client = new WebClient();
                client.DownloadDataCompleted += (o, e) =>
                    {
                        try
                        {
                            byte[] raw = e.Result;
                            Directory.CreateDirectory(this.DownloadFolder);
                            using (var fs = new FileStream(Path.Combine(this.DownloadFolder, music.FileName), FileMode.Create))
                            {
                                fs.Write(raw, 0, raw.Length);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            TaskCount--;
                        }

                        if (client != null)
                        {
                            client.Dispose();
                        }
                    };
                client.DownloadProgressChanged += (o, e) => { music.Progress = e.ProgressPercentage; };
                client.DownloadDataAsync(new Uri(music.Url));
                TaskCount++;
            }

            while (TaskCount > 0)
            {
                Thread.Sleep(100);
            }

            this.Dispatcher.Invoke(new Action(() => { DownloadingList.Clear(); }));
        }

        private void RefleshList()
        {
            this.Dispatcher.Invoke(new Action(() => { DownloadingList.Clear(); }));
            foreach (var music in this.GetMusicLst())
            {
                if (!this.MusicDB.Exists(p => p.Equals(music)))
                {
                    this.MusicDB.Add(music);

                    this.Dispatcher.Invoke(new Action(() => { DownloadingList.Add(music); }));
                }
            }

            this.SaveDownloadHistory();
        }

        private void LoadDownloadHistory()
        {
            // Load stored download list
            using (var fs = new FileStream("musicdb.xml", FileMode.OpenOrCreate, FileAccess.Read))
            {
                var x = new XmlSerializer(typeof(List<Music>));
                try
                {
                    this.MusicDB = x.Deserialize(fs) as List<Music>;
                }
                catch (Exception)
                {
                    this.MusicDB = new List<Music>();
                }
            }
        }

        private void SaveDownloadHistory()
        {
            using (FileStream fs = new FileStream("musicdb.xml", FileMode.Create, FileAccess.Write))
            {
                var x = new XmlSerializer(typeof(List<Music>));
                x.Serialize(fs, this.MusicDB);
            }
        }

        private List<Music> GetMusicLst()
        {
            List<Music> list = new List<Music>();

            WebRequest request = WebRequest.Create(string.Format(Settings.Default.DoubanFMUrl, this.SelectedChannel));
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                string json = sr.ReadToEnd();
                dynamic musiList = JObject.Parse(json);

                foreach (dynamic mu in musiList.song)
                {
                    Music music = new Music()
                                      {
                                          Url = mu.url,
                                          AlbumPicture = mu.picture,
                                          AlbumTitle = mu.albumtitle,
                                          Artist = mu.artist,
                                          Title = mu.title,
                                          PublicTime = mu.public_time,
                                          Publisher = mu.company
                                      };
                    list.Add(music);
                }
            }

            return list;
        }
    }
}
