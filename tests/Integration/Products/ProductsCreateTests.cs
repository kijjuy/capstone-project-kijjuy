using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using app;

namespace tests;

public class ProductsCreateTests
{

#region CreateProductsIntegration

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

    [Fact]
    public async Task CreateGet_RegularUserLoggedIn_RequestForbidden()
    {
	//arrange
	var factory = new CustomWebApplicationFactory<Program>();
	var client = AuthClientBuilder.BuildAdminAuthClient<TestRegularUserAuthHandler>(factory);

	DbHelper.initDb(factory.connectionString);

	//act
	var response = await client.GetAsync("/products/create");
	var body = await response.Content.ReadAsStringAsync();

	//assert
	Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateNewProduct_ValidInfo_CreatesProduct()
    {
	var factory = new CustomWebApplicationFactory<Program>();
	var client = AuthClientBuilder.BuildAdminAuthClient<TestAdminUserAuthHandler>(factory);

	DbHelper.initDb(factory.connectionString);

	var formData = new Dictionary<String, String>();
	formData["Name"] = "test";
	formData["CategoryId"] = "1";
	formData["Price"] = "123.45";
	formData["Description"] = "Test description";

	var content = new FormUrlEncodedContent(formData);
	var response = await client.PostAsync("/products/create", content);

	var body = await response.Content.ReadAsStringAsync();

	Assert.Equal(HttpStatusCode.Found, response.StatusCode);
	Assert.Equal("/products/1", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreateNewProduct_BadInfo_ReturnsError()
    {
	var factory = new CustomWebApplicationFactory<Program>();
	var client = AuthClientBuilder.BuildAdminAuthClient<TestAdminUserAuthHandler>(factory);

	DbHelper.initDb(factory.connectionString);

	var badTestData = new List<Dictionary<String,String>>();

	{ // No Description
	    var formData = new Dictionary<String, String>();
	    formData["Name"] = "test";
	    formData["CategoryId"] = "1";
	    formData["Price"] = "123.45";
	    badTestData.Add(formData);
	}
	{ // No Price
	    var formData = new Dictionary<String, String>();
	    formData["Name"] = "test";
	    formData["CategoryId"] = "1";
	    formData["Description"] = "test description";
	    badTestData.Add(formData);
	}
	{ // No CategoryId
	    var formData = new Dictionary<String, String>();
	    formData["Name"] = "test";
	    formData["Price"] = "123.45";
	    formData["Description"] = "test description";
	    badTestData.Add(formData);
	}
	{ // No Name
	    var formData = new Dictionary<String, String>();
	    formData["CategoryId"] = "1";
	    formData["Price"] = "123.45";
	    formData["Description"] = "test description";
	    badTestData.Add(formData);
	}
	{ // Bad Name - Low
	    var formData = new Dictionary<String, String>();
	    formData["Name"] = "aa";
	    formData["CategoryId"] = "1";
	    formData["Price"] = "123.45";
	    formData["Description"] = "test description";
	    badTestData.Add(formData);
	}
	{ // Bad Name - High
	    var formData = new Dictionary<String, String>();
	    formData["Name"] = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"; //101 chars
	    formData["CategoryId"] = "1";
	    formData["Price"] = "123.45";
	    formData["Description"] = "test description";
	    badTestData.Add(formData);
	}
	{ // Bad categoryId - Low
	    var formData = new Dictionary<String, String>();
	    formData["Name"] = "Test";
	    formData["CategoryId"] = "0";
	    formData["Price"] = "123.45";
	    formData["Description"] = "test description";
	    badTestData.Add(formData);
	}
	{ // Bad categoryId - High
	    var formData = new Dictionary<String, String>();
	    formData["Name"] = "Test";
	    formData["CategoryId"] = "21";
	    formData["Price"] = "123.45";
	    formData["Description"] = "test description";
	    badTestData.Add(formData);
	}

	foreach(var formData in badTestData) {
	    var content = new FormUrlEncodedContent(formData);
	    var response = await client.PostAsync("/products/create", content);

	    var body = await response.Content.ReadAsStringAsync();

	    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	    Assert.Contains("Create Product", body);
	}
    }

#endregion

}
