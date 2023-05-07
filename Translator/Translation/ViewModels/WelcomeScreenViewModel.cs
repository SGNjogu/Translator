using MvvmHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.Interface;
using Translation.Styles;
using Translation.Views.Pages.Auth;
using Xamarin.Forms;
using BaseViewModel = MvvmHelpers.BaseViewModel;

namespace Translation.ViewModels
{
    public class WelcomeScreenViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// List of Intro Items binded to UI
        /// </summary>
        ObservableRangeCollection<object> _introList;
        public ObservableRangeCollection<object> IntroList
        {
            get { return _introList; }
            set
            {
                _introList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Set visibility for last Welcome Page
        /// </summary>
        bool _isVisibleLastWelcomeWindow;

        public bool IsVisibleLastWelcomeWindow
        {
            get { return _isVisibleLastWelcomeWindow; }
            set
            {
                _isVisibleLastWelcomeWindow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Set visibility for first Welcome Page
        /// </summary>
        bool _isVisibleFirstWelcomeWindow;

        public bool IsVisibleFirstWelcomeWindow
        {
            get { return _isVisibleFirstWelcomeWindow; }
            set
            {
                _isVisibleFirstWelcomeWindow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The current position of the Carousel
        /// </summary>
        int _carouselPosition;
        public int CarouselPosition
        {
            get { return _carouselPosition; }
            set
            {
                _carouselPosition = value;
                OnPropertyChanged();
                if (CarouselPosition == IntroList.IndexOf(IntroList.LastOrDefault()))
                {
                    NextBtnText = "GET STARTED";
                    IsVisibleFirstWelcomeWindow = false;
                    IsVisibleLastWelcomeWindow = true;
                }
                else
                {
                    NextBtnText = "NEXT";
                    IsVisibleFirstWelcomeWindow = true;
                    IsVisibleLastWelcomeWindow = false;
                }
            }
        }

        /// <summary>
        /// Text of Next Btn in the UI
        /// </summary>
        string _nextBtnText;
        public string NextBtnText
        {
            get { return _nextBtnText; }
            set
            {
                _nextBtnText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Icon displayed on Welcome screens and Page Title Bars
        /// </summary>
        private string _iconImage;
        public string IconImage
        {
            get { return _iconImage; }
            set
            {
                _iconImage = value;
                OnPropertyChanged();
            }
        }


        IAppAnalytics AppAnalytics;

        #endregion

        #region Constructor

        public WelcomeScreenViewModel(IAppAnalytics appAnalytics)
        {
            AppAnalytics = appAnalytics;

            AppAnalytics.CaptureCustomEvent("WelcomePage Navigated");
            NextBtnText = "NEXT";
            IntroList = new ObservableRangeCollection<object>();
            LoadIconImage();
            LoadIntro();
            IsVisibleFirstWelcomeWindow = true;
            IsVisibleLastWelcomeWindow = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to load Intro Items
        /// </summary>
        public void LoadIntro()
        {
            IntroList.AddRange(new List<object>
            {
                new { Title = "Wide array of languages", Description = "Up to 40 languages to choose from", ImageSource = ThemeHelper.ImagePath("languages") },
                new { Title = "Keep your history", Description = "Record your sessions and play them later", ImageSource = ThemeHelper.ImagePath("history") },
                new { Title = "Realtime translation", Description = "Get your translation as you speak", ImageSource = ThemeHelper.ImagePath("realtime") }
            });

            CarouselPosition = 0;
        }

        void LoadIconImage()
        {
            IconImage = ThemeHelper.ImagePath("icon");
        }

        /// <summary>
        /// Method to go to the next into Item
        /// </summary>
        async Task NextIntroItem()
        {

            if (CarouselPosition == 0 || CarouselPosition != IntroList.IndexOf(IntroList.LastOrDefault()))
            {
                CarouselPosition += 1;
            }
            else if (CarouselPosition == IntroList.IndexOf(IntroList.LastOrDefault()))
            {
                await SkipIntro();
            }
        }

        /// <summary>
        /// Method to skip Intro
        /// </summary>
        async Task SkipIntro()
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage());
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        /// <summary>
        /// Command to Navigate to Next IntroItem
        /// And to LoginPage if at the end of list
        /// </summary>
        ICommand _nextIntroItemCommand = null;

        public ICommand NextIntroItemCommand
        {
            get
            {
                return _nextIntroItemCommand ?? (_nextIntroItemCommand =
                                          new Command(async () => await NextIntroItem()));
            }
        }

        /// <summary>
        /// Command to Navigate to Login Page
        /// </summary>
        ICommand _skipIntroCommand = null;

        public ICommand SkipIntroCommand
        {
            get
            {
                return _skipIntroCommand ?? (_skipIntroCommand =
                                          new Command(async () => await SkipIntro()));
            }
        }

        #endregion
    }
}
