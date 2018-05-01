using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BookingSystem.API.Handlers
{
    public class ThumbnailImageHandler : StaticMediaHandler
    {
        protected override string GetBasePath()
        {
            return ThumbnailGenerator.BaseDirectory;
        }
    }
}