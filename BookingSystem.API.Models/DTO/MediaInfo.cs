using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class MediaInfo
    {
        /// <summary>
        /// Note: This is a relative uri. eg /media/stream/img-assdae3re33.png
        /// </summary>
        public string Uri { get; set; }

        public string ThumbnailUri { get; set; }

        public string MimeType { get; set; }

        public string Tag { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}