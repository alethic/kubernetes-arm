using Autofac;

using Cogito.Autofac;

namespace ArmKubeOper.Components
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<Cogito.Autofac.AssemblyModule>();
            builder.RegisterModule<Cogito.Extensions.Configuration.Autofac.AssemblyModule>();
            builder.RegisterModule<Cogito.Extensions.Options.Autofac.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
