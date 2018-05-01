using BookingSystem.API.Handlers;
using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Z.EntityFramework.Plus;

namespace BookingSystem.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            //
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookingContext, Migrations.Configuration>());


            // Web API configuration and services
            FiltersConfig.Config(config);
            UnityConfig.RegisterComponents();

            //
            AutoMapperConfig.Config(config);

            //  Setup filters
            QueryFiltersConfig.ConfigFilters();


            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Web API routes
            config.MapHttpAttributeRoutes();


            //  Default Api
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                constraints: null,
                defaults: new { id = RouteParameter.Optional }
            );

            //
            config.MessageHandlers.Add(new ApiKeyHandler());
            config.MessageHandlers.Add(new AuthenticationHandler());

            //  include 
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            //
            BackgroundJobsConfig.Config();
        }
    }
}
