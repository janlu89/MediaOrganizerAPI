using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MusicFileAPI.Interfaces;
using MusicFileAPI.Model;
using MusicFileAPI.Services;

namespace MusicFileAPI
{
    public class Startup
    {
        private const string CorsPolicyName = "MyPolicy";
        private const string SwaggerUrl = "v1/swagger.json";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true);
            services.AddControllersWithViews()
                .AddFluentValidation(opt =>
                {
                    opt.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                });
            services.AddRazorPages();

            ConfigureCors(services);

            services.AddSingleton<ICloudStorage, AzureStorage>();
            services.AddSingleton<IStorageConnectionFactory, StorageConnectionFactory>(serviceProvider => 
            {
                CloudStorageOptions cloudStorageOptions = new CloudStorageOptions();
                cloudStorageOptions.ConnectionString = Configuration["AzureBlobStorage:ConnectionString"];
                cloudStorageOptions.Container = Configuration["AzureBlobStorage:BlobContainer"];
                return new StorageConnectionFactory(cloudStorageOptions);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Music File API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(CorsPolicyName);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(SwaggerUrl, "Music File API");
            });
        }

        private void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName,
                        builder => builder
                                    .AllowAnyHeader()
                                    .AllowAnyOrigin()
                                    .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
                                    .WithMethods(new string[] { "OPTIONS", "GET", "POST", "HEAD", "PUT", "DELETE", "PATCH" }));
            });
        }
    }
}
