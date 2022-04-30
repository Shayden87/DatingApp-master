using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    // Summary:
    // Acts as a bridge between program code and the database.
    // 
    // Derived from DbContext class and allows injection of DataContext 
    // into other parts of the application.
    public class DataContext : DbContext
    {
        // Summary: 
        // Constructor that is called when DataContext class is initiated.
        // Passed options parameter when added to Startup.cs configuration.
        //
        // Parameters:
        //   options:
        //     The options for this context.
        public DataContext(DbContextOptions options) : base(options)
        {
            
        }
        // Summary:
        // Creates database set (table) named Users.
        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }

        // Creates framework for the "like" feature by creating relationships 
        // between tables in our database.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserLike>()
                .HasKey(k => new {k.SourceUserId, k.LikedUserId});

            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(s => s.LikedUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }  
    }
}