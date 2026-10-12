using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageDeliverySystem.Infrastructure.Data;
using PackageDeliverySystem.Infrastructure;
using PackageDeliverySystem.Core.Interfaces;

// setup DI
var services = new ServiceCollection();

// configure SQLite
string connectionString = "Data Source=delivery.db";
services.AddDbContext<DeliveryDbContext>(options =>
    options.UseSqlite(connectionString));

// register repositories (right now only 4 package)
services.AddScoped<IPackageRepository, PackageRepository>();    // whenever class asks for IPackageRepository, give it PackageRepository
var serviceProvider = services.BuildServiceProvider();

// create db file automatically:
using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.EnsureCreated();
}