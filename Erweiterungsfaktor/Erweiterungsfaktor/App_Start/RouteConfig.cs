using System.Web.Mvc;
using System.Web.Routing;

namespace Erweiterungsfaktor
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //Route für Admin-Hauptmenü registrieren
            //routes.MapRoute(
            //    name: "Admin",
            //    url: "admin/{action}/{id}",
            //    defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional }
            //);
            //Standard-Route registrieren
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
