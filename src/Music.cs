// -----------------------------------------------------------------------
// <copyright file="Music.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    using DoubanMusicDownloader.Properties;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Music : INotifyPropertyChanged
    {
        public string Url { get; set; }
        public string AlbumPicture { get; set; }
        public string AlbumTitle { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string PublicTime { get; set; }
        public string Publisher { get; set; }

        private double _progress;
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                this.OnPropertyChanged("Progress");
            }
        }

        public string FileName
        {
            get
            {
                string name = string.Format(Settings.Default.FileNameFormat, this.Title, this.AlbumTitle, this.Artist);
                return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Music &&
                this.FileName == (obj as Music).FileName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
