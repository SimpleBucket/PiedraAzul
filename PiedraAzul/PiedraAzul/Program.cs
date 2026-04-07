#region NameSpaces
using PiedraAzul.Components;
using PiedraAzul.Extensions;
using PiedraAzul.Application;
using PiedraAzul.Infrastructure;
#endregion

var builder = WebApplication.CreateBuilder(args);

// 🔹 capas
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// 🔹 UI
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// 🔹 API stuff
builder.Services.AddSignalR();
builder.Services.AddGrpc();

// 🔹 Auth
builder.Services.AddAuth(builder.Configuration);

var app = builder.Build();

// middlewares
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// endpoints
app.MapGrpcServices();
app.MapHubs();
app.MapRazorComponents<App>();

// seed
await app.SeedAsync();

app.Run();