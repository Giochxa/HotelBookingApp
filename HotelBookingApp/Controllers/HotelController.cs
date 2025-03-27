using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBookingApp.Services;
using HotelBookingApp.ViewModels;
using HotelBookingApp.Models;
using HotelBookingApp.DTOs;
using System.IO;

namespace HotelBookingApp.Controllers
{
    public class HotelController : Controller
    {
        private readonly IHotelService _hotelService;

        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string name, string city, string country, int? rating)
        {
            var hotels = await _hotelService.GetAllHotelsAsync();

            if (!string.IsNullOrWhiteSpace(name))
                hotels = hotels.Where(h => h.Name.Contains(name, System.StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(city))
                hotels = hotels.Where(h => h.City.Contains(city, System.StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(country))
                hotels = hotels.Where(h => h.Country.Contains(country, System.StringComparison.OrdinalIgnoreCase)).ToList();

            if (rating.HasValue)
                hotels = hotels.Where(h => h.Rating >= rating.Value).ToList(); // updated: include selected and higher

            var hotelViewModels = hotels.Select(h => new HotelViewModel
            {
                Id = h.Id,
                Name = h.Name,
                Address = h.Address,
                City = h.City,
                Country = h.Country,
                Description = h.Description,
                Rating = h.Rating,
                FeaturedImage = h.FeaturedImage
            }).ToList();

            return View(hotelViewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);

            if (hotel == null)
                return NotFound();

            var hotelViewModel = new HotelViewModel
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address,
                City = hotel.City,
                Country = hotel.Country,
                Description = hotel.Description,
                Rating = hotel.Rating,
                FeaturedImage = hotel.FeaturedImage,
                Rooms = hotel.Rooms?.Select(r => new RoomViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    ImagePath = r.ImagePath
                }).ToList() ?? new List<RoomViewModel>()
            };

            return View(hotelViewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new HotelDto());
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HotelDto model)
        {
            ModelState.Remove(nameof(model.FeaturedImage));
            model.FeaturedImage = "/images/no-image.png";

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/hotels");
                Directory.CreateDirectory(uploadPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                model.FeaturedImage = "/images/hotels/" + fileName;
            }

            if (!ModelState.IsValid)
                return View(model);

            var hotel = new Hotel
            {
                Name = model.Name,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                Description = model.Description,
                Rating = model.Rating,
                FeaturedImage = model.FeaturedImage
            };

            var result = await _hotelService.AddHotelAsync(hotel);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to create hotel.");
                return View(model);
            }

            TempData["Success"] = "Hotel created successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            if (hotel == null)
                return NotFound();

            var dto = new HotelDto
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

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(HotelDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string imagePath = model.FeaturedImage;

            if (model.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/hotels");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                imagePath = "/images/hotels/" + uniqueFileName;
            }

            var hotel = new Hotel
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                Description = model.Description,
                Rating = model.Rating,
                FeaturedImage = imagePath
            };

            await _hotelService.UpdateHotelAsync(hotel);

            TempData["Success"] = "Hotel updated successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _hotelService.DeleteHotelAsync(id);
            TempData[success ? "Success" : "ErrorMessage"] = message;
            return RedirectToAction("Index");
        }
    }
}
