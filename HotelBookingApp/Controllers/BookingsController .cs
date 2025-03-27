using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HotelBookingApp.DTOs;
using HotelBookingApp.Services;
using HotelBookingApp.Models;
using HotelBookingApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelBookingApp.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(ApplicationDbContext context, IBookingService bookingService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _bookingService = bookingService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int roomId, int hotelId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            var hotel = await _context.Hotels.FindAsync(hotelId);

            if (room == null || hotel == null) return NotFound();

            var dto = new BookingDto
            {
                RoomId = roomId,
                RoomName = room.Name,
                HotelId = hotelId,
                HotelName = hotel.Name,
                PricePerNight = room.PricePerNight
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingDto bookingDto)
        {
            var room = await _context.Rooms.FindAsync(bookingDto.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("", "Room not found.");
                return View(bookingDto);
            }

            bookingDto.PricePerNight = room.PricePerNight;
            bookingDto.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (bookingDto.CheckOutDate <= bookingDto.CheckInDate)
                ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");

            var nights = (bookingDto.CheckOutDate - bookingDto.CheckInDate).Days;
            bookingDto.TotalPrice = nights * bookingDto.PricePerNight;

            if (!ModelState.IsValid)
                return View(bookingDto);

            var booking = new Booking
            {
                UserId = bookingDto.UserId,
                HotelId = bookingDto.HotelId,
                RoomId = bookingDto.RoomId,
                CheckInDate = bookingDto.CheckInDate,
                CheckOutDate = bookingDto.CheckOutDate,
                TotalPrice = bookingDto.TotalPrice,
                IsConfirmed = true
            };

            var result = await _bookingService.CreateBookingAsync(booking);
            if (result)
            {
                TempData["Success"] = "Booking created successfully.";
                return RedirectToAction("Index", "Rooms");
            }

            ModelState.AddModelError("", "Failed to create booking.");
            return View(bookingDto);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string userEmailFilter, int? roomId, int? hotelId, DateTime? checkIn, DateTime? checkOut)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

            var bookingsQuery = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Hotel)
                .AsQueryable();

            if (!isAdminOrManager)
            {
                bookingsQuery = bookingsQuery.Where(b => b.UserId == userId);
            }

            // Filter by user email (admin/manager only)
            if (isAdminOrManager && !string.IsNullOrEmpty(userEmailFilter))
            {
                var user = await _userManager.FindByEmailAsync(userEmailFilter);
                if (user != null)
                {
                    bookingsQuery = bookingsQuery.Where(b => b.UserId == user.Id);
                    ViewBag.SelectedUserId = user.Id;
                }
            }

            // Filter by Room
            if (roomId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.RoomId == roomId.Value);

            // Filter by Hotel
            if (hotelId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.HotelId == hotelId.Value);

            // Filter by Check-In / Check-Out
            if (checkIn.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CheckInDate >= checkIn.Value);

            if (checkOut.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CheckOutDate <= checkOut.Value);

            // Get filtered bookings
            var bookings = await bookingsQuery.OrderByDescending(b => b.CheckInDate).ToListAsync();

            // Filter users list to only those in current bookings
            var userIds = bookings.Select(b => b.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Email })
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.UserEmails = users.ToDictionary(u => u.Value, u => u.Text);

            // For Room/Hotel dropdowns
            ViewBag.Rooms = new SelectList(_context.Rooms, "Id", "Name", roomId);
            ViewBag.Hotels = new SelectList(_context.Hotels, "Id", "Name", hotelId);

            // For check-in/out values
            ViewBag.CheckIn = checkIn?.ToString("yyyy-MM-dd");
            ViewBag.CheckOut = checkOut?.ToString("yyyy-MM-dd");

            return View(bookings);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Hotel)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null || (!isAdminOrManager && booking.UserId != userId))
                return NotFound();

            var dto = new BookingDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomName = booking.Room?.Name ?? "N/A",
                HotelId = booking.HotelId,
                HotelName = booking.Hotel?.Name ?? "N/A",
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                PricePerNight = booking.Room?.PricePerNight ?? 0
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookingDto bookingDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingDto.Id);
            if (booking == null || (!isAdminOrManager && booking.UserId != userId))
                return NotFound();

            var nights = (bookingDto.CheckOutDate - bookingDto.CheckInDate).Days;
            var room = await _context.Rooms.FindAsync(bookingDto.RoomId);

            booking.CheckInDate = bookingDto.CheckInDate;
            booking.CheckOutDate = bookingDto.CheckOutDate;
            booking.TotalPrice = nights * (room?.PricePerNight ?? 0);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null || (!isAdminOrManager && booking.UserId != userId))
                return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AvailableRooms(DateTime checkIn, DateTime checkOut)
        {
            var availableRooms = _bookingService.GetAvailableRooms(checkIn, checkOut);
            return View(availableRooms);
        }
    }
}
