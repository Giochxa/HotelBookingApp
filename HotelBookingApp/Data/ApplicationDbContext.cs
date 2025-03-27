using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HotelBookingApp.Models;
using HotelBookingApp.Services;

namespace HotelBookingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        //public DbSet<Role> Roles { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fix for Booking.TotalPrice precision
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // Fix for Room.PricePerNight precision
            modelBuilder.Entity<Room>()
                .Property(r => r.PricePerNight)
                .HasColumnType("decimal(18,2)");

            //  Fix: Change ON DELETE behavior to NO ACTION
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Hotel)
                .WithMany()
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.NoAction); // FIX: Prevent multiple cascade paths

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.NoAction); //  FIX: Prevent multiple cascade paths
        }
           
    }
}