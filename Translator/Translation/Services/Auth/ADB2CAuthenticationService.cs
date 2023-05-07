using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Translation.AppSettings;
using Translation.Auth;
using Translation.Hmac;
using Translation.Models;
using Translation.Utils;
using Xamarin.Forms;

namespace Translation.Services.Auth
{
    public class ADB2CAuthenticationService
    {
        private IPublicClientApplication _pca;
        public static string IdToken { get; private set; }

        private static readonly Lazy<ADB2CAuthenticationService> lazy = new Lazy<ADB2CAuthenticationService>
           (() => new ADB2CAuthenticationService());

        public static ADB2CAuthenticationService Instance { get { return lazy.Value; } }
        public ADB2CAuthenticationService()
        {
            var builder = PublicClientApplicationBuilder.Create(Constants.ClientID)
            .WithIosKeychainSecurityGroup(Constants.IOSKeyChainGroup)
            .WithB2CAuthority(Constants.AuthoritySignInSignUp)
            .WithRedirectUri($"msal{Constants.ClientID}://auth");

            var windowLocatorService = DependencyService.Get<IParentWindowLocatorService>();

            if (windowLocatorService != null)
            {
                builder = builder.WithParentActivityOrWindow(() => windowLocatorService?.GetCurrentParentWindow());
            }

            _pca = builder.Build();
        }

        public async Task<AuthenticationObject> SignInAsync()
        {
            AuthenticationResult authResult = null;
            try
            {
                authResult = await SignInSilently();
            }
            catch
            {
                try
                {
                    authResult = await SignInInteractively();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Something happened. Please try again");
                }
            }

            if (authResult != null)
            {
                IdToken = authResult.IdToken;
                string email_objectId;
                var azureB2cUser = ValidateToken(authResult.IdToken);
                if (string.IsNullOrEmpty(azureB2cUser.EmailAddress) && !string.IsNullOrEmpty(azureB2cUser.UserId))
                {
                    email_objectId = azureB2cUser.UserId;
                }
                else
                {
                    email_objectId = azureB2cUser.EmailAddress;
                }
                var speechlyUserData = await GetBackendUserData(email_objectId);
                if (speechlyUserData != null)
                {
                    if (speechlyUserData.CanLogIn && speechlyUserData.User != null)
                    {
                        var userContext = await SaveUser(speechlyUserData.User);
                        return new AuthenticationObject() { SpeechlyUserData = speechlyUserData, UserContext = userContext };
                    }
                    else
                    {
                        await SignOutAsync();
                        return new AuthenticationObject() { SpeechlyUserData = speechlyUserData, UserContext = null };
                    }
                }
            }

            await SignOutAsync();
            return null;
        }

        private async Task<AuthenticationResult> SignInInteractively()
        {
            AuthenticationResult authResult = await _pca.AcquireTokenInteractive(Constants.Scopes)
                .ExecuteAsync();

            return authResult;
        }

        private async Task<AuthenticationResult> SignInSilently()
        {
            IEnumerable<IAccount> accounts = await _pca.GetAccountsAsync();
            AuthenticationResult authResult = await _pca.AcquireTokenSilent(Constants.Scopes, GetAccountByPolicy(accounts, Constants.PolicySignUpSignIn))
               .WithB2CAuthority(Constants.AuthoritySignInSignUp)
               .ExecuteAsync();

            return authResult;
        }

        public async Task SignOutAsync()
        {
            IEnumerable<IAccount> accounts = await _pca.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await _pca.RemoveAsync(account);
                AppSettings.Settings.ClearSettings();
            }
        }

        private async Task<UserContext> SaveUser(SpeechlyUser speechlyUser)
        {
            var newContext = new UserContext();

            newContext.Name = speechlyUser.UserName;
            newContext.Email = speechlyUser.Email;
            newContext.UserId = speechlyUser.ObjectId;
            newContext.Domain = DomainName(speechlyUser.Email);
            newContext.OrganizationId = speechlyUser.OrganizationId;
            newContext.UserIntID = speechlyUser.Id;
            newContext.ResellerName = speechlyUser.Reseller?.Name;
            newContext.ResellerEmail = speechlyUser.Reseller?.Email;
            newContext.DataConsentStatus = speechlyUser.Organization != null ? speechlyUser.Organization.DataConsentStatus : false;
            newContext.IsLoggedIn = true;

            var userJsonString = await Utils.JsonConverter.ReturnJsonStringFromObject(newContext);
            await Settings.AddSecureSetting(Settings.SecureSetting.UserObject, userJsonString);

            Settings.AddSetting(Settings.Setting.DataConsentStatus, newContext.DataConsentStatus.ToString());
            Settings.AddSetting(Settings.Setting.IsLoggedIn, true.ToString());

            return newContext;
        }

        private string DomainName(string email)
        {
            int atStringPosition = email.IndexOf("@");
            string domain = email.Substring(atStringPosition + 1);
            return domain;
        }

        private async Task<SpeechlyUserData> GetBackendUserData(string email_objectId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();

                HttpResponseMessage get_response = await client.GetAsync($"{Constants.AzureADB2CUserDataEndpoint}/{email_objectId}");

                string get_content = await get_response.Content.ReadAsStringAsync();

                if ((int)get_response.StatusCode == 200)
                {
                    Debug.WriteLine($"SUCCESS GET USER: {get_response.StatusCode}");
                    return JsonConvert.DeserializeObject<SpeechlyUserData>(get_content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        private AzureAdB2cUser ValidateToken(string jwtToken)
        {
            try
            {
                IdToken = jwtToken;
                var stream = jwtToken;
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(stream);
                var tokenS = jsonToken as JwtSecurityToken;

                AzureAdB2cUser azureAdB2CUser = new AzureAdB2cUser()
                {
                    TenantId = tokenS.Claims.FirstOrDefault(claim => claim.Type == "tid")?.Value,
                    UserId = tokenS.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value,
                    FirstName = tokenS.Claims.FirstOrDefault(claim => claim.Type == "given_name")?.Value,
                    LastName = tokenS.Claims.FirstOrDefault(claim => claim.Type == "family_name")?.Value,
                    UserName = tokenS.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value,
                    EmailAddress = tokenS.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value
                };
                return azureAdB2CUser;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IAccount GetAccountByPolicy(IEnumerable<IAccount> accounts, string policy)
        {
            foreach (var account in accounts)
            {
                string userIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
                if (userIdentifier.EndsWith(policy.ToLower())) return account;
            }

            return null;
        }
        public async Task<AuthenticationResult> AttemptSilentLogin()
        {
            AuthenticationResult authResult = null;
            var accounts = await _pca.GetAccountsAsync();

            try
            {
                authResult = await _pca.AcquireTokenSilent(Constants.Scopes, GetAccountByPolicy(accounts, Constants.PolicySignUpSignIn))
                    .WithB2CAuthority(Constants.AuthoritySignInSignUp)
                    .ExecuteAsync();

                if (authResult != null)
                {
                    IdToken = authResult.IdToken;
                    string email_objectId;
                    var azureB2cUser = ValidateToken(authResult.IdToken);
                    if (string.IsNullOrEmpty(azureB2cUser.EmailAddress) && !string.IsNullOrEmpty(azureB2cUser.UserId))
                    {
                        email_objectId = azureB2cUser.UserId;
                    }
                    else
                    {
                        email_objectId = azureB2cUser.EmailAddress;
                    }
                    var speechlyUserData = await GetBackendUserData(email_objectId);
                    var speechlyUser = speechlyUserData?.User;

                    if (speechlyUser == null || !speechlyUserData.CanLogIn)
                    {
                        authResult = null;
                        await SignOutAsync();
                        return authResult;
                    }

                    if (speechlyUserData.CanLogIn && speechlyUser != null)
                    {
                        await SaveUser(speechlyUser);
                    }
                    else
                    {
                        await SignOutAsync();
                    }
                }
                else
                {
                    await SignOutAsync();
                }

            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return authResult;
        }
    }
}
