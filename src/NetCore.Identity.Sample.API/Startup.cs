﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNetCore2;

namespace NetCore.Identity.Sample.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder =
                new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("Properties/launchSettings.json", optional: false, reloadOnChange: true);

            Environment = env;
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddWebMarkupMin(opt => opt.AllowCompressionInDevelopmentEnvironment = true)
                .AddXmlMinification()
                .AddHtmlMinification()
                .AddHttpCompression(options =>
                    options.CompressorFactories = new[] {
                        new GZipCompressorFactory(new GZipCompressionSettings { Level = CompressionLevel.Fastest })
                    }
                );

            // Development SSL
            // TODO: Set logic for production since this will only use dev certificate
            if (Environment.IsDevelopment() || Environment.IsStaging())
            {
                services.Configure<MvcOptions>(opt =>
                {
                    opt.Filters.Add(new RequireHttpsAttribute());
                    opt.SslPort = Configuration.GetValue<int>("iisSettings:iisExpress:sslPort");
                });
            }
            
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseWebMarkupMin();
            app.UseMvc(routeBuilder => Routing.ConfigureRoutes(routeBuilder));
        }
    }
}