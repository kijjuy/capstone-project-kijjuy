using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using app;

namespace tests;

public class ProductsCreateTests
{
    [Fact]
    public async Task LoggedOut_RedirectsToLogin()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        DbHelper.initDb(factory.connectionString);

        //act

        var response = await client.GetAsync("/products/create");

        //assert
        Assert.Equal(response.StatusCode, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task LoggedIn_GetsCreatePage()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(defaultScheme: "TestScheme")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                "TestScheme", options => { });
            });
        })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "TestScheme");

        DbHelper.initDb(factory.connectionString);

        //act

        var response = await client.GetAsync("/products/create");
        var body = await response.Content.ReadAsStringAsync();

        //assert
        response.EnsureSuccessStatusCode();

        Assert.Contains("Create", body);
    }
}
