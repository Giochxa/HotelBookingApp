using System;

namespace HotelBookingApp.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string HotelImage { get; set; }
        public string Name { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public decimal PricePerNight { get; set; }

        //  Fix: Ensure correct property name
        public bool IsAvailable { get; set; }

        public int MaximumGuests { get; set; }
        public string? ImagePath { get; set; }
    }
}
