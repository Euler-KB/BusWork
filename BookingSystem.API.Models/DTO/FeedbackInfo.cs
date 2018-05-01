using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class FeedbackInfo
    {
        public long Id { get; set; }

        public string Message { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class FeedbackInfoEx : FeedbackInfo
    {
        public UserInfo User { get; set; }
    }

    public class CreateFeedback
    {
        [Required]
        public string Message { get; set; }
    }
}