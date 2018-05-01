using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API
{
    public static class BackgroundJobsConfig
    {
        public static async void Config()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            
        }
    }
}