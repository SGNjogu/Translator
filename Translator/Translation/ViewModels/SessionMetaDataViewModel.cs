using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MvvmHelpers;
using Translation.DataService.Interfaces;
using Translation.Messages;
using Translation.Utils;
using Xamarin.Forms;
using SessionTag = Translation.Models.SessionTag;

namespace Translation.ViewModels
{
    public class SessionMetaDataViewModel : BaseViewModel
    {

        private string _sessionName;
        public string SessionName
        {
            get { return _sessionName; }
            set
            {
                _sessionName = value;
                OnPropertyChanged();
                ValidateInputs();
            }
        }

        private string _customTag;
        public string CustomTag
        {
            get { return _customTag; }
            set
            {
                _customTag = value;
                OnPropertyChanged();
            }
        }

        Color _btnColor;
        public Color BtnColor
        {
            get { return _btnColor; }
            set
            {
                _btnColor = value;
                OnPropertyChanged();
            }
        }

        Color _btnTextColor;
        public Color BtnTextColor
        {
            get { return _btnTextColor; }
            set
            {
                _btnTextColor = value;
                OnPropertyChanged();
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        private ObservableRangeCollection<string> _customTags;
        public ObservableRangeCollection<string> CustomTags
        {
            get { return _customTags; }
            set
            {
                _customTags = value;
                OnPropertyChanged();
            }
        }

        private ObservableRangeCollection<SessionTag> _sessionTags;
        public ObservableRangeCollection<SessionTag> SessionTags
        {
            get { return _sessionTags; }
            set
            {
                _sessionTags = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisibleOrganizationTags;
        public bool IsVisibleOrganizationTags
        {
            get { return _isVisibleOrganizationTags; }
            set
            {
                _isVisibleOrganizationTags = value;
                OnPropertyChanged();
            }
        }

        private ObservableRangeCollection<string> _organizationCustomTags;
        public ObservableRangeCollection<string> OrganizationCustomTags
        {
            get { return _organizationCustomTags; }
            set
            {
                _organizationCustomTags = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisibleOrganizationCustomTags = false;
        public bool IsVisibleOrganizationCustomTags
        {
            get { return _isVisibleOrganizationCustomTags; }
            set
            {
                _isVisibleOrganizationCustomTags = value;
                OnPropertyChanged();
            }
        }

        private bool IsEditing = false;

        private readonly IDataService _dataService;

        public SessionMetaDataViewModel(IDataService dataService)
        {
            _dataService = dataService;
            MessagingCenter.Subscribe<SessionMetaDataMessage>(this, "EditSessionMetadata", (sender) =>
            {
                EditSessionMetadata(sender);
            });
            MessagingCenter.Subscribe<string>(this, "ClearSessionMetadata", (sender) =>
            {
                ClearSessionMetadata();
            });
            MessagingCenter.Subscribe<string>(this, "UpdateOrganizationSettings", (sender) =>
            {
                GetOrganizationTags();
                GetOrganizationCustomTags();
            });
            ClearSessionMetadata();
        }

        private void ClearSessionMetadata()
        {
            IsEditing = false;
            SessionName = "";
            BtnColor = Color.FromHex("#bdbdbd");
            BtnTextColor = Color.FromHex("#616161");
            IsEnabled = false;
            SessionTags = new ObservableRangeCollection<SessionTag>();
            CustomTags = new ObservableRangeCollection<string>();
            OrganizationCustomTags = new ObservableRangeCollection<string>();
            GetOrganizationTags();
            GetOrganizationCustomTags();
        }

        private void EditSessionMetadata(SessionMetaDataMessage message)
        {
            IsEditing = true;
            SessionName = message.SessionName;

            if (message.SessionTags != null && message.SessionTags.Any())
            {
                SessionTags = new ObservableRangeCollection<SessionTag>(message.SessionTags);
            }

            if (message.CustomTags != null && message.CustomTags.Any())
            {
                CustomTags = new ObservableRangeCollection<string>(message.CustomTags);
            }

            IsEnabled = true;
            Application.Current.Resources.TryGetValue("AccentColor", out var btnColor);
            BtnColor = (Color)btnColor;
            BtnTextColor = Color.White;
        }

        public void ClearTags()
        {
            CustomTags = new ObservableRangeCollection<string>();
            SessionTags = new ObservableRangeCollection<SessionTag>();
        }

        private async void GetOrganizationTags()
        {
            var organizationTags = await _dataService.GetOrganizationTagsAsync();

            if (organizationTags != null && organizationTags.Any())
            {
                foreach (var tag in organizationTags.Where(i => i.IsShownOnApp))
                {
                    var sessionTag = new SessionTag
                    {
                        IsMandatory = tag.IsMandatory,
                        ShowInApp = tag.IsShownOnApp,
                        OrganizationTagId = tag.OrganizationTagId,
                        TagName = tag.TagName
                    };

                    sessionTag.SessionTagChanged += OnTagValueChanged;

                    SessionTags.Add(sessionTag);
                }

                if (SessionTags.Any())
                {
                    IsVisibleOrganizationTags = true;
                }
                else
                {
                    IsVisibleOrganizationTags = false;
                }
            }
            else
            {
                IsVisibleOrganizationTags = false;
            }
        }

        private async void GetOrganizationCustomTags()
        {
            var organizationCustomTags = await _dataService.GetCustomTagsAsync();

            if (organizationCustomTags != null && organizationCustomTags.Any())
            {
                foreach (var tag in organizationCustomTags)
                {
                    OrganizationCustomTags.Add(tag.TagName);
                }

                if (organizationCustomTags.Any())
                    IsVisibleOrganizationCustomTags = true;
            }
        }

        private void OnTagValueChanged(object sender, Events.SessionTagChangedArgs args)
        {
            ValidateInputs();
        }

        private void ValidateInputs()
        {
            if (SessionTags != null && SessionTags.Any())
            {
                bool mandatoryTagsMissing = false;
                var mandtoryTags = SessionTags.Where(s => s.IsMandatory == true && s.ShowInApp == true);

                if (mandtoryTags.Any())
                {
                    mandatoryTagsMissing = mandtoryTags.Any(s => string.IsNullOrEmpty(s.TagValue));
                }

                if (!mandatoryTagsMissing && !string.IsNullOrEmpty(SessionName))
                {
                    IsEnabled = true;
                    Application.Current.Resources.TryGetValue("AccentColor", out var btnColor);
                    BtnColor = (Color)btnColor;
                    BtnTextColor = Color.White;
                }
                else
                {
                    IsEnabled = false;
                    BtnColor = Color.FromHex("#bdbdbd");
                    BtnTextColor = Color.Black;
                }
            }
            else
            {
                ValidateSessionName();
            }
        }

        private void ValidateSessionName()
        {
            if (string.IsNullOrEmpty(SessionName))
            {
                IsEnabled = false;
                BtnColor = Color.FromHex("#bdbdbd");
                BtnTextColor = Color.Black;
            }
            else
            {
                IsEnabled = true;
                BtnColor = Color.FromHex("#b624c1");
                BtnTextColor = Color.White;
            }
        }

        private void Continue()
        {
            try
            {
                SessionMetaDataMessage message = new SessionMetaDataMessage
                {
                    SessionName = SessionName
                };

                if (SessionTags != null && SessionTags.Any())
                {
                    message.SessionTags = SessionTags.ToList();
                }

                if (CustomTags != null && CustomTags.Any())
                {
                    message.CustomTags = CustomTags.ToList();
                }

                if (IsEditing)
                {
                    MessagingCenter.Instance.Send(message, "UpdateSessionMetadata");
                }
                else
                {
                    MessagingCenter.Instance.Send(message, "SaveSessionMetadata");
                }

                IsEditing = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void AddTag()
        {
            if (!string.IsNullOrEmpty(CustomTag))
            {
                CustomTags.Insert(0, CustomTag);
                CustomTag = string.Empty;
            }
        }

        private void RemoveTag(string tag)
        {
            CustomTags.Remove(tag);
        }

        private void AddCustomTag(string tag)
        {
            CustomTags.Add(tag);
        }

        ICommand _addTagCommand = null;

        public ICommand AddTagCommand
        {
            get
            {
                return _addTagCommand ?? (_addTagCommand =
                                          new Command(() => AddTag()));
            }
        }

        ICommand _addCustomTagCommand = null;

        public ICommand AddCustomTagCommand
        {
            get
            {
                return _addCustomTagCommand ?? (_addCustomTagCommand =
                                          new Command((object obj) => AddCustomTag((string)obj)));
            }
        }

        ICommand _removeTagCommand = null;

        public ICommand RemoveTagCommand
        {
            get
            {
                return _removeTagCommand ?? (_removeTagCommand =
                                          new Command((object obj) => RemoveTag((string)obj)));
            }
        }

        ICommand _continueCommand = null;

        public ICommand ContinueCommand
        {
            get
            {
                return _continueCommand ?? (_continueCommand =
                                          new Command(() => Continue()));
            }
        }
    }
}
