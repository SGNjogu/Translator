using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Translation.Auth;
using Translation.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Translation.AppSettings
{
    public static class Settings
    {
        /// <summary>
        /// Settings enums for easier identity of saved items
        /// </summary>
        public enum Setting
        {
            AppTheme,
            IsLoggedIn,
            DefaultSourceLanguage,
            DefaultTargetLanguage,
            DataConsentStatus,
            IsDefaultLanguageOverridden,
            DeviceAddress,
            RememberSetup,
            UseNerualVoice,
            TranscriptionsFontSize
        }

        /// <summary>
        /// Theme enums for easier identity of saved items
        /// </summary>
        public enum Theme
        {
            LightTheme,
            DarkTheme,
            SystemPreferred
        }

        /// <summary>
        /// SecureSettings enums for easier identity of saved items
        /// </summary>
        public enum SecureSetting
        {
            UserObject,
            Organization
        }

        /// <summary>
        /// Client type enum
        /// </summary>
        public enum ClientType
        {
            Desktop,
            iOS,
            Android
        }

        /// <summary>
        /// Method to add setting
        /// </summary>
        /// <param name="preference">Takes in settings enum</param>
        /// <param name="setting">takes in the setting in string</param>
        public static void AddSetting(Setting preference, string setting)
        {
            Preferences.Set(EnumsConverter.ConvertToString(preference), setting);
        }

        /// <summary>
        /// Method to get setting
        /// </summary>
        /// <param name="preference">Takes in settings enum</param>
        /// <returns>Setting string</returns>
        public static string GetSetting(Setting preference)
        {
            bool hasKey = Preferences.ContainsKey(EnumsConverter.ConvertToString(preference));

            if (hasKey)
            {
                return Preferences.Get(EnumsConverter.ConvertToString(preference), null);
            }

            return null;
        }

        /// <summary>
        /// Method to remove setting
        /// </summary>
        /// <param name="preference">Takes in the setting enum</param>
        public static void RemoveSetting(Setting preference)
        {
            Preferences.Remove(EnumsConverter.ConvertToString(preference));
        }

        /// <summary>
        /// Method to clear all settings
        /// </summary>
        public static void ClearSettings()
        {
            Preferences.Clear();
        }

        /// <summary>
        /// Method to check if a user is logged in
        /// </summary>
        /// <returns>True if user is logged in</returns>
        public static bool IsUserLoggedIn()
        {
            string isLoggedIn = GetSetting(Setting.IsLoggedIn);

            if (!string.IsNullOrEmpty(isLoggedIn))
            {
                return Convert.ToBoolean(isLoggedIn);
            }

            return false;
        }

        /// <summary>
        /// Method to check if a user overrides the default language settings for the organization
        /// </summary>
        /// <returns>True if default language is overridden</returns>
        public static bool IsDefaultLanguageOverridden()
        {
            string isLanguageOverridden = GetSetting(Setting.IsDefaultLanguageOverridden);

            if (!string.IsNullOrEmpty(isLanguageOverridden))
            {
                return Convert.ToBoolean(isLanguageOverridden);
            }

            return false;
        }

        /// <summary>
        /// Method to get currently loggedIn user
        /// </summary>
        /// <returns>Current User</returns>
        public static async Task<UserContext> CurrentUser()
        {
            if (IsUserLoggedIn())
            {
                var userJson = await GetSecureSetting(SecureSetting.UserObject);
                return await JsonConverter.ReturnObjectFromJsonString<UserContext>(userJson).ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Method to add secure setting
        /// </summary>
        /// <param name="preference">Takes in the secure settings enum</param>
        /// <param name="setting">Takes in the setting string</param>
        public static async Task AddSecureSetting(SecureSetting preference, string setting)
        {
            try
            {
                await SecureStorage.SetAsync(EnumsConverter.ConvertToString(preference), setting);
            }
            catch (Exception ex)
            {
                // Possible that device doesn't support secure storage on device.
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Method to get a secure setting
        /// </summary>
        /// <param name="preference">Takes in the secure settings enum</param>
        /// <returns>The secure setting</returns>
        public static async Task<string> GetSecureSetting(SecureSetting preference)
        {
            try
            {
                return await SecureStorage.GetAsync(EnumsConverter.ConvertToString(preference));
            }
            catch (Exception ex)
            {
                // Possible that device doesn't support secure storage on device.
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Method to remove a secure setting
        /// </summary>
        /// <param name="preference">Takes in the secure settings enum</param>
        public static void RemoveSecureSetting(SecureSetting preference)
        {
            SecureStorage.Remove(EnumsConverter.ConvertToString(preference));
        }

        /// <summary>
        /// Method to remove all secure settings
        /// </summary>
        public static void RemoveAllSecureSettings()
        {
            SecureStorage.RemoveAll();
        }

        /// <summary>
        /// Method to get clientType
        /// </summary>
        public static string GetClientType()
        {
            string client = "";

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    client = EnumsConverter.ConvertToString(ClientType.iOS);
                    break;
                case Device.Android:
                    client = EnumsConverter.ConvertToString(ClientType.Android);
                    break;
            }

            return client;
        }
    }
}
