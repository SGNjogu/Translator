using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Translation.Views.Components.QuickStartComponents
{
    public partial class QuickStartLanguageTwoView : ContentView
    {
        public QuickStartLanguageTwoView()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<object>(this, "BringLanguageTwoIntoView", (sender) =>
            {
                ScrollTo((int)sender);
            });
        }

        private void ScrollTo(int target)
        {
            try
            {
                collectionViewTwo.ScrollTo(target, position: ScrollToPosition.Start, animate: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
