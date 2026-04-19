#region NameSpaces
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using PiedraAzul.Client.Services.GraphQLServices;
using PiedraAzul.Components;
using PiedraAzul.Extensions;
using PiedraAzul.Application;
using PiedraAzul.Infrastructure;
using PiedraAzul.Client.Extensions;
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
builder.Services.AddPiedraAzulGraphQL();
builder.Services.AddHttpContextAccessor();

// InteractivityAuto – registers shared client services + PersistentAuthenticationStateProvider
var graphqlUrl = builder.Configuration["GraphQLUrl"] ?? "https://localhost:7128";
var hubUrl = builder.Configuration["hubUrl"] ?? "https://localhost:7128";
builder.Services.AddClientServer(graphqlUrl, hubUrl);

// Override auth state provider for server-side: reads from HttpContext, persists to WASM
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

// Override GraphQL client for SSR: forwards the incoming request cookie to the outgoing HTTP call
builder.Services.AddScoped<GraphQLHttpClient>(sp =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    var handler = new CookieForwardingHandler(accessor);
    return new GraphQLHttpClient(new HttpClient(handler) { BaseAddress = new Uri(graphqlUrl) });
});

// 🔹 Auth
builder.Services.AddAuth(builder.Configuration);

var app = builder.Build();

// middlewares
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// endpoints
app.MapGraphQLEndpoint();
app.MapHubs();
app.MapApiEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PiedraAzul.Client._Imports).Assembly);

// seed
await app.SeedAsync();

app.Run();
