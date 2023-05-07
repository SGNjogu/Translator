using Plugin.CurrentActivity;
using Translation.Auth;

namespace Translation.Droid.Utils
{
    public class AndroidParentWindowLocatorService : IParentWindowLocatorService
    {
        public object GetCurrentParentWindow()
        {
            return CrossCurrentActivity.Current.Activity;
        }
    }
}