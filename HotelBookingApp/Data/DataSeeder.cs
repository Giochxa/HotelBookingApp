using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBookingApp.Models;

namespace HotelBookingApp.Data
{
    public class DataSeeder
    {
        public static async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created and up to date
            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roleNames = { "Admin", "Manager", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = roleName });
                }
            }

            // Seed Admin User
            var adminEmail = "admin@example.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Manager User
            var managerEmail = "manager@example.com";
            if (await userManager.FindByEmailAsync(managerEmail) == null)
            {
                var managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "System Manager",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(managerUser, "Manager@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }

            // Seed Regular User
            var userEmail = "user@example.com";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var regularUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "System User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(regularUser, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }

            // Seed Sample Hotels
            if (!context.Hotels.Any())
            {
                context.Hotels.AddRange(
                    new Hotel
                    {
                        Name = "Luxury Hotel",
                        Address = "123 Main St",
                        City = "New York",
                        Country = "USA",
                        Description = "A luxury hotel",
                        Rating = 4.5,
                        FeaturedImage = "/images/hotels/luxury.jpg"
                    },
                    new Hotel
                    {
                        Name = "Budget Inn",
                        Address = "456 Elm St",
                        City = "Los Angeles",
                        Country = "USA",
                        Description = "An affordable hotel",
                        Rating = 3.8,
                        FeaturedImage = "/images/hotels/budget.jpg"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Seed Room Types
            var roomTypeNames = new[] { "Standard", "Single Room", "Double Room", "Deluxe Room" };
            foreach (var name in roomTypeNames)
            {
                if (!await context.RoomTypes.AnyAsync(rt => rt.Name == name))
                {
                    context.RoomTypes.Add(new RoomType { Name = name });
                }
            }
            await context.SaveChangesAsync();

            // Seed Sample Rooms
            if (!context.Rooms.Any())
            {
                var luxuryHotel = await context.Hotels.FirstOrDefaultAsync(h => h.Name == "Luxury Hotel");
                var budgetHotel = await context.Hotels.FirstOrDefaultAsync(h => h.Name == "Budget Inn");

                var standardType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.Name == "Standard");

                if (luxuryHotel != null && budgetHotel != null && standardType != null)
                {
                    context.Rooms.AddRange(
                        new Room
                        {
                            Name = "King Suite",
                            HotelId = luxuryHotel.Id,
                            RoomTypeId = standardType.Id,
                            PricePerNight = 250.00m,
                            IsAvailable = true,
                            MaximumGuests = 3,
                            ImagePath = "/images/rooms/1.jpg"
                        },
                        new Room
                        {
                            Name = "Deluxe Double",
                            HotelId = luxuryHotel.Id,
                            RoomTypeId = standardType.Id,
                            PricePerNight = 180.00m,
                            IsAvailable = true,
                            MaximumGuests = 2,
                            ImagePath = "/images/rooms/2.jpg"
                        },
                        new Room
                        {
                            Name = "Single Room",
                            HotelId = budgetHotel.Id,
                            RoomTypeId = standardType.Id,
                            PricePerNight = 80.00m,
                            IsAvailable = true,
                            MaximumGuests = 1,
                            ImagePath = "/images/rooms/3.jpg"
                        },
                        new Room
                        {
                            Name = "Economy Room",
                            HotelId = budgetHotel.Id,
                            RoomTypeId = standardType.Id,
                            PricePerNight = 60.00m,
                            IsAvailable = false,
                            MaximumGuests = 2,
                            ImagePath = "/images/rooms/4.jpg"
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
