using System.ComponentModel.DataAnnotations;

namespace HotelBookingApp.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int HotelId { get; set; }

        [Required]
        public int RoomTypeId { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaximumGuests { get; set; }

        [Required]
        [Range(1, 10000)]
        public decimal PricePerNight { get; set; }

        public bool IsAvailable { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? ImagePath { get; set; } // optional: to store saved path

    }
}
