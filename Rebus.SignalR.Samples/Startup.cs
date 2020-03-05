using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.SignalR.Samples.Hubs;

namespace Rebus.SignalR.Samples
{
    public class Startup
    {
        private static string GenerateTransientQueueName(string inputQueueName)
        {
            return $"{inputQueueName}-{Environment.MachineName}-{Guid.NewGuid()}";
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR()
                .AddRebusBackplane<ChatHub>();

            services.AddRebus(configure => configure
                .Transport(x =>
                {
                    x.UseRabbitMq("amqp://guest:guest@localhost:5672", GenerateTransientQueueName("Rebus.SignalR"))
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

            app.ApplicationServices.UseRebus();

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
