using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Components.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForcedUpdatePopup : ContentPage
    {
        public ForcedUpdatePopup()
        {
            InitializeComponent();
        }
    }
}