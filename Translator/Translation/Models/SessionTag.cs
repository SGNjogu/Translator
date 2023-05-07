
using MvvmHelpers;
using Translation.Events;

namespace Translation.Models
{
    public class SessionTag : BaseViewModel
    {
        public event SessionTagChangedEvent SessionTagChanged;
        public int OrganizationTagId { get; set; }
        public int SessionId { get; set; }
        public int OrganizationId { get; set; }

        private string _tagValue;
        public string TagValue
        {
            get { return _tagValue; }
            set
            {
                _tagValue = value;
                OnPropertyChanged();
                var args = new SessionTagChangedArgs { TagValue = TagValue };
                SessionTagChanged?.Invoke(this, args);
            }
        }

        private string _tagName;
        public string TagName
    {
            get { return _tagName; }
            set
            {
                _tagName = value;
                OnPropertyChanged();
            }
        }

        public bool ShowInApp { get; set; }

        private bool _isMandatory;
        public bool IsMandatory
        {
            get { return _isMandatory; }
            set
            {
                _isMandatory = value;
                OnPropertyChanged();
            }
        }
    }
}
