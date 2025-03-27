using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBookingApp.Models;
using HotelBookingApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApp.Services
{
    public interface IRoomService
    {
        Task<List<RoomViewModel>> GetAllRoomsAsync();
        Task<List<RoomViewModel>> GetRoomsByHotelIdAsync(int hotelId);
        Task<RoomViewModel> GetRoomByIdAsync(int id);
        Task<bool> AddRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(Room room);
        Task<bool> DeleteRoomAsync(int id);
        Task<Room> GetRoomEntityByIdAsync(int id);

    }
}
