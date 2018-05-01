using BookingSystem.API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace BookingSystem.API
{
    public class FiltersConfig
    {
        public static void Config(HttpConfiguration config)
        {
            config.Filters.Add(new ModelStateValidator());
            config.Filters.Add(new UnexpectedExceptionFilter());
        }
    }
}