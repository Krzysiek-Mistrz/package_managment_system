namespace PackageDeliverySystem.Infrastructure.Data;

public static class DatabaseSettings
{
    private const string ConnectionStringVariable = "PDS_CONNECTION_STRING";

    /// <summary>
    /// reads the mysql connection string from the environment, shared by runtime and design-time
    /// </summary>
    public static string GetConnectionString()
        => Environment.GetEnvironmentVariable(ConnectionStringVariable)
           ?? throw new InvalidOperationException($"Environment variable {ConnectionStringVariable} is not set.");
}
