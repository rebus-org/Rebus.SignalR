using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.SignalR.Samples.Hubs;
using Rebus.SignalR.Samples.Options;

namespace Rebus.SignalR.Samples
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        private static string GenerateTransientQueueName(string inputQueueName)
        {
            return $"{inputQueueName}-{Environment.MachineName}-{Guid.NewGuid().ToString()}";
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR()
                .AddRebusBackplane<ChatHub>();

            var rabbitMqOptions = Configuration.GetSection(nameof(RabbitMqOptions)).Get<RabbitMqOptions>();
            
            var rabbitMqConnectionString = $"amqp://{rabbitMqOptions.User}:{rabbitMqOptions.Password}@{rabbitMqOptions.Host}:{rabbitMqOptions.Port.ToString()}";

            services.AddRebus(configure => configure
                .Transport(x =>
                {
                    x.UseRabbitMq(rabbitMqConnectionString, GenerateTransientQueueName("Rebus.SignalR"))
                    .InputQueueOptions(o =>
                    {
                        o.SetAutoDelete(true);
                        o.SetDurable(false);
                    });
                })
                .Options(o => o.EnableSynchronousRequestReply())
                .Routing(r => r.TypeBased()));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}
