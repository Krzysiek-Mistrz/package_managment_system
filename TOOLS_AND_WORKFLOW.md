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

