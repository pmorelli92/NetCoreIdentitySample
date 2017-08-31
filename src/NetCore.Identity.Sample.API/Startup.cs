using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetCore.Identity.Sample.API.JWT;
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

            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.AddSingleton<JwtIssuerOptions, JwtIssuerOptions>();

            // Set up JWT
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = GetIssuer(jwtAppSettingOptions);
                options.Audience = GetAudience(jwtAppSettingOptions);

                options.SigningCredentials =
                    new SigningCredentials(
                        algorithm: SecurityAlgorithms.HmacSha256,
                        key: GetSignInKey(jwtAppSettingOptions));
            });

            // Set up Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy => policy.RequireClaim("rol", "api_access"));
            });

            services
                .AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = GetSignInKey(jwtAppSettingOptions),
                        ValidateIssuer = true,
                        ValidIssuer = GetIssuer(jwtAppSettingOptions),
                        ValidateAudience = true,
                        ValidAudience = GetAudience(jwtAppSettingOptions),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    };
                });

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
            services.AddScoped(e => new Data.UserContextFactory().CreateDbContext());
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

        private SymmetricSecurityKey GetSignInKey(IConfigurationSection configurationSection)
            => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configurationSection["SecretKey"]));

        private string GetAudience(IConfigurationSection configurationSection)
            => configurationSection["Audience"];

        private string GetIssuer(IConfigurationSection configurationSection)
            => configurationSection["Issuer"];

        #endregion Private Methods
    }
}