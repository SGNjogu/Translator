using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Translation.Services.Auth;

namespace Translation.Hmac
{
    public static class HttpClientProvider
    {
        public static HttpClient Create()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Constants.BackendAPiEndpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ADB2CAuthenticationService.IdToken);

            return client;
        }
    }
}
