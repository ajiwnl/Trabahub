using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using Trabahub.Data;

namespace Trabahub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.Configure<StripeSetting>(Configuration.GetSection("Stripe"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];
            app.UseSession();
        }
    }
}
