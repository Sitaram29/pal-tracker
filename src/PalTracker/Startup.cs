using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Hypermedia;
using Steeltoe.Management.Endpoint;
using System.Net;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint.Info;

namespace PalTracker
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
            services.AddCloudFoundryActuators(Configuration, MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
            
            services.AddControllers();

            

            var message = Configuration.GetValue<string>("WELCOME_MESSAGE");
            var memory_limit= Configuration.GetValue<string>("MEMORY_LIMIT");
            var port= Configuration.GetValue<string>("PORT"); 
            var cf_instance_index= Configuration.GetValue<string>("CF_INSTANCE_INDEX");
            var cf_instance_addr= Configuration.GetValue<string>("CF_INSTANCE_ADDR");
            if (string.IsNullOrEmpty(message)){

                throw new ApplicationException("WELCOME_MESSAGE not configured.");
            }
            services.AddScoped<IHealthContributor, TimeEntryHealthContributor>();
            services.AddSingleton(sp => new WelcomeMessage(message));
            services.AddSingleton(cf => new CloudFoundryInfo(port,memory_limit,cf_instance_index,cf_instance_addr));
            services.AddSingleton<IOperationCounter<TimeEntry>, OperationCounter<TimeEntry>>();
            services.AddSingleton<IInfoContributor, TimeEntryInfoContributor>();

            services.AddScoped<ITimeEntryRepository, MySqlTimeEntryRepository>();
            services.AddDbContext<TimeEntryContext>(options => options.UseMySql(Configuration));
            //services.AddSingleton<ITimeEntryRepository, InMemoryTimeEntryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCloudFoundryActuators(MediaTypeVersion.V2, ActuatorContext.ActuatorAndCloudFoundry);
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
