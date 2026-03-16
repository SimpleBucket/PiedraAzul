using Microsoft.JSInterop;
using Shared.Grpc;

namespace PiedraAzul.Client.Services;

public class AuthenticationService
{
    private readonly AuthService.AuthServiceClient authClient;
    private readonly IJSRuntime js;

    public AuthenticationService(AuthService.AuthServiceClient authClient, IJSRuntime js)
    {
        this.authClient = authClient;
        this.js = js;
    }

    public async Task<bool> Login(string email, string password)
    {
        var response = await authClient.LoginAsync(new LoginRequest
        {
            Email = email,
            Password = password
        });

        if (response == null || string.IsNullOrEmpty(response.AccessToken))
            return false;

        await js.InvokeVoidAsync("localStorage.setItem", "authToken", response.AccessToken);

        return true;
    }

    public async Task Logout()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", "authToken");
    }

    public async Task<string?> GetToken()
    {
        return await js.InvokeAsync<string>("localStorage.getItem", "authToken");
    }
}