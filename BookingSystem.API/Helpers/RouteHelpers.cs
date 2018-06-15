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
            var Now = DateTime.Now;
            if (Now >= departure && Now < arrival)
            {
                return BusRouteState.Active;
            }
            else if (Now >= arrival)
            {
                return BusRouteState.Used;
            }
            else if (Now < departure)
            {
                return BusRouteState.Pending;
            }

            //  Still pending
            return BusRouteState.Pending;
        }
    }
}