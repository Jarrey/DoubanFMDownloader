// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The channel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DoubanMusicDownloader
{
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