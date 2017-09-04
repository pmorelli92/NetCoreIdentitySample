using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetCore.Identity.Sample.API.Tokens;
using System;
using System.IO.Compression;
using System.Text;
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
            ConfigureHTTPS(services);
            ConfigureIdentity(services);
            ConfigureDatabase(services);
            ConfigureGzipCompression(services);

            var tokenValidation = ConfigureJWTToken(services);
            
            // Enable Authenticaton
            services
                .AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt => opt.TokenValidationParameters = tokenValidation);

            // Add Mvc with default configuration
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseWebMarkupMin();
            app.UseAuthentication();
            app.UseMvc(routeBuilder => Routing.ConfigureRoutes(routeBuilder));
        }

        #region Private Methods

        private void ConfigureGzipCompression(IServiceCollection services)
        {
            services
                .AddWebMarkupMin(opt => opt.AllowCompressionInDevelopmentEnvironment = true)
                .AddHttpCompression(options =>
                    options.CompressorFactories = new[] {
                        new GZipCompressorFactory(new GZipCompressionSettings { Level = CompressionLevel.Fastest })
                }
            );
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            // We do not use services.AddDbContext since our config is in the factory
            // This line can be replaced with Autofac or other DI container
            services.AddScoped(e => new Data.UserContextFactory(Configuration).CreateDbContext());
        }

        private void ConfigureHTTPS(IServiceCollection services)
        {
            // TODO: Set the production certificates
            if (Environment.IsDevelopment() || Environment.IsStaging())
            {
                services.Configure<MvcOptions>(opt =>
                {
                    opt.Filters.Add(new RequireHttpsAttribute());
                    opt.SslPort = Configuration.GetValue<int>("iisSettings:iisExpress:sslPort");
                });
            }
        }

        private void ConfigureIdentity(IServiceCollection services)
        {
            services
                .AddIdentity<Entities.User, Entities.Role>(opt =>
                {
                    opt.Password.RequiredLength = 4;
                    opt.Password.RequireDigit = false;
                    opt.Password.RequiredUniqueChars = 0;
                    opt.Password.RequireLowercase = false;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireNonAlphanumeric = false;
                })
            .AddEntityFrameworkStores<Data.UserContext>()
            .AddDefaultTokenProviders();
        }

        private TokenValidationParameters ConfigureJWTToken(IServiceCollection services)
        {
            // Set up JWT
            var issuer = Configuration.GetValue<string>("Jwt:Issuer");
            var audience = Configuration.GetValue<string>("Jwt:Audience");
            var secretKey = Configuration.GetValue<string>("Jwt:SecretKey");
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            // This line can be replaced with Autofac or other DI container
            services.AddSingleton(e =>
                new JwtFactory(
                    issuer,
                    audience,
                    new SigningCredentials(
                        key: signingKey,
                        algorithm: SecurityAlgorithms.HmacSha256)));

            return new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = signingKey,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
            };
        }

        #endregion Private Methods
    }
}