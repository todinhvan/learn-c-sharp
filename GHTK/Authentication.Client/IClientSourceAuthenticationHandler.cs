using System;
using System.Collections.Generic;
using System.Text;

namespace Authentication.Client
{
    public interface IClientSourceAuthenticationHandler
    {
        Task<bool> AuthenticateAsync(string clientSource);
    }
}
