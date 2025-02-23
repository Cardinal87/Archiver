using Archiver.API.DTO.Request;
using Microsoft.AspNetCore.Http.Features;
using System.Net;

namespace Archiver.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseWindowsService();


            builder.Configuration.AddJsonFile("appsettings.json")
                .Build();


            ConfigureServices(builder.Services, builder.Configuration);
            builder.WebHost.ConfigureKestrel(opt =>
            {
                opt.Listen(IPAddress.Loopback, 5091);
                opt.Limits.MaxRequestBodySize = 1024 * 1024 * 512;
            });
            

            var app = builder.Build();
            app.UseCors("MainPolicy");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
        private static void ConfigureServices(IServiceCollection services, IConfiguration conf)
        {
            services.AddControllers();
            services.AddCors(set =>
            {
                set.AddPolicy("MainPolicy", opt => {
                    opt.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .WithMethods("POST");
                });
            });
            services.Configure<OutputOptions>(conf.GetSection(OutputOptions.Section));
        }
    }
}
