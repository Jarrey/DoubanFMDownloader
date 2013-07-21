// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The channel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using System.Collections.Generic;

    /// <summary>
    /// The channel.
    /// </summary>
    public class Channel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public Channel(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        #endregion

        #region Static Members

        /// <summary>
        /// Gets the channels.
        /// </summary>
        public static List<Channel> Channels
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
                               new Channel(10, "电影原声 MHz"),
                               new Channel(13, "爵士 MHz"),
                               new Channel(14, "电子 MHz"),
                               new Channel(15, "说唱 MHz"),
                               new Channel(16, "R&B MHz"),
                               new Channel(17, "日语 MHz"),
                               new Channel(18, "韩语 MHz"),
                               new Channel(19, "Puma MHz"),
                               new Channel(20, "女生 MHz"),
                               new Channel(22, "法语 MHz"),
                               new Channel(32, "咖啡 MHz"),
                               new Channel(27, "古典 MHz"),
                               new Channel(26, "豆瓣音乐人 MHz"),
                               new Channel(30, "BMW MHz")
                           };
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}