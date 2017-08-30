﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace NetCore.Identity.Sample.API
{
    public class Routing
    {
        public static IRouteBuilder ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            AddDefaultRoute(routeBuilder, defaultController: "Home", defaultAction: "Index");
            return routeBuilder;
        }

        private static void AddDefaultRoute(IRouteBuilder routeBuilder, string defaultController, string defaultAction)
        {
            routeBuilder.MapRoute(
                name: "default",
                template: "{controller}/{action}/{id?}",
                defaults: new { controller = defaultController, action = defaultAction });
        }
    }
}