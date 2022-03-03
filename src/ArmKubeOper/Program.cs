using System.Threading.Tasks;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using KubeOps.Operator;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ArmKubeOper
{

    public static class Program
    {

        public static async Task Main(string[] args) =>
            await Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(b => b.RegisterModule<AssemblyModule>()))
                .ConfigureWebHostDefaults(w => w.UseStartup<Startup>())
                .Build()
                .RunOperatorAsync(args);

    }

}
