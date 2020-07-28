using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence {
    public class DataContext : IdentityDbContext<AppUser> {

        public DataContext (DbContextOptions options) : base (options) { }

        public DbSet<Value> Values { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }

        // Because this is a one to many relationship and in the AppUser 
        // class is stored as a virual property there is no need of extra configuration
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating (ModelBuilder builder) {

            // This allows us, when creating a migration,
            // to give a primary key to the AppUser
            base.OnModelCreating (builder);

            builder.Entity<Value> ()
                .HasData (
                    new Value { Id = 1, Name = "Value 101" },
                    new Value { Id = 2, Name = "Value 102" },
                    new Value { Id = 3, Name = "Value 103" }
                );

            #region 
            // This code describes the relationship between
            // UserActivity with AppUser and Activity

            // This creates the id from the UserId and the ActivityId
            builder.Entity<UserActivity> (x => x.HasKey (ua =>
                new { ua.AppUserId, ua.ActivityId }));

            // It loads the AppUser using AppUserId
            builder.Entity<UserActivity> ()
                .HasOne (u => u.AppUser)
                .WithMany (a => a.UserActivities)
                .HasForeignKey (u => u.AppUserId);

            // It loads the Activity using ActivityId
            builder.Entity<UserActivity> ()
                .HasOne (a => a.Activity)
                .WithMany (u => u.UserActivities)
                .HasForeignKey (a => a.ActivityId);

            #endregion
        }
    }
}