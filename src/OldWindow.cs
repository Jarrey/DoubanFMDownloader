// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The old window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// The old window.
    /// </summary>
    public class OldWindow : IWin32Window
    {
        #region Fields

        /// <summary>
        /// The _handle.
        /// </summary>
        private readonly IntPtr _handle;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OldWindow"/> class.
        /// </summary>
        /// <param name="handle">
        /// The handle.
        /// </param>
        public OldWindow(IntPtr handle)
        {
            this._handle = handle;
        }

        #endregion

        #region Explicit Interface Properties

        /// <summary>
        /// Gets the handle.
        /// </summary>
        IntPtr IWin32Window.Handle
        {
            get
            {
                return this._handle;
            }
        }

        #endregion
    }
}