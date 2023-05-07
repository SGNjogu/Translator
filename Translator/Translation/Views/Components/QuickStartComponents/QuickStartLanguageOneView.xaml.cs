using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Translation.Views.Components.QuickStartComponents
{
    public partial class QuickStartLanguageOneView : ContentView
    {
        public QuickStartLanguageOneView()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<object>(this, "BringLanguageOneIntoView", (sender) =>
            {
                ScrollTo((int)sender);
            });
        }

        private void ScrollTo(int target)
        {
            try
            {
                collectionViewOne.ScrollTo(target, position: ScrollToPosition.Start, animate: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
