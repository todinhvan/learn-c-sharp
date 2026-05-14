using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Client
{
    internal class RemoteClientSourceAuthenticationHandler(string authenticationApiUrl) : IClientSourceAuthenticationHandler
    {
        private readonly HttpClient _httpClient = default!;

        public async Task<bool> AuthenticateAsync(string clientSource)
        {
            if (!string.IsNullOrEmpty(clientSource))
            {
                return false;
            }

            var response = await _httpClient.GetAsync($"{authenticationApiUrl}/api/clientsource/{clientSource}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
    }
}
