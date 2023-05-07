using System;
using System.Windows.Input;
using Translation.Helpers;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class FontSizeViewViewModel
    {
        public FontSizeViewViewModel()
        {
            FontSizeHelper.GetTranscriptionsFontSize();
        }

        ICommand _zoomOut = null;

        public ICommand ZoomOut
        {
            get
            {
                return _zoomOut ?? (_zoomOut =
                                          new Command((object obj) => FontSizeHelper.DecreaseFontSize()));
            }
        }

        ICommand _zoomIn = null;

        public ICommand ZoomIn
        {
            get
            {
                return _zoomIn ?? (_zoomIn =
                                          new Command((object obj) => FontSizeHelper.IncreaseFontSize()));
            }
        }
    }
}
