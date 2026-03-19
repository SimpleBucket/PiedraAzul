using Microsoft.JSInterop;
using PiedraAzul.Client.Services;
using Shared.Grpc;
using System.Net;
using System.Net.Http.Headers;

namespace PiedraAzul.Client.DelegatingHandlers;

public class HttpDelegatingHandler : DelegatingHandler
{
    private readonly IJSRuntime js;
    private readonly RefreshAuthClient refreshClient;

    private static SemaphoreSlim _refreshLock = new(1, 1);

    public HttpDelegatingHandler(
        IJSRuntime js,
        RefreshAuthClient refreshClient)
    {
        this.js = js;
        this.refreshClient = refreshClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await js.InvokeAsync<string>("sessionStorage.getItem", "accessToken");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            // Verificar si otro request ya refrescó el token
            var currentToken = await js.InvokeAsync<string>("sessionStorage.getItem", "accessToken");

            if (currentToken != accessToken)
            {
                var retryRequest = await CloneHttpRequestMessageAsync(request);

                retryRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", currentToken);

                return await base.SendAsync(retryRequest, cancellationToken);
            }

            var newToken = await RefreshTokenAsync();

            if (string.IsNullOrWhiteSpace(newToken))
                return response;

            var newRequest = await CloneHttpRequestMessageAsync(request);

            newRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", newToken);

            return await base.SendAsync(newRequest, cancellationToken);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<string?> RefreshTokenAsync()
    {
        try
        {
            var response = await refreshClient.Client.RefreshTokenAsync(new RefreshTokenRequest());

            if (string.IsNullOrWhiteSpace(response.AccessToken))
                return null;

            await js.InvokeVoidAsync("sessionStorage.setItem", "accessToken", response.AccessToken);

            return response.AccessToken;
        }
        catch
        {
            await js.InvokeVoidAsync("sessionStorage.clear");
            return null;
        }
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;

            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.Add(header.Key, header.Value);
            }
        }

        return clone;
    }
}