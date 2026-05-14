using Microsoft.Extensions.Configuration;

namespace Authentication.Client
{
    public class ClientSourceAuthenticationHandlerFactory
    {
        public static IClientSourceAuthenticationHandler CreateClientSourceAuthenticationHandler(IConfiguration configuration)
        {
            var service = configuration["AuthenticationService"];
            switch (service)
            {
                case "Remote":
                    {
                        return new RemoteClientSourceAuthenticationHandler(configuration.GetConnectionString("RemoteAuthenticationService") ?? throw new("Missing RemoteAuthenticationService connection string"));
                    }
                case "SqlServer":
                    {
                        return new SqlServerClientSourceAuthenticationHandler(configuration.GetConnectionString("SqlServerAuthenticationService") ?? throw new("Missing SqlServerAuthenticationService connection string"));
                    }
                case "MongoDb":
                    {
                        return new MongoDbClientSourceAuthenticationHandler(configuration.GetConnectionString("MongoDbAuthenticationService") ?? throw new("Missing MongoDbAuthenticationService connection string"));
                    }
                default:
                    {
                        throw new Exception("Invalid authentication service type");
                    }
            }
        }
    }
}
