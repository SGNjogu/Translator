using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace Translation.Droid.Utils
{
    [Activity]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = "auth",
        DataScheme = "msalc6380b83-1161-4764-9687-0dcdf66c8027")]

    public class MsalActivity : BrowserTabActivity
    {

    }
}