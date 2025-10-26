using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using app;

namespace tests;

public class ProductsCreateTests
{
    [Fact]
    public async Task CreateGet_LoggedOut_RedirectsToLogin()
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
    public async Task CreateGet_AdminLoggedIn_GetsCreatePage()
    {
        //arrange
        var factory = new CustomWebApplicationFactory<Program>();
	var client = AuthClientBuilder.BuildAdminAuthClient<TestAdminUserAuthHandler>(factory);


        DbHelper.initDb(factory.connectionString);

        //act

        var response = await client.GetAsync("/products/create");
        var body = await response.Content.ReadAsStringAsync();

        //assert
	response.EnsureSuccessStatusCode();

        Assert.Contains("Create Product", body);
    }

    //[Fact]
    public async Task CreateGet_RegularUserLoggedIn_GetsUnauthorizedRedirect()
    {
	var factory = new CustomWebApplicationFactory<Program>();
	var client = AuthClientBuilder.BuildAdminAuthClient<TestRegularUserAuthHandler>(factory);

	DbHelper.initDb(factory.connectionString);

    }
}
