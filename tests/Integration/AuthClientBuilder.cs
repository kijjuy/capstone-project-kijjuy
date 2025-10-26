using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using app;

namespace tests;

public static class AuthClientBuilder
{

    /**
     * <summary>
     * Builds a new HttpClient with a mock authentication handler.
     * The authentication scheme is determined by <typeparamref name="TAuthHandler"/>.
     * </summary>
     */
    public static HttpClient BuildAdminAuthClient<TAuthHandler>(CustomWebApplicationFactory<Program> factory)
    where TAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(defaultScheme: "TestScheme")
            .AddScheme<AuthenticationSchemeOptions, TAuthHandler>(
                "TestScheme", options => { });
            });
        })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "TestScheme");
        return client;
    }
}
