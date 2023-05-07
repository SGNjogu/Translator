using System;
using Xamarin.Forms;

namespace Translation.Utils
{
    public static class ResourceFinder
    {
        public static T GetResource<T>(string resourceName) where T : class
        {
            Application.Current.Resources.TryGetValue(resourceName, out var returnVal);
            return returnVal as T;
        }

        public static void UpdateResource(string resourceName, object resource)
        {
            try
            {
                Application.Current.Resources[resourceName] = resource;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
