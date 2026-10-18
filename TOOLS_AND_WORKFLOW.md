## useful

```bash
dotnet tool install -g dotnet-format
dotnet format
dotnet format --verify-no-changes
```

## tests

```bash
dotnet new xunit -n MyProject.Tests
dotnet add reference ../MyProject/MyProject.csproj
dotnet test
```


## database (mysql)

```bash
docker compose up -d
export PDS_CONNECTION_STRING="Server=localhost;Port=3306;Database=package_delivery_system;User=package_delivery_system_user;Password=package_delivery_system_password;"
dotnet tool restore
dotnet ef migrations add <Name> --project src/PackageDeliverySystem.Infrastructure --output-dir Data/Migrations
dotnet run --project src/PackageDeliverySystem.Cli
```
