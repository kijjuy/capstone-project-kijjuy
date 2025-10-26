using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using app.Repositories;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{

    public CustomWebApplicationFactory()
    : base()
    {
        String dbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, $"test_{Guid.NewGuid()}.db"));
        connectionString = $"Data Source={dbPath}";
    }

    public String connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<RepositoryOptions>();
            services.Configure<RepositoryOptions>(options =>
            {
                options.ConnectionString = connectionString;
            });
        });
    }
}

