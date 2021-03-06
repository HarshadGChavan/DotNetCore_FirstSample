using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FirstSample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FirstSample
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
            services.AddDbContextPool<AppDBContext>(optionsAction=>optionsAction.UseSqlServer(Configuration.GetConnectionString("EmployeeDbConnection")));
            services.AddControllersWithViews();
            services.AddTransient<IEmployeeRepository,SQLEmployeeRepository>();
            services.AddIdentity<ApplicationUser,IdentityRole>()
                                            .AddEntityFrameworkStores<AppDBContext>();
            services.Configure<IdentityOptions>(options=>
            {
                options.Password.RequiredLength=5;
                options.Password.RequiredUniqueChars =3;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy=>policy.RequireClaim("Delete Role")
                                  .RequireClaim("Create Role"));
                
                options.AddPolicy("EditRolePolicy",
                  policy => policy.RequireClaim("Edit Role") );
            }  );

            services.AddAuthentication()
                    .AddGoogle(options =>
                            {
                                options.ClientId ="432303751834-e19rl6v33p1dl8oospfso3hisjqkvqq8.apps.googleusercontent.com";
                                options.ClientSecret ="JbvgrGUKYx2ljXeQ0ai2lsKR";
                            }
                    );

            services.AddMvc(config=> {
                var policy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
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
                
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAuthentication();

            //app.UseMvc();

           app.UseRouting();
           
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
