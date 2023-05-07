using Microsoft.CognitiveServices.Speech.Transcription;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.AuditTracking;
using Translation.Interface;
using Translation.Utils;
using Translation.Views.Components.Popups;
using Xamarin.Forms;

namespace Translation.Models
{
    public class TranslationResultText : BaseViewModel
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public bool IsPerson1 { get; set; }
        public string LanguageName { get; set; }
        public string SourceLanguageCode { get; set; }
        public string TargetLanguageCode { get; set; }
        private string _originalText;
        public string OriginalText
        {
            get { return _originalText; }
            set
            {
                _originalText = value;
                OnPropertyChanged();
            }
        }
        private string _translatedText;
        public string TranslatedText
        {
            get { return _translatedText; }
            set
            {
                _translatedText = value;
                OnPropertyChanged();
            }
        }
        public string DateString { get; set; }
        public string Person { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsCopyPasteEnabled { get; set; }
        public bool IsComplete { get; set; }
        public long OffsetInTicks { get; set; }
        public string Sentiment { get; set; }

        private string _sentimentEmoji;
        public string SentimentEmoji
        {
            get { return _sentimentEmoji; }
            set
            {
                _sentimentEmoji = value;
                OnPropertyChanged();
            }
        }
        IAppAnalytics _appAnalytics = new AppAnalytics();
        async Task CopyText()
        {
            string text = $"{Person}:\nOriginal: {OriginalText}\nTranslated: {TranslatedText}\n{DateString}";
            await Dialogs.CopyTextToClipBoard(text, "Copied to clipboard.");
           
            _appAnalytics.CaptureCustomEvent("Copy Events",
                        new Dictionary<string, string> {
                        {"User", App.CurrentUser?.Email },
                        {"Organisation", App.CurrentUser?.Organization },
                        {"Action", "Copy single translation text" }
                 });
        
        await ClosePopup();
        }

        async Task ShareText()
        {
            string text = $"{Person}:\nOriginal: {OriginalText}\nTranslated: {TranslatedText}\n{DateString}";
            _appAnalytics.CaptureCustomEvent("Share Events",
                       new Dictionary<string, string> {
                        {"User", App.CurrentUser?.Email },
                        {"Organisation", App.CurrentUser?.Organization },
                        {"Action", "Share single translation text" }
                });
            await Dialogs.ShareText(text);
            await ClosePopup();
        }

        async Task ClosePopup()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        async Task OpenPoup()
        {
            if (IsCopyPasteEnabled)
                await PopupNavigation.Instance.PushAsync(new CopySharePopup { BindingContext = this });
        }

        /// <summary>
        /// Command to open popup
        /// </summary>
        ICommand _copyTextCommand = null;
        public ICommand CopyTextCommand
        {
            get
            {
                return _copyTextCommand ?? (_copyTextCommand =
                                          new Command(async () => await CopyText()));
            }
        }

        /// <summary>
        /// Command to open popup
        /// </summary>
        ICommand _shareTextCommand = null;
        public ICommand ShareTextCommand
        {
            get
            {
                return _shareTextCommand ?? (_shareTextCommand =
                                          new Command(async () => await ShareText()));
            }
        }

        /// <summary>
        /// Command to open popup
        /// </summary>
        ICommand _openPopupCommand = null;
        public ICommand OpenPopupCommand
        {
            get
            {
                return _openPopupCommand ?? (_openPopupCommand =
                                          new Command(async () => await OpenPoup()));
            }
        }
    }
}
