using BookingSystem.API.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace BookingSystem.API
{
    public class ThumbnailGenerator
    {
        public static readonly string BaseDirectory;

        public static readonly uint ThumbnailSize = 200;

        static ThumbnailGenerator()
        {
            BaseDirectory = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["MEDIA_THUMBNAIL_DIR"]);
            Directory.CreateDirectory(BaseDirectory);
        }

        public static void Destroy(string name)
        {
            var absolutePath = Path.Combine(BaseDirectory, name);
            if (File.Exists(absolutePath))
            {
                FileHelpers.DeleteFile(new string[] { absolutePath });
            }
        }

        static ThumbnailSharp.Format GetFormat(string mimeType)
        {
            ThumbnailSharp.Format format = ThumbnailSharp.Format.Jpeg;
            if (mimeType.Contains("png"))
                format = ThumbnailSharp.Format.Png;
            else if (mimeType.Contains("tiff"))
                format = ThumbnailSharp.Format.Tiff;
            else if (mimeType.Contains("bmp"))
                format = ThumbnailSharp.Format.Bmp;
            else if (mimeType.Contains("gif"))
                format = ThumbnailSharp.Format.Gif;

            return format;
        }

        public static void Update(string path, string mimeType)
        {
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
            {
                using (var f = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var file = File.Open(Path.Combine(BaseDirectory, Path.GetFileName(path)), FileMode.Truncate | FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                {
                    var stream = new ThumbnailSharp.ThumbnailCreator()
                      .CreateThumbnailStream(ThumbnailSize, f, GetFormat(mimeType));

                    if (stream != null)
                        stream.CopyTo(file);

                    file.Flush();
                }
            });
        }

        public static void Generate(string path, string mimeType)
        {
            HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
            {
                try
                {
                    using (var f = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete ))
                    using (var image = System.Drawing.Image.FromStream(f))
                    using (var file = File.Open(Path.Combine(BaseDirectory, Path.GetFileName(path)), FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    {
                        f.Seek(0, SeekOrigin.Begin);
                        var stream = new ThumbnailSharp.ThumbnailCreator()
                           .CreateThumbnailStream((uint)Math.Min(image.Width, ThumbnailSize), f, GetFormat(mimeType));

                        if (stream != null)
                            stream.CopyTo(file);

                        file.Flush();
                    }

                }
                catch
                {

                }

            }); 
        }



    }
}