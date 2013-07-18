// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Interop;

    using DoubanMusicDownloader.Properties;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using MessageBox = System.Windows.MessageBox;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Static Fields

        /// <summary>
        /// The task count.
        /// </summary>
        private static int TaskCount;

        #endregion

        #region Fields

        /// <summary>
        /// The background worker.
        /// </summary>
        private readonly BackgroundWorker bw = new BackgroundWorker { WorkerSupportsCancellation = true };

        /// <summary>
        /// The download folder.
        /// </summary>
        private string DownloadFolder = string.Empty;

        /// <summary>
        /// The music db.
        /// </summary>
        private Dictionary<string, Music> MusicDB;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.DownloadingList = new MusicList();

            this.InitializeComponent();
            this.DataContext = this;

            this.btnDownload.IsEnabled = true;
            this.btnCancel.IsEnabled = false;
            this.LoadDownloadHistory();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the channels.
        /// </summary>
        public List<Channel> Channels
        {
            get
            {
                return new List<Channel>
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

        /// <summary>
        /// Gets or sets the downloading list.
        /// </summary>
        public MusicList DownloadingList { get; set; }

        /// <summary>
        /// Gets or sets the selected channel.
        /// </summary>
        public int SelectedChannel { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The button cancel on click event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.bw.CancelAsync();
        }

        /// <summary>
        /// The button clean history on click event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnCleanHistory_OnClick(object sender, RoutedEventArgs e)
        {
            this.MusicDB.Clear();
            this.SaveDownloadHistory();
        }

        /// <summary>
        /// The button download on click event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnDownload_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog { ShowNewFolderButton = true };
            var source = PresentationSource.FromVisual(this) as HwndSource;
            var win = new OldWindow(source.Handle);
            if (dlg.ShowDialog(win) != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            this.DownloadFolder = dlg.SelectedPath;

            this.bw.DoWork += (o, arg) =>
                {
                    while (!this.bw.CancellationPending)
                    {
                        this.DownloadMusic();
                        if (this.DownloadingList.Count == 0)
                        {
                            try
                            {
                                this.RefreshList();
                            }
                            catch (Exception ex)
                            {
                                this.Dispatcher.Invoke(new Action(() => MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)));
                                break;
                            }
                        }

                        Thread.Sleep(100);
                    }

                    this.Dispatcher.Invoke(
                        new Action(
                            () =>
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

        /// <summary>
        /// Download music.
        /// </summary>
        private void DownloadMusic()
        {
            for (int i = 0; i < this.DownloadingList.Count; i++)
            {
                while (TaskCount > Settings.Default.TaskCount)
                {
                    Thread.Sleep(100);
                }

                Music music = this.DownloadingList[i];
                var client = new WebClient();
                client.DownloadDataCompleted += (o, e) =>
                    {
                        try
                        {
                            byte[] raw = e.Result;
                            Directory.CreateDirectory(this.DownloadFolder);
                            using (
                                var fs = new FileStream(
                                    Path.Combine(this.DownloadFolder, music.FileName), FileMode.Create))
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

            this.Dispatcher.Invoke(new Action(() => this.DownloadingList.Clear()));
        }

        /// <summary>
        /// Get music list.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private List<Music> GetMusicList()
        {
            var list = new List<Music>();

            try
            {
                WebRequest request = WebRequest.Create(
                    string.Format(Settings.Default.DoubanFMUrl, this.SelectedChannel));
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Timeout = 5000;
                WebResponse response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    dynamic musicList = JObject.Parse(json);

                    foreach (dynamic mu in musicList.song)
                    {
                        var music = new Music
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
            }
            catch (WebException e)
            {
                throw e;
            }
            catch
            {
                // ignore exception on get response and parse json data
            }

            return list;
        }

        /// <summary>
        /// Load download history.
        /// </summary>
        private void LoadDownloadHistory()
        {
            // Load stored download list
            using (var fs = new FileStream("musicdb", FileMode.OpenOrCreate, FileAccess.Read))
            {
                try
                {
                    using (TextReader tr = new StreamReader(fs))
                    {
                        object c = JsonConvert.DeserializeObject(
                            tr.ReadToEnd(),
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All,
                                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                            });
                        this.MusicDB = (Dictionary<string, Music>)c;
                    }
                }
                catch (Exception)
                {
                    this.MusicDB = new Dictionary<string, Music>();
                }
            }
        }

        /// <summary>
        /// Refresh list.
        /// </summary>
        private void RefreshList()
        {
            this.Dispatcher.Invoke(new Action(() => this.DownloadingList.Clear()));

            var musicList = this.GetMusicList();
            foreach (Music music in musicList)
            {
                if (!this.MusicDB.ContainsKey(music.FileName))
                {
                    this.MusicDB.Add(music.FileName, music);

                    this.Dispatcher.Invoke(new Action(() => this.DownloadingList.Add(music)));
                }
            }

            this.SaveDownloadHistory();
        }

        /// <summary>
        /// The save download history.
        /// </summary>
        private void SaveDownloadHistory()
        {
            using (var fs = new FileStream("musicdb", FileMode.Create, FileAccess.Write))
            {
                string json = JsonConvert.SerializeObject(
                    this.MusicDB,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                    });
                using (TextWriter tw = new StreamWriter(fs))
                {
                    tw.Write(json);
                }
            }
        }

        #endregion
    }
}