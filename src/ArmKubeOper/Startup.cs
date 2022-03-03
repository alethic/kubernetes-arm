using DotnetKubernetesClient;

using k8s;

using KubeOps.Operator;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ArmKubeOper
{

    public class Startup
    {

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddKubernetesOperator(o => o.Name = "arm");
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseKubernetesOperator();
        }

    }

}
