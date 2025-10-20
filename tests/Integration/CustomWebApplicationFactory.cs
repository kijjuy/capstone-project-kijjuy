using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var testSettings = new Dictionary<String, String?>();
            testSettings["ConnectionStrings:DefaultConnection"] = "Data Source=../test.db;Mode=Memory;Cache=Shared";

            configBuilder.AddInMemoryCollection(testSettings);
        });
    }
}
