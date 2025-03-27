using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotelBookingApp.Models;
using HotelBookingApp.Services;
using HotelBookingApp.DTOs;

namespace HotelBookingApp.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AdminController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly IRoomService _roomService;

        public AdminController(IHotelService hotelService, IRoomService roomService)
        {
            _hotelService = hotelService;
            _roomService = roomService;
        }

        // ===========================
        //        HOTELS MANAGEMENT
        // ===========================

        [HttpGet]
        public async Task<IActionResult> ManageHotels()
        {
            var hotels = await _hotelService.GetAllHotelsAsync();
            return View(hotels);
        }

        [HttpGet]
        public IActionResult CreateHotel()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromForm] HotelDto hotelDto)
        {
            if (!ModelState.IsValid)
                return View(hotelDto);

            var hotel = new Hotel
            {
                Name = hotelDto.Name,
                Address = hotelDto.Address,
                City = hotelDto.City,
                Country = hotelDto.Country,
                Description = hotelDto.Description,
                Rating = hotelDto.Rating,
                FeaturedImage = hotelDto.FeaturedImage
            };

            var result = await _hotelService.AddHotelAsync(hotel);
            if (result)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to add hotel.");
            return View(hotelDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditHotel(int id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            if (hotel == null)
                return NotFound();

            var hotelDto = new HotelDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address,
                City = hotel.City,
                Country = hotel.Country,
                Description = hotel.Description,
                Rating = hotel.Rating,
                FeaturedImage = hotel.FeaturedImage
            };

            return View(hotelDto);
        }

        [HttpPost]
        public async Task<IActionResult> EditHotel([FromForm] HotelDto hotelDto)
        {
            if (!ModelState.IsValid)
                return View(hotelDto);

            var hotel = new Hotel
            {
                Id = hotelDto.Id,
                Name = hotelDto.Name,
                Address = hotelDto.Address,
                City = hotelDto.City,
                Country = hotelDto.Country,
                Description = hotelDto.Description,
                Rating = hotelDto.Rating,
                FeaturedImage = hotelDto.FeaturedImage
            };

            var result = await _hotelService.UpdateHotelAsync(hotel);
            if (result)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to update hotel.");
            return View(hotelDto);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var (success, message) = await _hotelService.DeleteHotelAsync(id);

            if (!success)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction("Index"); 
        }



        // ===========================
        //        ROOMS MANAGEMENT
        // ===========================

        [HttpGet]
        public async Task<IActionResult> ManageRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return View(rooms);
        }

        [HttpGet]
        public IActionResult CreateRoom()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromForm] RoomDto roomDto)
        {
            if (!ModelState.IsValid)
                return View(roomDto);

            var room = new Room
            {
                Name = roomDto.Name,
                HotelId = roomDto.HotelId,
                RoomTypeId = roomDto.RoomTypeId,
                PricePerNight = roomDto.PricePerNight,
                IsAvailable = roomDto.IsAvailable
            };

            var result = await _roomService.AddRoomAsync(room);
            if (result)
                return RedirectToAction("ManageRooms");

            ModelState.AddModelError("", "Failed to add room.");
            return View(roomDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound();

            var roomDto = new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                HotelId = room.HotelId,
                RoomTypeId = room.RoomTypeId,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable
            };

            return View(roomDto);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoom([FromForm] RoomDto roomDto)
        {
            if (!ModelState.IsValid)
                return View(roomDto);

            var room = new Room
            {
                Id = roomDto.Id,
                Name = roomDto.Name,
                HotelId = roomDto.HotelId,
                RoomTypeId = roomDto.RoomTypeId,
                PricePerNight = roomDto.PricePerNight,
                IsAvailable = roomDto.IsAvailable
            };

            var result = await _roomService.UpdateRoomAsync(room);
            if (result)
                return RedirectToAction("ManageRooms");

            ModelState.AddModelError("", "Failed to update room.");
            return View(roomDto);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (result)
                return RedirectToAction("ManageRooms");

            ModelState.AddModelError("", "Failed to delete room.");
            return RedirectToAction("ManageRooms");
        }
    }
}
