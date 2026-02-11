using app.Repositories;
using app.Services;
using app.Mappers;
using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.HttpOverrides;
using System.Globalization;
using Stripe;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.AspNetCore;

namespace app;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

	var cultureInfo = new CultureInfo("en-CA");
	CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
	CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        builder.Services.AddDbContext<IdentityContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection"))
        );

        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>();

        builder.Services.AddAuthorization();

	builder.Services.AddAuthentication().AddGoogle(googleOptions =>
	{
	    googleOptions.ClientId = builder.Configuration["OAuth:GoogleId"]!;
	    googleOptions.ClientSecret = builder.Configuration["OAuth:GoogleSecret"]!;
	    googleOptions.CallbackPath = builder.Configuration["OAuth:GoogleCallback"];
	});

	builder.Logging.ClearProviders();
	if(builder.Environment.IsDevelopment())
	{
	    builder.Host.UseSerilog((context, services, config) => {
		config
		    .ReadFrom.Configuration(context.Configuration)
		    .ReadFrom.Services(services)
		    .Enrich.FromLogContext()
		    .WriteTo.Console();
	    });

	    builder.Services.AddControllersWithViews()
		.AddRazorRuntimeCompilation();
	    Log.Logger.Information("Build is in development");
	} 
	else
	{
	    builder.Host.UseSerilog((context, services, config) => {
		config
		    .ReadFrom.Configuration(context.Configuration)
		    .ReadFrom.Services(services)
		    .Enrich.FromLogContext()
		    .WriteTo.Console()
		    .WriteTo.File("/logs/app.log", rollingInterval: RollingInterval.Day);
	    });

	    builder.Services.AddControllersWithViews();
	    Log.Logger.Information("Build is in production");
	}

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

        builder.Services.Configure<StripeClientOptions>(options =>
        {
            String? apiKey = builder.Configuration["StripeSecrets:ApiKey"];
            if (apiKey == null)
            {
                throw new ArgumentNullException("Stripe Api Key was null");
            }
            options.ApiKey = apiKey;
            StripeConfiguration.ApiKey = apiKey;
        });

        ConfigureEmailServiceOptions(builder);

        builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
        builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

        builder.Services.AddScoped<IProductsService, ProductsService>();
        builder.Services.AddScoped<ICategoriesService, CategoriesService>();
        builder.Services.AddScoped<IImagesService, LocalImagesService>();
        builder.Services.AddScoped<ICheckoutService, app.Services.CheckoutService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        builder.Services.AddScoped<IProductMapper, ProductMapper>();

        builder.Services.AddTransient<IEmailSender, EmailService>();



        var app = builder.Build();

	app.Use(async (context, prev) =>
	{
	    await prev.Invoke();
	    if(context.Response.StatusCode.Equals((int)HttpStatusCode.NotFound) || 
	       context.Response.StatusCode.Equals((int)HttpStatusCode.InternalServerError) ||
	       context.Response.StatusCode.Equals((int)HttpStatusCode.MethodNotAllowed))
	    {
		context.Response.StatusCode = (int)HttpStatusCode.Redirect;
		context.Response.Redirect("/");
	    }
	});

	var options = new ForwardedHeadersOptions
	{
	    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
	};
	options.KnownNetworks.Clear();
	options.KnownProxies.Clear();
	app.UseForwardedHeaders(options);

	app.Use(async (context, next) =>
	{
	    context.Response.Headers.Add(
		"Content-Security-Policy",
		"default-src 'self';" + 
		"script-src 'self' https://cdn.jsdelivr.net;" + 
		"style-src 'self' https://cdn.jsdelivr.net;" + 
		"font-src 'self';" + 
		"img-src 'self' data:;" + 
		"connect-src 'self';" +          
    		"media-src 'self';" +            
    		"frame-src 'none';" +            
    		"object-src 'none';" +           
    		"frame-ancestors 'none';" +      
    		"base-uri 'self';" +            
    		"form-action 'self';" +          
    		"upgrade-insecure-requests;"  
	    );

	    await next();
	});

	app.Use(async (context, next) =>
	{
	    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
	    await next();
	});

	app.Use(async (context, next) =>
	{
	    context.Response.Headers.Add("X-Frame-Options", "DENY");
	    await next();
	});


        using (var scope = app.Services.CreateScope())
        {
            String adminPass = builder.Configuration["SeededUsers:Admin"];
            DbInitializer.SetAdminPass(adminPass);
            DbInitializer.SeedUsersAndRoles(scope.ServiceProvider).Wait();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

	app.UseStaticFiles();

        app.MapControllers();
        app.MapRazorPages();


        app.Run();
    }

    /**
     * <summary>
     * Gets the email options from user secrets, then creates a new EmailServiceOptions from them.
     * </summary>
     */
    private static void ConfigureEmailServiceOptions(WebApplicationBuilder builder)
    {
        String senderName = builder.Configuration["EmailOptions:SenderName"];
        String senderAddress = builder.Configuration["EmailOptions:SenderAddress"];
        String SMTPServerAddress = builder.Configuration["EmailOptions:SMTPServerAddress"];
        String SMTPPortStr = builder.Configuration["EmailOptions:SMTPPort"];
        String SMTPAuthLogin = builder.Configuration["EmailOptions:SMTPAuthLogin"];
        String SMTPAuthPassword = builder.Configuration["EmailOptions:SMTPAuthPassword"];

        if (
            senderName == null ||
            senderAddress == null ||
            SMTPServerAddress == null ||
            SMTPPortStr == null ||
            SMTPAuthLogin == null ||
            SMTPAuthPassword == null
            )
        {
            throw new ArgumentNullException("One or more email options were null.");
        }

        builder.Services.Configure<EmailServiceOptions>(options =>
        {
            options.SenderName = senderName;
            options.SenderAddress = senderAddress;
            options.SMTPServerAddress = SMTPServerAddress;
            options.SMTPPort = int.Parse(SMTPPortStr);
            options.SMTPAuthLogin = SMTPAuthLogin;
            options.SMTPAuthPassword = SMTPAuthPassword;
        });
    }
}
