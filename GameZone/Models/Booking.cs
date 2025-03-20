using System;
using System.ComponentModel.DataAnnotations;

namespace GameZone.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Changed from nvarchar(450) to string
        public ApplicationUser User { get; set; }
        [Required]
        public int GameId { get; set; }
        public Game Game { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan BookingTime { get; set; }
        [Required]
        [Range(1, 4)]
        public int Duration { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
    }
}