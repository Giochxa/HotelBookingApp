using HotelBookingApp.Data;
using HotelBookingApp.Models;
using HotelBookingApp.Repositories;
using HotelBookingApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBookingApp.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoomRepository _roomRepository;

        public RoomService(ApplicationDbContext context, IRoomRepository roomRepository)
        {
            _context = context;
            _roomRepository = roomRepository;
        }

        public async Task<List<RoomViewModel>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Hotel) //  include Hotel
                .Select(r => new RoomViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    PricePerNight = r.PricePerNight,
                    RoomTypeId = r.RoomTypeId,
                    RoomTypeName = r.RoomType != null ? r.RoomType.Name : "Unknown",
                    HotelId = r.HotelId,
                    HotelName = r.Hotel != null ? r.Hotel.Name : "Unknown",
                    IsAvailable = r.IsAvailable,
                    MaximumGuests = r.MaximumGuests,
                    ImagePath = r.ImagePath
                })
                .ToListAsync();
        }


        public async Task<RoomViewModel> GetRoomByIdAsync(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return null;

            return new RoomViewModel
            {
                Id = room.Id,
                Name = room.Name,
                HotelId = room.HotelId,
                HotelName = room.Hotel?.Name,
                HotelImage = room.Hotel?.FeaturedImage,
                PricePerNight = room.PricePerNight,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType != null ? room.RoomType.Name : "Unknown",
                IsAvailable = room.IsAvailable,
                MaximumGuests = room.MaximumGuests,
                ImagePath = $"{room.ImagePath}"
            };
        }

        public async Task<List<RoomViewModel>> GetRoomsByHotelIdAsync(int hotelId)
        {
            return await _context.Rooms
                .Where(r => r.HotelId == hotelId)
                .Include(r => r.RoomType)
                .Include(r => r.Hotel) //  include Hotel
                .Select(r => new RoomViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    PricePerNight = r.PricePerNight,
                    RoomTypeId = r.RoomTypeId,
                    RoomTypeName = r.RoomType != null ? r.RoomType.Name : "Unknown",
                    HotelId = r.HotelId,
                    HotelName = r.Hotel != null ? r.Hotel.Name : "Unknown",
                    IsAvailable = r.IsAvailable,
                    MaximumGuests = r.MaximumGuests,
                    ImagePath = r.ImagePath
                })
                .ToListAsync();
        }


        public async Task<bool> AddRoomAsync(Room room)
        {
            return await _roomRepository.AddRoomAsync(room);
        }

        public async Task<bool> UpdateRoomAsync(Room room)
        {
            return await _roomRepository.UpdateRoomAsync(room);
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            return await _roomRepository.DeleteRoomAsync(id);
        }
        public async Task<Room> GetRoomEntityByIdAsync(int id)
        {
            return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        }

    }
}
