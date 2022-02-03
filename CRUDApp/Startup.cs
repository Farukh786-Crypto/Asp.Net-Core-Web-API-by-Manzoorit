using CRUDApp.Data;
using CRUDApp.DataModels;
using CRUDApp.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDApp
{
    public class Startup
    {
        private TokenValidationParameters tokenValidationParameter;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(
                  /*  // Add Exception filter At Application level inside configureservice method
                        confg => confg.Filters.Add(new MyExceptionFilter())
                    )
                    .AddXmlSerializerFormatters()
                    .AddXmlDataContractSerializerFormatters();*/
                  conf=> conf.Filters.Add(new AuthorizeFilter()))
                .AddXmlSerializerFormatters()
                .AddXmlDataContractSerializerFormatters();

            services.AddDbContext<OrganizationDbContext>();

            // Add Identity
            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<OrganizationDbContext>();

            // Step - 1: Create signingkey from secretekey

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-jwt-secret-key"));


            // Step - 2: Create Validation Parameters using signing key
            var tokenValidationParameter = new TokenValidationParameters()
            {
                IssuerSigningKey = signingKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Step - 3: Set Authentication Type as JwtBearer
            services.AddAuthentication(x => x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)

                // Step - 4: Set Validation Parameter created above
                .AddJwtBearer(jwt =>
                {
                    jwt.TokenValidationParameters = tokenValidationParameter;
                }
                );

            // cookie based authentication
            services.ConfigureApplicationCookie(opt=>
            {
                opt.Events = new CookieAuthenticationEvents
                {
                    // Authentication
                    OnRedirectToLogin = redirectContext =>
                      {
                          redirectContext.HttpContext.Response.StatusCode = 403; // Forbidden 
                          return Task.CompletedTask;
                      },
                    // Authorization
                    OnRedirectToAccessDenied=redirectContext =>
                    {
                        redirectContext.HttpContext.Response.StatusCode = 401; // Unauthorize
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CRUDApp", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRUDApp v1"));
            }

            app.UseExceptionHandler(
               options =>
               {
                   options.Run(async context =>
                   {
                       context.Response.StatusCode = 500; // Internal Server Error
                       context.Response.ContentType = "application/json";
                       var ex = context.Features.Get<IExceptionHandlerFeature>();
                       if (ex != null)
                       {
                           await context.Response.WriteAsync(ex.Error.Message);
                           //await context.Response.WriteAsync("We are Working on it! From Middleware ");
                       }
                   });
               }
           );
            app.UseHttpsRedirection();
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
