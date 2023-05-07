using System;
using System.Collections.Generic;

using Translation.Models;

using Xamarin.Forms;

namespace Translation.Views.Components.QuickStartComponents
{
    public partial class AutoDetectionLanguages : ContentView
    {
        public AutoDetectionLanguages()
        {
            InitializeComponent();
        }

        void ListView_ItemTapped(System.Object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            var selectedItem = e.Item as Language;
            MessagingCenter.Instance.Send(selectedItem, "AutoDetectionSlectedLanguage");
        }
    }
}
