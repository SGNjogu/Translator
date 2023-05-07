using Android.Views;
using Android.Views.Animations;
using Translation.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.Views.ScaleGestureDetector;
using Android.Content;
using Translation.Helpers;

[assembly: ExportRenderer(typeof(ZoomListView), typeof(Translation.Droid.CustomControls.ZoomListView))]
namespace Translation.Droid.CustomControls
{
    public class ZoomListView : ListViewRenderer, IOnScaleGestureListener
    {
        private float mScale = 1f;
        private ScaleGestureDetector mScaleDetector;

        public ZoomListView(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            mScaleDetector = new ScaleGestureDetector(Context, this);
        }


        public override bool DispatchTouchEvent(MotionEvent e)
        {
            base.DispatchTouchEvent(e);
            return mScaleDetector.OnTouchEvent(e);
        }

        public bool OnScale(ScaleGestureDetector detector)
        {
           

            if (detector.ScaleFactor > 1f)
            {
                FontSizeHelper.IncreaseFontSize();
            }
                

            if (detector.ScaleFactor < 1f) 
            {

                FontSizeHelper.DecreaseFontSize();
            }
                
           
            return true;
        }

        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            return true;
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {

        }
    }
}