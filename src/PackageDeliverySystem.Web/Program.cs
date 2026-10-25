using PackageDeliverySystem.Web.Components;
using PackageDeliverySystem.Core;
using PackageDeliverySystem.Web.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using PackageDeliverySystem.Infrastructure;
using PackageDeliverySystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IDeliveryDataService, MockDeliveryDataService>();
builder.Services.AddDbContext<DeliveryDbContext>(options => 
    options.UseMySQL(DatabaseSettings.GetConnectionString()));
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IBoxRepository, BoxRepository>();
builder.Services.AddScoped<IPackageAcceptanceService, PackageAcceptanceService>();
builder.Services.AddScoped<IPackageFulfillmentService, PackageFulfillmentService>();
builder.Services.AddScoped<IPackageIntakeService, PackageIntakeService>();
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();
builder.Services.AddScoped<IPeopleService, PeopleService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<INotificationService, BackgroundNotificationService>();

builder.Services.AddHangfire(config => config.UseInMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<IPackageAcceptanceService>(
    "expire-packages",
    service => service.ProcessExpiredPackagesAsync(),
    Cron.Minutely()
);

RecurringJob.AddOrUpdate<IPackageIntakeService>(
    "release-missed-deposits",
    service => service.ProcessMissedDepositsAsync(),
    Cron.Minutely()
);

app.Run();
