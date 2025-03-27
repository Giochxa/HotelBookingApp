using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotelBookingApp.Models;

namespace HotelBookingApp.ViewModels
{
    public class HotelViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Description { get; set; }

        public double Rating { get; set; }

        public string FeaturedImage { get; set; }

        public List<RoomViewModel> Rooms { get; set; }
        public string? ImagePath { get; set; }
    }
}
