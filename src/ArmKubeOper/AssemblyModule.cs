using ArmKubeOper.Services;

using Autofac;

using Azure.Identity;
using Azure.ResourceManager;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

namespace ArmKubeOper
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<Cogito.Autofac.AssemblyModule>();
            builder.RegisterModule<Cogito.Serilog.Autofac.AssemblyModule>();
            builder.RegisterModule<Cogito.Extensions.Logging.Autofac.AssemblyModule>();
            builder.RegisterModule<Cogito.Extensions.Logging.Serilog.Autofac.AssemblyModule>();
            builder.RegisterModule<ArmKubeOper.Components.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(sp => new AzureArmClient(new DefaultAzureCredential(), sp.ResolveOptional<IOptions<ArmClientOptions>>()?.Value));
        }

    }

}
