using app.Repositories;
using app.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace app;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<IdentityContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection"))
        );

        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<IdentityContext>();

        builder.Services.AddAuthorization();

        builder.Logging.AddConsole();

        builder.Services.AddControllersWithViews();

        builder.WebHost.ConfigureKestrel((context, serverOptions) =>
        {
            var kestralSection = context.Configuration.GetSection("Kestral");
            serverOptions.Configure(kestralSection)
            .Endpoint("HTTP", listenOptions =>
            {

            });
        });

        builder.Services.AddSingleton<ReaderMapper>();

        String? conString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (conString == null)
        {
            throw new ArgumentNullException("Connection string was null.");
        }

        builder.Services.Configure<RepositoryOptions>(options =>
        {
            options.ConnectionString = conString;
        });

        builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
        builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();

        builder.Services.AddScoped<IProductsService, ProductsService>();
        builder.Services.AddScoped<ICategoriesService, CategoriesService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
