using Microsoft.EntityFrameworkCore;
using NewsPortal_App.Models;

namespace NewsPortal_App.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // डेटाबेस टेबल्स के लिए DbSet
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }

    }
}
