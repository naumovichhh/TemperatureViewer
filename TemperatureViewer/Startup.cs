using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Data.SqlClient;
using TemperatureViewer.Database;
using TemperatureViewer.Services;
using TemperatureViewer.Repositories;
using TemperatureViewer.SignalR;

namespace TemperatureViewer
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
            services.AddControllersWithViews();
            var defaultString = Configuration.GetConnectionString("DefaultConnection");
            if (CheckConnection(defaultString))
            {
                services.AddDbContext<DefaultContext>(options =>
                    options.UseSqlServer(defaultString));
            }
            else
            {
                var localDBString = Configuration.GetConnectionString("LocalDBConnection");
                services.AddDbContext<DefaultContext>(options =>
                    options.UseSqlServer(localDBString));
            }
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ISensorsRepository, SensorsRepository>();
            services.AddScoped<ILocationsRepository, LocationsRepository>();
            services.AddScoped<IObserversRepository, ObserversRepository>();
            services.AddScoped<IValuesRepository, ValuesRepository>();
            services.AddScoped<IThresholdsRepository, ThresholdsRepository>();
            services.AddScoped<InformationService>();
            services.AddScoped<AccountService>();
            services.Configure<HostOptions>(hostOptions => hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddSignalR();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
                options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
            });
        }

        private bool CheckConnection(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "";
            try 
            {
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
            }
            catch (SqlException ex)
            {
                return false;
            }
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapHub<TemperatureHub>("/updateTemperature");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
