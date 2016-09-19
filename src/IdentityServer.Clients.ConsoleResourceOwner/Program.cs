using System;
using System.Net.Http;
using System.Text;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.Extensions;
using Newtonsoft.Json.Linq;

namespace IdentityServer.Clients.ConsoleResourceOwner
{
    internal class Program
    {
        private static void Main()
        {
            var response = RequestToken();
            ShowResponse(response);

            Console.ReadLine();
            CallService(response.AccessToken);
        }

        private static TokenResponse RequestToken()
        {
            var client = new TokenClient(
                Constants.Constants.TokenEndpoint,
                "ro.client",
                "secret");

            // idsrv supports additional non-standard parameters 
            // that get passed through to the user service
            var optional = new
            {
                acr_values = "tenant:custom_account_store1 foo bar quux"
            };
            return client.RequestResourceOwnerPasswordAsync("Admin", "123456", "read write", optional).Result;
        }

        private static void CallService(string token)
        {
            const string baseAddress = Constants.Constants.AspNetWebApiSampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = client.GetStringAsync("identity").Result;

            "\n\nService claims:".ConsoleGreen();
            Console.WriteLine(JArray.Parse(response));
        }

        private static void ShowResponse(TokenResponse response)
        {
            if (!response.IsError)
            {
                "Token response:".ConsoleGreen();
                Console.WriteLine(response.Json);

                if (!response.AccessToken.Contains(".")) return;
                "\nAccess Token (decoded):".ConsoleGreen();

                var parts = response.AccessToken.Split('.');
                var header = parts[0];
                var claims = parts[1];

                Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
                Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
            }
            else
            {
                if (response.IsHttpError)
                {
                    "HTTP error: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorStatusCode);
                    "HTTP error reason: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorReason);
                }
                else
                {
                    "Protocol error response:".ConsoleGreen();
                    Console.WriteLine(response.Json);
                }
            }
        }
    }
}