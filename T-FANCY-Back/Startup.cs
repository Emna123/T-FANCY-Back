using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using T_FANCY_Back.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using T_FANCY_Back.Services;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace T_FANCY_Back
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

            //DbContext configuration 
            services.AddDbContext<TfancyContext>(opt => opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            //Adds the default identity system configuration for the specified User and Role types.
            services.AddIdentity<User, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TfancyContext>();
                services.AddIdentityCore<Client>().AddRoles<IdentityRole>().AddEntityFrameworkStores<TfancyContext>();
                services.AddIdentityCore<Manager>().AddRoles<IdentityRole>().AddEntityFrameworkStores<TfancyContext>();

            //Gets or sets the amount of time a generated token remains valid. Defaults to 1 day.
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.Name = "Default";
                opt.TokenLifespan = TimeSpan.FromHours(1);
            });

            //Enable cors
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            //use Json.NET configuration
            services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore).AddNewtonsoftJson
                (options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddControllers();
            //token refreshtoken configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWTRefreshTokens", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "This site uses Bearer token and you have to pass" +
                     "it as Bearer<<space>>Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {{
                new OpenApiSecurityScheme
                {
                    Reference=new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id="Bearer"
                   },
                    Scheme="oauth2",
                   Name="Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
                    }
            });
              });
            //configure JWT
            var jwtkey = Configuration.GetValue<string>("JwtSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(jwtkey);
            TokenValidationParameters tokenValidation = new TokenValidationParameters
            {
               IssuerSigningKey=new SymmetricSecurityKey(keyBytes),
               ValidateLifetime=true,
               ValidateAudience=false,
               ValidateIssuer=false,
               ClockSkew =TimeSpan.Zero

            };

            services.AddSingleton(tokenValidation);
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(jwtOptions =>
             {
                 jwtOptions.TokenValidationParameters = tokenValidation;
                 jwtOptions.Events = new JwtBearerEvents();
                 jwtOptions.Events.OnTokenValidated = async(context) =>
                 {
                     var ipAddress = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                     var jwtService = context.Request.HttpContext.RequestServices.GetService<IJwtService>();
                     var jwtToken = context.SecurityToken as JwtSecurityToken;
                     if (!await jwtService.IsTokenValid(jwtToken.RawData, ipAddress)) 
                    context.Fail("Invalid Token Details");
                 };
             });
            services.AddTransient<IJwtService, JwtService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //upload files
            app.UseStaticFiles(); // For the wwwroot folder

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Images")),
                RequestPath = new PathString("/Images")
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", "Images")),
                RequestPath = new PathString("/Images")
            });
            //Enable cors
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
