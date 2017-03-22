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

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using TagLib;

    using MessageBox = System.Windows.MessageBox;
    using System.Collections.Concurrent;
    using Microsoft.Win32;
    using Properties;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Static Fields

        /// <summary>
        /// The task count.
        /// </summary>
        private static int TaskCount = 0;

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
        private List<string> MusicDB;

        private ConcurrentDictionary<string, string> MimeTypeToExtension = new ConcurrentDictionary<string, string>();
        
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

            this.LoadChannels();
            this.LoadDownloadHistory();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the downloading list.
        /// </summary>
        public MusicList DownloadingList { get; set; }

        public ObservableCollection<Channel> Channels { get; private set; }

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
            var dlg = new FolderBrowserDialog { ShowNewFolderButton = true, SelectedPath = DownloadFolder };
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
                                this.bw.CancelAsync();
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

        private void LoadChannels()
        {
            if (Channels == null)
            {
                Channels = new ObservableCollection<Channel>();
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.Default.ChannelsUrl);
                request.Method = "GET";
                request.UserAgent = Settings.Default.UserAgent;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Timeout = 30000;  // time out, 10s

                // get the channel list from douban.fm
                // try Settings.ReDo times
                int round = 1;
                WebResponse response = null;
                while (true)
                {
                    try
                    {
                        response = request.GetResponse();
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (round > Settings.Default.ReDo) throw ex;
                        round++;
                    }
                }

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    dynamic channelcList = JObject.Parse(json);

                    foreach (dynamic gp in channelcList.groups)
                    {
                        foreach (dynamic ch in gp.chls)
                        {
                            var channel = new Channel(ch);
                            Channels.Add(channel);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                // ignore exception on get response and parse json data
            }
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
                    Thread.Sleep(500);
                }

                TaskCount++;
                Music music = this.DownloadingList[i];
                var client = new WebClient();
                client.DownloadDataCompleted += (o, e) =>
                    {
                        try
                        {
                            WebClient webClient = o as WebClient;
                            if (webClient != null)
                            {
                                string extension = this.ConvertMimeTypeToExtension(webClient.ResponseHeaders["Content-Type"]);
                                byte[] raw = e.Result;
                                Directory.CreateDirectory(this.DownloadFolder);
                                string filePath = Path.Combine(this.DownloadFolder, music.FileName + extension);
                                using (var fs = new FileStream(filePath, FileMode.Create))
                                {
                                    fs.Write(raw, 0, raw.Length);
                                }

                                this.SetMusicTag(music, filePath);
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
            }

            while (TaskCount > 0)
            {
                Thread.Sleep(100);
            }

            this.Dispatcher.Invoke(new Action(() => this.DownloadingList.Clear()));
        }

        /// <summary>
        /// The set music tag.
        /// </summary>
        /// <param name="music">
        /// The music.
        /// </param>
        /// <param name="musicFile">
        /// The music file.
        /// </param>
        private void SetMusicTag(Music music, string musicFile)
        {
            try
            {
                using (TagLib.File f = TagLib.File.Create(musicFile))
                {
                    f.Tag.Title = music.Title;
                    f.Tag.Album = music.AlbumTitle;
                    f.Tag.Year = music.PublicTime;
                    f.Tag.Performers = music.Artist.Split('_');

                    var client = new WebClient();
                    var pictureData = new ByteVector(client.DownloadData(music.AlbumPicture));
                    f.Tag.Pictures = new IPicture[] { new Picture(pictureData) { Type = PictureType.FrontCover } };
                    f.Save();
                }
            }
            catch
            {
                // ignore exception on get response and parse json data
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
                if (!this.MusicDB.Contains(music.FileName))
                {
                    this.MusicDB.Add(music.FileName);

                    this.Dispatcher.Invoke(new Action(() => this.DownloadingList.Add(music)));
                }
            }

            this.SaveDownloadHistory();
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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(Settings.Default.DoubanFMUrl, this.SelectedChannel));
                request.Method = "GET";
                request.UserAgent = Settings.Default.UserAgent;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Timeout = 30000;  // time out, 10s

                // get the music list from douban.fm
                // try Settings.ReDo times
                int round = 1;
                WebResponse response = null;
                while (true)
                {
                    try
                    {
                        response = request.GetResponse();
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (round > Settings.Default.ReDo) throw ex;
                        round++;
                    }
                }

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string json = sr.ReadToEnd();
                    dynamic musicList = JObject.Parse(json);

                    foreach (dynamic mu in musicList.song)
                    {
                        var music = new Music(mu);
                        list.Add(music);
                    }
                }
            }
            catch (WebException e)
            {
                throw e;
            }
            catch (Exception e)
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
            try
            {
                // Load stored download list
                using (var fs = new FileStream("musicdb", FileMode.OpenOrCreate, FileAccess.Read))
                {
                    using (TextReader tr = new StreamReader(fs))
                    {
                        object c = JsonConvert.DeserializeObject(tr.ReadToEnd());
                        this.MusicDB = (c as JArray).ToObject<List<string>>() ?? new List<string>();
                    }
                }
            }
            catch (Exception)
            {
                this.MusicDB = new List<string>();
            }
        }

        /// <summary>
        /// The save download history.
        /// </summary>
        private void SaveDownloadHistory()
        {
            using (var fs = new FileStream("musicdb", FileMode.Create, FileAccess.Write))
            {
                string json = JsonConvert.SerializeObject(MusicDB);
                using (TextWriter tw = new StreamWriter(fs))
                {
                    tw.Write(json);
                }
            }
        }
        private string ConvertMimeTypeToExtension(string mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
            { 
                throw new ArgumentNullException("mimeType");
            }

            string index = string.Format(@"MIME\Database\Content Type\{0}", mimeType);
            string extension;
            if (this.MimeTypeToExtension.TryGetValue(index, out extension))
            {
                return extension;
            }
            
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(index, false);
            object obj = registryKey != null ? registryKey.GetValue("Extension", null) : "." + mimeType.Split('/')[1];
            extension = obj != null ? obj.ToString() : string.Empty;
            this.MimeTypeToExtension[index] = extension;
            return extension;

        }

        #endregion
    }
}