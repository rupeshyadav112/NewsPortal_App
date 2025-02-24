using Microsoft.EntityFrameworkCore;

namespace NewsPortal_App.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your DbSets (Tables)
       
    }
}
