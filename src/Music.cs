// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The music.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

    using DoubanMusicDownloader.Properties;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The music.
    /// </summary>
    public class Music : INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Music"/> class.
        /// </summary>
        public Music()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Music"/> class.
        /// </summary>
        /// <param name="music">
        /// The music.
        /// </param>
        public Music(JObject music)
        {
            this._music = music;
            this.Url = this.Parse<string>("url");
            this.AlbumPicture = this.Parse<string>("picture");
            this.AlbumTitle = this.Parse<string>("albumtitle");
            this.Artist = this.Parse<string>("artist");
            this.Title = this.Parse<string>("title");
            this.PublicTime = this.Parse<uint>("public_time");
            this.Publisher = this.Parse<string>("company");
        }

        #endregion

        #region Fields

        /// <summary>
        /// The _progress.
        /// </summary>
        private double _progress;

        private JObject _music;

        #endregion

        #region Public Events

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the album picture.
        /// </summary>
        public string AlbumPicture { get; set; }

        /// <summary>
        /// Gets or sets the album title.
        /// </summary>
        public string AlbumTitle { get; set; }

        /// <summary>
        /// Gets or sets the artist.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName
        {
            get
            {
                string name = string.Format(Settings.Default.FileNameFormat, this.Title, this.AlbumTitle, this.Artist);
                return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
            }
        }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        public double Progress
        {
            get
            {
                return this._progress;
            }

            set
            {
                this._progress = value;
                this.OnPropertyChanged("Progress");
            }
        }

        /// <summary>
        /// Gets or sets the public time.
        /// </summary>
        public uint PublicTime { get; set; }

        /// <summary>
        /// Gets or sets the publisher.
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Music && this.FileName == (obj as Music).FileName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        private void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// The parse.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <typeparam name="T">
        /// The type want to get value
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        private T Parse<T>(string propertyName)
        {
            try
            {
                JToken value;
                if (this._music != null && this._music.TryGetValue(propertyName, out value))
                {
                    var returnValue = value.Value<T>();
                    return returnValue;
                }
            }
            catch (Exception)
            {
            }

            return default(T);
        }

        #endregion
    }
}