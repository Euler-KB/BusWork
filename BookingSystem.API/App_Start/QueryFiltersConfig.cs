using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Z.EntityFramework.Plus;

namespace BookingSystem.API
{
    public class QueryFiltersConfig
    {
        public static void ConfigFilters()
        {
            QueryFilterManager.AllowPropertyFilter = false;
            QueryFilterManager.Filter<AppUser>(FilterKeys.ActiveUsers, x => x.Where(t => !t.IsSoftDeleted));
            QueryFilterManager.Filter<UserWallet>(FilterKeys.ActiveWallet, x => x.Where(t => !t.IsSoftDeleted));
            QueryFilterManager.Filter<BusRoute>(FilterKeys.ActiveRoutes, x => x.Where(t => !t.IsSoftDeleted));
            QueryFilterManager.Filter<Bus>(FilterKeys.ActiveBuses, x => x.Where(t => !t.IsSoftDeleted));
            QueryFilterManager.Filter<AppUser>(FilterKeys.RegisteredUsers, x => x.Where(t => t.EmailConfirmed && !t.LockedOut), false);
        }
    }
}