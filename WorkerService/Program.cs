using WorkerService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EmailService.Services;
using EmailService.Repositories;
using EmailService.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<EmailServiceDbContext>(options =>
            options.UseNpgsql(hostContext.Configuration.GetConnectionString("EmailServiceConnection")));
        
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        
        services.AddHostedService<Worker>();
    });

await builder.RunConsoleAsync();
