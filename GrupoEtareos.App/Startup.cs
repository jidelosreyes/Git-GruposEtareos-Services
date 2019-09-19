using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GrupoEtareos.App.Startup))]
namespace GrupoEtareos.App
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
