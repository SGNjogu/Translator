﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.Help
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Features : ContentPage
    {
        public Features()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }
    }
}