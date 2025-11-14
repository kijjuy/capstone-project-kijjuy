using app.Repositories;
using app.Services;
using app.Mappers;
using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace app;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.AddDbContext<IdentityContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection"))
        );

        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>();

        builder.Services.AddAuthorization();

        builder.Logging.AddConsole();

#if DEBUG
        builder.Services.AddControllersWithViews()
            .AddRazorRuntimeCompilation();
#else
	builder.Services.AddControllersWithViews();
#endif

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

        ConfigureEmailServiceOptions(builder);

        builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
        builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

        builder.Services.AddScoped<IProductsService, ProductsService>();
        builder.Services.AddScoped<ICategoriesService, CategoriesService>();
        builder.Services.AddScoped<IImagesService, LocalImagesService>();
        builder.Services.AddScoped<ICheckoutService, CheckoutService>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        builder.Services.AddScoped<IProductMapper, ProductMapper>();

        builder.Services.AddTransient<IEmailSender, EmailService>();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            using (var scope = app.Services.CreateScope())
            {
                String adminPass = builder.Configuration["SeededUsers:Admin"];
                DbInitializer.SetAdminPass(adminPass);
                DbInitializer.SeedUsersAndRoles(scope.ServiceProvider).Wait();
            }
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

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
