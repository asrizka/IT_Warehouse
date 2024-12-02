using IT_Inventory.Models;
using Microsoft.EntityFrameworkCore;
namespace IT_Inventory.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<ITW_User> ITW_User { get; set; }
        public DbSet<ITW_Category> ITW_Category { get; set; }
        public DbSet<ITW_Device> ITW_Device { get; set; }
        public DbSet<ITW_DeviceDetail> ITW_DeviceDetail { get; set; }
        public DbSet<ITW_Request> ITW_Request { get; set; }
        public DbSet<ITW_Disposal> ITW_Disposal { get; set; }
        public DbSet<ITW_Report> ITW_Report { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ITW_Report>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("ITW_Report"); // Link the model to the view
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
