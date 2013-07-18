// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The music.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

    using DoubanMusicDownloader.Properties;

    /// <summary>
    /// The music.
    /// </summary>
    public class Music : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The _progress.
        /// </summary>
        private double _progress;

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
        public string PublicTime { get; set; }

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

        #endregion
    }
}