using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Translation.Utils
{
    public static class Dialogs
    {
        public enum DialogMessage
        {
            NetworkError,
            Defined,
            UndefinedError,
            InputError
        }

        static readonly string UndefinedError = "Something went wrong, please try again later.";
        static readonly string NetworkError = "Network Error.";
        static readonly string InputError = " is required.";

        public static void HandleDialogMessage(
            DialogMessage error,
            string message = "",
            int seconds = 2,
            string backgroundColor =
            "#333333",
            ToastPosition position = ToastPosition.Bottom)
        {
            switch (error)
            {
                case DialogMessage.NetworkError:
                    message = "    " + NetworkError + "    ";
                    break;
                case DialogMessage.UndefinedError:
                    message = "    " + UndefinedError + "    ";
                    break;
                case DialogMessage.Defined:
                    message = "    " + message + "    ";
                    break;
                case DialogMessage.InputError:
                    message = "    " + message + InputError + "    ";
                    break;
            }
            UserDialogs.Instance.Toast(new ToastConfig(message)
            .SetBackgroundColor(Color.FromHex(backgroundColor))
            .SetMessageTextColor(Color.White)
            .SetDuration(TimeSpan.FromSeconds(seconds))
            .SetPosition(position)
            );
        }

        public static IProgressDialog ProgressDialog = UserDialogs.Instance.Progress(new ProgressDialogConfig
        {
            AutoShow = false,
            CancelText = "Cancel",
            IsDeterministic = false,
            MaskType = MaskType.Black,
            Title = null
        });

        public static async Task OpenBrowser(string url)
        {
            await Browser.OpenAsync(url, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show
            });
        }

        public static async Task ShareText(string text)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = text,
            });
        }

        public static async Task SendEmail(string subject, string body, List<string> recipients)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = recipients,
                    //Cc = ccRecipients,
                    //Bcc = bccRecipients
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fbsEx)
            {
                // Email is not supported on this device
                Debug.WriteLine(fbsEx.Message);
                Dialogs.HandleDialogMessage(DialogMessage.Defined, "Your device does not support sending emails.");
            }
            catch (Exception ex)
            {
                // Some other exception occurred
                Debug.WriteLine(ex.Message);
            }
        }

        public static async Task CopyTextToClipBoard(string text, string message = "Copied")
        {
            await Clipboard.SetTextAsync(text);
            HandleDialogMessage(DialogMessage.Defined, message, 1);
        }

        public static async Task<string> GetTextFromClipBoard()
        {
            if (Clipboard.HasText)
            {
                return await Clipboard.GetTextAsync();
            }

            return null;
        }
    }
}