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
    private readonly ITokenService tokenService;

    private static SemaphoreSlim _refreshLock = new(1, 1);

    public HttpDelegatingHandler(
        IJSRuntime js,
        RefreshAuthClient refreshClient,
        ITokenService tokenService)
    {
        this.js = js;
        this.refreshClient = refreshClient;
        this.tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
           HttpRequestMessage request,
           CancellationToken cancellationToken)
    {
        var token = await tokenService.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        var newToken = await tokenService.RefreshTokenAsync();

        if (string.IsNullOrWhiteSpace(newToken))
            return response;

        var newRequest = await CloneHttpRequestMessageAsync(request);

        newRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", newToken);

        return await base.SendAsync(newRequest, cancellationToken);
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