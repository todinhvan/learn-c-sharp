/*
        
         CREATE TABLE ClientSources
        (
	        ClientId	NVARCHAR(60) NOT NULL,
	        ValidFrom	DATETIME NOT NULL,
	        ValidTo		DATETIME NOT NULL,
	        IsEnable	BIT NOT NULL,

	        CONSTRAINT  pk_ClientSources PRIMARY KEY (ClientId),
	        -- INDEX		idx_ClientSources (ClientId, ValidFrom, ValidTo, IsEnable) -- we don't actually need this since primary key is included in the select query
        )
        
*/
using Microsoft.Data.SqlClient;

namespace Authentication.Client
{
    public class SqlServerClientSourceAuthenticationHandler : IClientSourceAuthenticationHandler, IDisposable
    {
        private readonly SqlConnection _sqlConnection;
        private bool disposedValue = false;

        public SqlServerClientSourceAuthenticationHandler(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
        }
        
        public async Task<bool> AuthenticateAsync(string clientSource)
        {
            if (_sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                _sqlConnection.Open();
            }

            string query = "SELECT TOP 1 1 FROM ClientSources WHERE ClientId = @ClientSource AND GETDATE() >= ValidFrom AND GETDATE() <= ValidTo AND IsEnable = 1";
            var command = new SqlCommand(query, _sqlConnection);
            command.Parameters.AddWithValue("@ClientSource", clientSource);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                if (_sqlConnection.State == System.Data.ConnectionState.Open)
                {
                    _sqlConnection.Close();
                }
                _sqlConnection.Dispose();
                disposedValue = true;
            }
        }
    }
}
