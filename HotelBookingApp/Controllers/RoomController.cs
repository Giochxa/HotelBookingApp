using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HotelBookingApp.Services;
using HotelBookingApp.DTOs;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using HotelBookingApp.Models;
using System.IO;
using HotelBookingApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace HotelBookingApp.Controllers
{
    [Route("Rooms")]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IHotelService _hotelService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly IBookingService _bookingService;

        public RoomController(IRoomService roomService, IHotelService hotelService, IRoomTypeService roomTypeService, IBookingService bookingService)
        {
            _roomService = roomService;
            _hotelService = hotelService;
            _roomTypeService = roomTypeService;
            _bookingService = bookingService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? hotelId, decimal? minPrice, decimal? maxPrice, int? roomTypeId, int? guests, DateTime? checkIn, DateTime? checkOut)
        {
            var allRooms = hotelId.HasValue
                ? await _roomService.GetRoomsByHotelIdAsync(hotelId.Value)
                : await _roomService.GetAllRoomsAsync();

            if (minPrice.HasValue)
                allRooms = allRooms.Where(r => r.PricePerNight >= minPrice.Value).ToList();
            if (maxPrice.HasValue)
                allRooms = allRooms.Where(r => r.PricePerNight <= maxPrice.Value).ToList();
            if (roomTypeId.HasValue)
                allRooms = allRooms.Where(r => r.RoomTypeId == roomTypeId.Value).ToList();
            if (guests.HasValue)
                allRooms = allRooms.Where(r => r.MaximumGuests >= guests.Value).ToList();

            if (checkIn.HasValue && checkOut.HasValue)
            {
                var bookings = await _bookingService.GetBookingsInRangeAsync(checkIn.Value, checkOut.Value);
                var bookedRoomIds = bookings.Select(b => b.RoomId).Distinct();
                allRooms = allRooms.Where(r => !bookedRoomIds.Contains(r.Id)).ToList();
            }

            var hotels = await _hotelService.GetAllHotelsAsync();
            ViewBag.Hotels = new SelectList(hotels, "Id", "Name", hotelId);
            ViewBag.SelectedCheckIn = checkIn?.ToString("yyyy-MM-dd");
            ViewBag.SelectedCheckOut = checkOut?.ToString("yyyy-MM-dd");
            ViewBag.SelectedMinPrice = minPrice;
            ViewBag.SelectedMaxPrice = maxPrice;
            ViewBag.SelectedRoomTypeId = roomTypeId;
            ViewBag.SelectedGuests = guests;

            return View(allRooms);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound("Room not found.");
            return View(room);
        }

        [Authorize]
        [HttpPost("Book/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null || !room.IsAvailable)
            {
                TempData["ErrorMessage"] = "Room is not available for booking.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = $"Room '{room.Name}' successfully booked.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomDto model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            string imagePath = model.ImagePath;
            if (model.ImageFile != null)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/rooms");
                Directory.CreateDirectory(uploadPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string fullPath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/rooms/" + fileName;
            }

            var room = new Room
            {
                Name = model.Name,
                HotelId = model.HotelId,
                RoomTypeId = model.RoomTypeId,
                PricePerNight = model.PricePerNight,
                MaximumGuests = model.MaximumGuests,
                IsAvailable = model.IsAvailable,
                ImagePath = imagePath
            };

            var result = await _roomService.AddRoomAsync(room);

            if (!result)
            {
                ModelState.AddModelError("", "Failed to create room.");
                await PopulateDropdownsAsync();
                return View(model);
            }

            TempData["Success"] = "Room created successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound();

            await LoadDropdownsAsync(room.HotelId, room.RoomTypeId);

            var dto = new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                HotelId = room.HotelId,
                RoomTypeId = room.RoomTypeId,
                PricePerNight = room.PricePerNight,
                MaximumGuests = room.MaximumGuests,
                IsAvailable = room.IsAvailable,
                ImagePath = room.ImagePath
            };

            return View(dto);
        }

        [HttpPost("Edit/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model.HotelId, model.RoomTypeId);
                return View(model);
            }

            var validHotel = await _hotelService.GetHotelByIdAsync(model.HotelId);
            if (validHotel == null)
            {
                ModelState.AddModelError("HotelId", "Invalid Hotel selected.");
                await LoadDropdownsAsync(model.HotelId, model.RoomTypeId);
                return View(model);
            }

            var room = await _roomService.GetRoomEntityByIdAsync(id);
            if (room == null)
                return NotFound();

            string imagePath = model.ImagePath;
            if (model.ImageFile != null)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/rooms");
                Directory.CreateDirectory(uploadPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string fullPath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/rooms/" + fileName;
            }

            room.Name = model.Name;
            room.PricePerNight = model.PricePerNight;
            room.RoomTypeId = model.RoomTypeId;
            room.IsAvailable = model.IsAvailable;
            room.MaximumGuests = model.MaximumGuests;
            room.HotelId = model.HotelId;
            room.ImagePath = imagePath;

            var success = await _roomService.UpdateRoomAsync(room);
            if (!success)
                return NotFound();

            TempData["Success"] = "Room updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _roomService.DeleteRoomAsync(id);
            if (!success)
                return NotFound();

            TempData["Success"] = "Room deleted successfully.";
            return RedirectToAction("Index");
        }

        private async Task PopulateDropdownsAsync()
        {
            var hotels = await _hotelService.GetAllHotelsAsync() ?? new List<Hotel>();
            var roomTypes = await _roomTypeService.GetAllAsync() ?? new List<RoomType>();

            ViewBag.Hotels = new SelectList(hotels, "Id", "Name");
            ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name");
        }

        private async Task LoadDropdownsAsync(int selectedHotelId, int selectedRoomTypeId)
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            var roomTypes = await _roomTypeService.GetAllAsync();

            ViewBag.Hotels = new SelectList(hotels, "Id", "Name", selectedHotelId);
            ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", selectedRoomTypeId);
        }
    }
}
