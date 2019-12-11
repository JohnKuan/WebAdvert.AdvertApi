using AdvertApi.HealthChecks;
using AdvertApi.Services;
using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdvertApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonSimpleNotificationService>();
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<IAdvertStorageService, DynamoDBAdvertStorage>();
            services.AddTransient<StorageHealthCheck>();
            services.AddHealthChecks()
                    .AddCheck<StorageHealthCheck>("Storage");
            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigin", policy => policy.WithOrigins("*").AllowAnyHeader());
            });
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseHealthChecks("/health");
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });
        }
    }
}