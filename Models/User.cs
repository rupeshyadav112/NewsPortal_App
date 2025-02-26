﻿using System.ComponentModel.DataAnnotations;

namespace NewsPortal_App.Models
{
    public class User
    {
        [Key] // Primary key specify karein
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfileImagePath { get; set; } = "~/images/avatar.png";
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? GoogleUserId { get; set; }
        public bool IsGoogleAccount { get; set; } = false;
        // Navigation Properties

        //public ICollection<Comment> Comments { get; set; } // A user can make many comments
        //public ICollection<CommentLike> Likes { get; set; } // A user can like many comments

    }
}