using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Erweiterungsfaktor.Startup))]
namespace Erweiterungsfaktor
{
    //Die StartUp-Klasse ist der Einstiegspunkt in die Anwendung
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
