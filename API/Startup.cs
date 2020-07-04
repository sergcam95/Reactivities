using Application.Activities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;

namespace API
{
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            // Database configuration
            services.AddDbContext<DataContext> (opt => {
                opt.UseSqlite (Configuration.GetConnectionString ("DefaultConnection"));
            });

            // Service for handling CORS
            services.AddCors (opt => {
                opt.AddPolicy ("CorsPolicy", policy => {
                    policy.AllowAnyHeader ().AllowAnyMethod ().WithOrigins ("http://localhost:3000");
                });
            });

            // Add MediatR to the services. We use typeof(List.Handler)
            // in order to locate the assembly that it is going to use
            // have all the handlers. We could've use any other handler
            // inside the assembly
            services.AddMediatR(typeof(List.Handler).Assembly);

            services.AddControllers ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            // app.UseHttpsRedirection();

            app.UseRouting ();

            app.UseAuthorization ();

            // Add the service to the pipeline (as a middleware)
            app.UseCors ("CorsPolicy");

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}