using HotelReservationApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationApp.Data
{
    public class HotelReservationContext : DbContext
    {
        public HotelReservationContext(DbContextOptions<HotelReservationContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<HotelImage> HotelImages { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<RoomAvailability> RoomAvailabilities { get; set; }
        public DbSet<HotelAmenity> HotelAmenities { get; set; }
        public DbSet<HotelAmenityMapping> HotelAmenityMapping { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Reviews tablosunda cascade silmeyi kapat
            modelBuilder
                .Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Review>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Reviews)
                .HasForeignKey(r => r.HotelID)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservations tablosunda cascade silmeyi kapat
            modelBuilder
                .Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HotelAmenity>().HasKey(a => a.AmenityID);
            modelBuilder.Entity<HotelAmenityMapping>().HasKey(h => new { h.HotelID, h.AmenityID });

            modelBuilder
                .Entity<HotelAmenityMapping>()
                .HasOne(h => h.Hotel)
                .WithMany(h => h.HotelAmenityMappings)
                .HasForeignKey(h => h.HotelID);

            modelBuilder
                .Entity<HotelAmenityMapping>()
                .HasOne(h => h.Amenity)
                .WithMany(a => a.HotelAmenityMappings)
                .HasForeignKey(h => h.AmenityID);

            // RoomAvailability için unique constraint (RoomID + Date)
            modelBuilder
                .Entity<RoomAvailability>()
                .HasIndex(ra => new { ra.RoomID, ra.Date })
                .IsUnique();

            // RoomAvailability için primary key
            modelBuilder.Entity<RoomAvailability>().HasKey(ra => ra.AvailabilityID);

            // Decimal alanlar için precision tanımlamaları
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(10, 2);

            modelBuilder.Entity<Reservation>().Property(r => r.TotalAmount).HasPrecision(10, 2);

            modelBuilder.Entity<Room>().Property(r => r.PricePerNight).HasPrecision(10, 2);

            modelBuilder.Entity<RoomAvailability>().Property(ra => ra.Price).HasPrecision(10, 2);
        }
    }
}
