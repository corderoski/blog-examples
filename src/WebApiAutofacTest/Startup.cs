using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Autofac;
using WebApiAutofacTest.Services;
using Autofac.Integration.WebApi;
using Autofac.Extensions.DependencyInjection;

namespace WebApiAutofacTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var container = new ContainerBuilder();

            container.Populate(services);

            var thisAssembly = typeof(Startup).Assembly;
            container.RegisterAssemblyTypes(thisAssembly)
                .Where(p => p.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .InstancePerRequest();

            container.RegisterApiControllers(thisAssembly)
                .InstancePerRequest();

            container.RegisterType<RandomService>().As<IRandomService>();

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
