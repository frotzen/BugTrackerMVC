﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class NotificationType
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Notification Type")]
        public string? Name { get; set; }
    }
}
