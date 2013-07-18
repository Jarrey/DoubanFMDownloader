// -----------------------------------------------------------------------
// <copyright file="OldWindow.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DoubanMusicDownloader
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OldWindow : System.Windows.Forms.IWin32Window
    {
        IntPtr _handle;
        public OldWindow(IntPtr handle)
        {
            _handle = handle;
        }
        #region IWin32Window Members
        IntPtr System.Windows.Forms.IWin32Window.Handle
        {
            get { return _handle; }
        }
        #endregion
    } 

}
