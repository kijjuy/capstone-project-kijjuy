namespace app;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Logging.AddConsole();

        builder.Services.AddControllers();

        //Setup kestral to host on port 8080
        builder.WebHost.ConfigureKestrel((context, serverOptions) =>
        {
            var kestralSection = context.Configuration.GetSection("Kestral");
            serverOptions.Configure(kestralSection)
            .Endpoint("HTTP", listenOptions =>
            {

            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllerRoute();

        app.Run();
    }
}
