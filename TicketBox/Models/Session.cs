using System.ComponentModel.DataAnnotations;

namespace TicketBox.Models
{
    public class Session
    {
        public int Id { get; set; }

        [Display(Name = "Фильм")]
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }

        [Display(Name = "Зал")]
        public int HallId { get; set; }
        public Hall? Hall { get; set; }

        [Display(Name = "Цена")]
        [Range(20000, 200000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Время начала")]
        [Required]
        public DateTime StartTime { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
