using Android.Content;
using Translation.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(Translation.Droid.CustomControls.BorderlessEntry))]
namespace Translation.Droid.CustomControls
{
    public class BorderlessEntry : EntryRenderer
    {
        public BorderlessEntry(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.Background = null;
            }
        }
    }
}