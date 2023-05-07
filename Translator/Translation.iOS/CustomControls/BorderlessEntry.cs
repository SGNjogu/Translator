
using System.ComponentModel;
using Translation.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(Translation.iOS.CustomControls.BorderlessEntry))]
namespace Translation.iOS.CustomControls
{
    public class BorderlessEntry : EntryRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control != null)
            {
                Control.Layer.BorderWidth = 0;
                Control.BorderStyle = UIKit.UITextBorderStyle.None;
            }
        }
    }
}