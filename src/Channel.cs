// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The channel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using Newtonsoft.Json.Linq;
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
        public Channel(JObject chaanel)
        {
            this.Id = Utils.Parse<int>(chaanel, "id");
            this.Name = Utils.Parse<string>(chaanel, "name");
            this.Intro = Utils.Parse<string>(chaanel, "intro");            
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

        public string Intro { get; set; }

        #endregion
    }
}