
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.AppSettings;
using Translation.Enums;
using Translation.Interface;
using Translation.Utils;
using Translation.Views.Pages.AppShell;
using Translation.Views.Pages.Auth;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
        public string AuthenticationErrorMessage { get; private set; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private string _errorActionMessage;
        public string ErrorActionMessage
        {
            get { return _errorActionMessage; }
            set
            {
                _errorActionMessage = value;
                OnPropertyChanged();
            }
        }

        IAppAnalytics AppAnalytics;
        IAppCrashlytics AppCrashlytics;

        public AuthViewModel(IAppAnalytics appAnalytics, IAppCrashlytics appCrashlytics)
        {
            AppAnalytics = appAnalytics;
            AppCrashlytics = appCrashlytics;

            AppAnalytics.CaptureCustomEvent("AuthPage Navigated");
        }

        /// <summary>
        /// Method to Navigate to AppShell
        /// </summary>
        async Task NavigateToAppShell()
        {
            Application.Current.MainPage = new NavigationPage(new MainPage());
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        private void InitializeErrorMessages()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //Validate Speechly Account 
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountNotFound))
                {
                    ErrorMessage = "You do not have an active Tala Account.";
                    ErrorActionMessage = "Please contact your administrator to create your account.";
                }
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeactivated))
                {
                    ErrorMessage = "Your Tala account has been deactivated.";
                    ErrorActionMessage = "Please contact your administrator to activate your account.";
                }
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDisabled))
                {
                    ErrorMessage = "Your Tala account has been disabled.";
                    ErrorActionMessage = "Please contact your administrator to enable your account.";
                }
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeleted))
                {
                    ErrorMessage = "Your Tala account has been deleted.";
                    ErrorActionMessage = "Please contact your administrator to create your account.";
                }

                //Validate Core License
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseIsExpired))
                {
                    ErrorMessage = "Your licence has expired.";
                    ErrorActionMessage = "Please contact your administrator to renew your licence.";
                }
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseIsInActive))
                {
                    ErrorMessage = "Your licence has been deactivated.";
                    ErrorActionMessage = "Please contact your administrator to activate your licence.";
                }
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseIsSuspended))
                {
                    ErrorMessage = "Your licence has been suspended.";
                    ErrorActionMessage = "Please contact your administrator.";
                }
                if (AuthenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseNotFound))
                {
                    ErrorMessage = "You do not have a licence assigned to you.";
                    ErrorActionMessage = "Please contact your administrator to assign a licence to you.";
                }
            }
        }

        /// <summary>
        /// Method to initiate Sign In
        /// </summary>
        async Task SignIn()
        {
            try
            {
                Dialogs.ProgressDialog.Show();

                var authenticationObject = await Services.Auth.ADB2CAuthenticationService.Instance.SignInAsync();
                if (authenticationObject != null)
                {
                    var userContext = authenticationObject.UserContext;
                    var speechlyUser = authenticationObject.SpeechlyUserData;

                    if (userContext != null)
                    {
                        App.CurrentUser = userContext;

                        Analytics.TrackEvent("User Logged In",
                            new Dictionary<string, string> {
                        { "User", App.CurrentUser.Name },
                        { "UserId", App.CurrentUser.UserId },
                        { "Time", DateTime.UtcNow.ToString() } });

                        Settings.AddSetting(Settings.Setting.IsLoggedIn, true.ToString());

                        Application.Current.MainPage = new NavigationPage(new MainPage());
                        await Application.Current.MainPage.Navigation.PopToRootAsync();
                        MessagingCenter.Instance.Send("", "ReInitialize");
                    }
                    else
                    {
                        AuthenticationErrorMessage = speechlyUser.ErrorMessage;
                        InitializeErrorMessages();

                        if (!speechlyUser.HasSpeeclyAccount ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeactivated) ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeleted) ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDisabled) ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountNotFound))
                        {
                            Application.Current.MainPage = new NavigationPage(new LoginFailedPage { BindingContext = this }); ;
                            await Application.Current.MainPage.Navigation.PopToRootAsync();
                        }
                        else
                        {
                            if (!speechlyUser.HasValidSpeechlyCoreLicense)
                            {
                                Application.Current.MainPage = new NavigationPage(new InvalidLicensePage { BindingContext = this }); ;
                                await Application.Current.MainPage.Navigation.PopToRootAsync();
                            }
                        }
                        Messages.LoginErrorMessage loginMessage = new Messages.LoginErrorMessage { ErrorMessage = this.ErrorMessage, ErrorActionMessage = this.ErrorActionMessage };
                        MessagingCenter.Instance.Send(loginMessage, "LoginErrorMessage");
                    }
                }

                Dialogs.ProgressDialog.Hide();
            }
            catch (Exception ex)
            {
                Dialogs.ProgressDialog.Hide();
                await Services.Auth.ADB2CAuthenticationService.Instance.SignOutAsync(); //Deletes any cached accounts
                Debug.WriteLine(ex.Message);
                Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Something happened. Please try again");
                Crashes.TrackError(ex, attachments: await AppCrashlytics.Attachments());
            }
        }

        async Task RetryLogin()
        {
            await Services.Auth.ADB2CAuthenticationService.Instance.SignOutAsync(); //Deletes any cached accounts
            Application.Current.MainPage = new NavigationPage(new LoginPage());
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        /// <summary>
        /// Command to Navigate to AppShell
        /// </summary>
        ICommand _goToAppShellCommand = null;

        public ICommand GoToAppShellCommand
        {
            get
            {
                return _goToAppShellCommand ?? (_goToAppShellCommand =
                                          new Command(async () => await NavigateToAppShell()));
            }
        }

        /// <summary>
        /// Command to Initialize SignIn
        /// </summary>
        ICommand _signInCommand = null;

        public ICommand SignInCommand
        {
            get
            {
                return _signInCommand ?? (_signInCommand =
                                          new Command(async () => await SignIn()));
            }
        }

        /// <summary>
        /// Command to Retry login
        /// </summary>
        ICommand _retryLoginCommand = null;

        public ICommand RetryLoginCommand
        {
            get
            {
                return _retryLoginCommand ?? (_retryLoginCommand =
                                          new Command(async () => await RetryLogin()));
            }
        }
    }
}
