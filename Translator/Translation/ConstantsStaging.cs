using Newtonsoft.Json;
using Translation.Models;

namespace Translation
{
    public static class Constants
    {
        public static string DefaultLicenseKey = "AC7289-2F8195-CE096E-8686F0-CBD9A3-V3";
        public static string DefaultIngramMicroCustomerId = "TA-3599-8795-1134";

        public static string AppName = "Speechly";
        public static string AndroidPackageName = "com.fitts.speechly";
        public static string iOSPackageName = "com.fitts.Speechly";

        public static string CognitiveServicesApiKey = "9529ef7057ca4ef8a1115480a69b32c0";
        public static string CognitiveServicesRegion = "westeurope";
        public static string GoogleJsonCredentials = JsonConvert.SerializeObject(new GoogleCredentials());

        //TextAnalytics API Endpoint for Sentiment Analysis
        public static string TextAnalyticsAPIEndpoint = "https://speechlystagingtextanaytics.cognitiveservices.azure.com/";
        public static string TextAnalyticsAzureKey = "934b763f08ad4237b2725a498526ac2c";

        //Obtained from the server earlier, APIKey MUST be stored securely and in App.Config
        public static string HmacAppId = "4d53bce03ec34c0a911182d4c228ee6c";
        public static string HmacApiKey = "A93reRTUJHsCuQSHR+L3GxqOJyDmQpCgps102ciuabc=";

        // Backend API Endpoint
        public static string BackendAPiEndpoint = "https://speechly-api-staging.azurewebsites.net/";
        public static string UserSessionsEndpoint = "api/v2/Sessions/List";
        public static string SessionsEndpoint = "api/v2/Sessions";
        public static string TranscriptionsEndpoint = "api/v2/Transcriptions";
        public static string CreateBulkTranscriptionsEndpoint = "api/v2/CreateBulkTranscriptions";
        public static string SessionTranscriptionsEndpoint = "api/v2/Transcriptions/List";
        public static string SessionNumberEndpoint = "api/v2/Sessions/GenerateSessionNumber";
        public static string AzureADB2CUsersEndpoint = "api/v2/Users/GetByEmail";
        public static string AzureADB2CUserDataEndpoint = "api/v2/Users/GetUserData";
        public static string ImmersiveReaderEndpoint = "api/v2/ImmersiveReader";
        public static string OrganizationSettingsEndpoint = "api/v2/OrganizationSettings";
        public static string OrganizationTagsEndpoint = "api/v2/OrganizationTags/List";
        public static string CustomTagListEndpoint = "api/v2/Organizations";
        public static string UsageTrackingEndpoint = "api/v2/PackageOrderUsageEndpoints/GetUsage";
        public static string SessionTagsCreateEndpoint = "api/v2/SessionTags";
        public static string SessionTagsGetEndpoint = "api/v2/SessionTags/List";
        public static string AppVersionsEndpoint = "/api/v2/AppVersions/List/";
        public static string BackendLanguagesEndpoint = "api/v2/Languages";
        public static string UsersEndpoint = "api/v2/Users/";
        public const string UserQuestions = "userQuestions";
        public const string OrganizationQuestions = "organizationQuestions";
       public static string UserUsageLimitEndpoint = "api/v2/Users/";
       public static string UserLanguageEndpoint = "api/v2/Users/";

        // Microsoft Graph API Endpoints
        public static string GraphAPIEndpoint = "https://graph.microsoft.com/v1.0";
        public static string GraphAPIOrganizationEndpoint = $"{GraphAPIEndpoint}/organization";
        public static string GraphAPIUserEndpoint = $"{GraphAPIEndpoint}/me?$select=id,mail,userType,displayName";

        // AppCenter Key
        public const string AndroidAppCenterKey = "5d720188-e246-4294-a11d-45fe703ba50b";
        public const string IOSAppCenterKey = "f7cc0fca-4e85-4ec9-9ce3-4003681be7c8";

        // Azure Blob Storage
        public static string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=speechlystoragestaging;AccountKey=epFjrq4hEoH2yMlt8TQQrbQrEbYKsqJwOqspRIO6hBYiGFDZ+JtJHbIVtG9gQJZXhsH3/xqViSe0+/IQYlGMfQ==;EndpointSuffix=core.windows.net";
        public static string AzureStorageUrl = "https://speechlystoragestaging.blob.core.windows.net";
        public static string RecordingsContainer = "recordings";
        public static string RecordingsURL = $"{AzureStorageUrl}/{RecordingsContainer}/";

        //Azure AD B2C Constants

        //To Change ClientID, also change it on Android.Utils.MsalActivity, and on iOS Info.plist
        //This is VITAL for the redirectUrl to work on both platforms
        public const string ClientID = "c6380b83-1161-4764-9687-0dcdf66c8027";

        public static string[] Scopes = new string[] { "openid", "profile" };
        public const string IOSKeyChainGroup = "com.microsoft.adalcache";
        public static string PolicySignUpSignIn = "B2C_1A_signup_signin";
        public static string Tenant = "speechlyauth.onmicrosoft.com";
        public static string AzureADB2CHostname = "speechlyauth.b2clogin.com";
        public static string AuthorityBase = $"https://{AzureADB2CHostname}/tfp/{Tenant}/";
        public static string AuthoritySignInSignUp = $"{AuthorityBase}{PolicySignUpSignIn}/";
    }
}
