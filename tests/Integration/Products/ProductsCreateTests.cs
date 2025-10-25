using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
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
    public async Task AdminLoggedIn_GetsCreatePage()
    {
        //arrange
	var client = AuthClientBuilder.BuildAdminAuthClient<TestAdminUserAuthHandler>();
        var factory = new CustomWebApplicationFactory<Program>();


        DbHelper.initDb(factory.connectionString);

        //act

        var response = await client.GetAsync("/products/create");
        var body = await response.Content.ReadAsStringAsync();

        //assert
	response.EnsureSuccessStatusCode();

        Assert.Contains("Create Product", body);
    }

    //[Fact]
    public async Task RegularUserLoggedIn_GetsUnauthorizedRedirect()
    {
	var factory = new CustomWebApplicationFactory<Program>();
	var client = AuthClientBuilder.BuildAdminAuthClient<TestRegularUserAuthHandler>();
    }
}
