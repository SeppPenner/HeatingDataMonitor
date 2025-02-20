using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataHandler;
using DataHandler.Services;
using RaspberryPIUtils;
using DataHistory;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace WebCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<RaspberryPI>();
            services.AddSingleton<Config>(Program.Config);
            services.AddSingleton<DataStorage>();

#if DEBUG
            services.AddHostedService<MockDataService>();
#endif
#if RELEASE
            services.AddHostedService<SerialDataService>();
#endif

            bool historyIsDefined = !String.IsNullOrWhiteSpace(Program.Config.HistorySQLiteConnectionString) 
                && Program.Config.HistorySaveDelayInMinutes.HasValue;
            if (historyIsDefined)
            {
                services.AddDbContextPool<HeatingDataContext>(optionsBuilder =>
                    optionsBuilder.UseSqlite(Program.Config.HistorySQLiteConnectionString));

                services.AddHostedService<HistoryService>();
                services.AddScoped<IHistoryRepository, HistoryRepository>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
