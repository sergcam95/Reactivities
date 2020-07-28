using System.Text;
using Application.Activities;
using Application.Interfaces;
using API.Middleware;
using AutoMapper;
using Domain;
using FluentValidation.AspNetCore;
using Infrastructure.Photos;
using Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace API {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            // Database configuration
            services.AddDbContext<DataContext> (opt => {
                opt.UseLazyLoadingProxies ();
                opt.UseSqlite (Configuration.GetConnectionString ("DefaultConnection"));
            });

            // Service for handling CORS
            services.AddCors (opt => {
                opt.AddPolicy ("CorsPolicy", policy => {
                    policy.AllowAnyHeader ().AllowAnyMethod ().WithOrigins ("http://localhost:3000");
                });
            });

            // Add MediatR to the services. We use typeof(List.Handler)
            // in order to locate the assembly that ihas
            // all the handlers. We could've used any other handler
            // inside the assembly
            services.AddMediatR (typeof (List.Handler).Assembly);

            // AutoMapper
            services.AddAutoMapper (typeof (List.Handler));

            // In order to use FluentValidation we need to append
            // .AddFluentValidation to the services.AddControllers()

            services.AddControllers (opt => {
                    // AuthorizationPolicyBuilder is for securing all routes
                    var policy = new AuthorizationPolicyBuilder ().RequireAuthenticatedUser ().Build ();
                    opt.Filters.Add (new AuthorizeFilter (policy));
                })
                .AddFluentValidation (config => {
                    config.RegisterValidatorsFromAssemblyContaining<Create> ();
                });

            // AddIdentity is when we are serving Pages with Razer
            // and using Cookies
            // In this case we use headers from which we receive the token
            // NOTE: SECURITY
            var builder = services.AddIdentityCore<AppUser> ();
            var identityBuilder = new IdentityBuilder (builder.UserType, builder.Services);
            identityBuilder.AddEntityFrameworkStores<DataContext> ();
            identityBuilder.AddSignInManager<SignInManager<AppUser>> ();

            #region Authorization for managing an activity

            // This is the policy for authorizing users to delete or modify
            // an activity only if they are the host
            services.AddAuthorization (opt => {
                opt.AddPolicy ("IsActivityHost", policy => {
                    policy.Requirements.Add (new IsHostRequirement ());
                });
            });

            services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler> ();

            #endregion

            #region Authentication

            // We need to add the Nuget Package Microsoft.AspNetCore.Authentication.JwtBearer
            // We want to tell our API what what we should be validating when we receive a token
            // NOTE: SECURITY
            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (Configuration["TokenKey"]));
            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer (opt => {
                    opt.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateAudience = false,
                    ValidateIssuer = false
                    };
                });

            // Register JwtGenerator service
            // NOTE: SECURITY
            services.AddScoped<IJwtGenerator, JwtGenerator> ();

            services.AddScoped<IUserAccessor, UserAccessor> ();

            #endregion

            #region Cloudinary Services

            services.Configure<CloudinarySettings> (Configuration.GetSection ("Cloudinary"));
            services.AddScoped<IPhotoAccessor, PhotoAccessor> ();

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {

            // All the exceptions are catch in this middleware
            app.UseMiddleware<ErrorHandlingMiddleware> ();

            if (env.IsDevelopment ()) {
                // app.UseDeveloperExceptionPage ();
            }

            // app.UseHttpsRedirection();

            app.UseRouting ();

            // Add the service to the pipeline (as a middleware)
            app.UseCors ("CorsPolicy");

            // NOTE: SECURITY
            app.UseAuthentication ();
            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}