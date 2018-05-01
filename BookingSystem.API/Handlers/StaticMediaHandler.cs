using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BookingSystem.API.Handlers
{
    public class StaticMediaHandler : HttpTaskAsyncHandler
    {
        private string basePath;

        public override bool IsReusable => true;

        protected virtual string GetBasePath()
        {
            return HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["MEDIA_ROOT_DIR"]);
        }

        public StaticMediaHandler()
        {
            basePath = GetBasePath();

            //  Ensure directory exists
            Directory.CreateDirectory(basePath);
        }

        protected string ResolvePath(string path)
        {
            return Path.Combine(basePath, path);
        }

        protected virtual string GetMediaName(Uri uri)
        {
            return uri.Segments.Last();
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string name = GetMediaName(request.Url);
            using (var dbContext = new BookingContext())
            {
                var f = await dbContext.Media.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name);
                if (f != null && File.Exists(f.Path))
                {
                    using (var file = File.Open(f.Path, FileMode.Open, FileAccess.Read))
                    {
                        response.AddHeader("Content-Length", file.Length.ToString());
                        response.ContentType = f.MimeType;
                    }

                    await Task.Run(() => response.WriteFile(f.Path));

                    return;

                }

                response.StatusCode = 404;
                response.StatusDescription = "No such media exists";
            }
        }
    }
}