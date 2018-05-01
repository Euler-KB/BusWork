using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public class DiskMediaStore
    {
        public static readonly DiskMediaStore Instance;

        public static bool EnableThumbnailGeneration;

        static DiskMediaStore()
        {
            EnableThumbnailGeneration = bool.Parse(ConfigurationManager.AppSettings["ENABLE_THUMBNAIL_GENERATION"]);

            //
            string rootPath = System.Web.Hosting.HostingEnvironment.MapPath(ConfigurationManager.AppSettings["MEDIA_ROOT_DIR"]);
            Directory.CreateDirectory(rootPath);

            //
            Instance = new DiskMediaStore(rootPath);
        }

        public class SaveMediaResponse
        {
            public string Name { get; set; }

            public string Path { get; set; }

            public bool GenerateThumbnail { get; set; }

            public long Size { get; set; }
        }


        private string basePath;

        public string BasePath
        {
            get
            {
                return basePath;
            }
        }

        public string ResolvePath(string path)
        {
            return Path.Combine(basePath, path);
        }

        public DiskMediaStore(string rootPath)
        {
            this.basePath = rootPath;
        }

        public async Task<SaveMediaResponse> SaveMedia(string name, string mimeType, Stream stream)
        {
            var path = ResolvePath(name);
            using (var fs = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                await stream.CopyToAsync(fs);
            }

            //  Generate thumbnail
            if (EnableThumbnailGeneration)
            {
                //  Generate thumbnail
                ThumbnailGenerator.Generate(path, mimeType);
            }

            return new SaveMediaResponse()
            {
                Name = name,
                Path = path,
                Size = stream.Length,
                GenerateThumbnail = EnableThumbnailGeneration
            };
        }

        public async Task<SaveMediaResponse> UpdateMedia(string originalName, string newName, string mimeType, Stream stream)
        {
            string path = ResolvePath(originalName);
            if (File.Exists(path))
            {
                //  Remove this file
                FileHelpers.DeleteFile(new string[] { path });

                //
                if (EnableThumbnailGeneration)
                {
                    ThumbnailGenerator.Destroy(originalName);
                }

                return await SaveMedia(newName, mimeType, stream);
            }
            else
            {
                return await SaveMedia(originalName, mimeType, stream);
            }
        }

        public Task DeleteMedia(string name)
        {
            string path = ResolvePath(name);

            FileHelpers.DeleteFile(new string[] { path });

            if (EnableThumbnailGeneration)
            {
                //  Delete thumbnail
                ThumbnailGenerator.Destroy(name);
            }

            return Task.FromResult(0);
        }
    }
}