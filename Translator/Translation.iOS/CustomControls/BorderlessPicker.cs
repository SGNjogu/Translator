using System.ComponentModel;
using Translation.CustomControls;
using Translation.iOS.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Translation.CustomControls.BorderlessPicker), typeof(Translation.iOS.CustomControls.BorderlessPicker))]
namespace Translation.iOS.CustomControls
{
    public class BorderlessPicker : PickerRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            Control.Layer.BorderWidth = 0;
        }
    }
}