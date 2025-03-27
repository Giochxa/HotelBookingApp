using System;

namespace HotelBookingApp.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }

        //  FIX: Add missing IsConfirmed property
        public bool IsConfirmed { get; set; }
    }
}
