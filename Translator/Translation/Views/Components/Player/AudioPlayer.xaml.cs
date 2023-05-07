using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Components.Player
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AudioPlayer : ContentView
    {
        public AudioPlayer()
        {
            InitializeComponent();
        }

        private void RangeSlider_DragCompleted(object sender, EventArgs e)
        {
            var newPosition = slider.UpperValue;
            TimeSpan span = TimeSpan.FromSeconds((double)(new decimal(newPosition)));
            MessagingCenter.Instance.Send(new SliderDragMessage { Position = span }, "SliderDragCompleted");
        }
    }
}