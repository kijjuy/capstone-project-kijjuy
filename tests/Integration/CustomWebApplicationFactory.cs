using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using app.Repositories;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{

    public static readonly String dbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "test.db"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<RepositoryOptions>();
            services.Configure<RepositoryOptions>(options =>
            {
                options.ConnectionString = $"Data Source={dbPath}";
            });
        });
    }
}
