using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public enum BusRouteState
    {
        Active,
        Used,
        Pending
    }

    public static class RouteHelpers
    {
        public static BusRouteState Categorize(DateTime departure, DateTime arrival)
        {
            if (DateTime.UtcNow >= departure && DateTime.UtcNow < arrival)
            {
                return BusRouteState.Active;
            }
            else if (arrival >= DateTime.UtcNow)
            {
                return BusRouteState.Used;
            }
            else if (DateTime.UtcNow < departure)
            {
                return BusRouteState.Pending;
            }

            //  Still pending
            return BusRouteState.Pending;
        }
    }
}