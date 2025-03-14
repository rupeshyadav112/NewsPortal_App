﻿using System.ComponentModel.DataAnnotations;
namespace NewsPortal_App.Models
{
        public class Post
        {
            public int PostID { get; set; }

            [Required]
            [MaxLength(255)]
            public string Title { get; set; }

            [Required]
            public string Content { get; set; }

            [Required]
            [MaxLength(50)]
            public string Category { get; set; }

            [MaxLength(255)]
            public string ImagePath { get; set; }

            [MaxLength(50)]
            public string FontStyle { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public ICollection<Comment> Comments { get; set; } // A post can have many comments
    }
    }


