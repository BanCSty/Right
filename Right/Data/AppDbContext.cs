using Microsoft.EntityFrameworkCore;
using Right.Model;

namespace Right.Data
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserGroup> Group { get; set; }
        public virtual DbSet<UserState> State { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                //1 - Many
                entity.HasOne(g => g.UserGroup).WithMany(x => x.Users)
                    .OnDelete(DeleteBehavior.Restrict);

                //1 - many
                entity.HasOne(g => g.UserState).WithMany(x => x.Users)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
