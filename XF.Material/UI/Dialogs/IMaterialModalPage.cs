using System;
using System.Threading.Tasks;

namespace XF.Material.Forms.UI.Dialogs
{
    public interface IMaterialModalPage : IDisposable
    {
        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        string MessageText { get; set; }

        /// <summary>
        /// Dismisses this modal dialog asynchronously.
        /// </summary>
        Task DismissAsync();
    }

    public interface IMaterialLoadingModalPage : IMaterialModalPage
    {
        /// <summary>
        /// 
        /// </summary>
        double Progress { get; set; }
    }
}
