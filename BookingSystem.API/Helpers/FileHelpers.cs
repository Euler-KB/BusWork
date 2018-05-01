using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace BookingSystem.API.Helpers
{
    public class FileHelpers
    {
        public static void DeleteFile(string[] paths, int maxRetryTimes = 10)
        {
            if (paths.Length > 0)
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                {
                    foreach (var path in paths)
                    {
                        while (maxRetryTimes-- > 0)
                        {
                            try
                            {
                                if (File.Exists(path))
                                    File.Delete(path);

                                break;
                            }
                            catch
                            {

                            }
                        }
                    }

                });
        }
    }
}