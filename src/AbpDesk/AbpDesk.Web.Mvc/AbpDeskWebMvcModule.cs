﻿using AbpDesk.EntityFrameworkCore;
using AbpDesk.Web.Mvc.Navigation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.AspNetCore.EmbeddedFiles;
using Volo.Abp.AspNetCore.Modularity;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;

namespace AbpDesk.Web.Mvc
{
    [DependsOn(
        typeof(AbpAspNetCoreEmbeddedFilesModule),
        typeof(AbpAspNetCoreMvcUiBootstrapModule), 
        typeof(AbpDeskApplicationModule), 
        typeof(AbpDeskEntityFrameworkCoreModule),
        typeof(AbpIdentityHttpApiClientModule),
        typeof(AbpIdentityWebModule)
        )]
    public class AbpDeskWebMvcModule : AbpModule
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            var hostingEnvironment = services.GetSingletonInstance<IHostingEnvironment>();
            var configuration = BuildConfiguration(hostingEnvironment);

            AbpDeskDbConfigurer.Configure(services, configuration);

            services.Configure<NavigationOptions>(options =>
            {
                options.MenuContributors.Add(new MainMenuContributor());
            });

            services.Configure<AbpIdentityHttpApiClientOptions>(configuration.GetSection("AbpIdentity:HttpApiClient"));

            services.AddMvc();
            services.AddAssemblyOf<AbpDeskWebMvcModule>();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();

            context.GetLoggerFactory().AddConsole().AddDebug();

            if (context.GetEnvironment().IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseEmbeddedFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "defaultWithArea",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static IConfigurationRoot BuildConfiguration(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}
