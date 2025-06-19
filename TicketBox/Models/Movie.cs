using System.ComponentModel.DataAnnotations;

namespace TicketBox.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        [Required]
        public string Title { get; set; } = null!;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Ссылка на постер")]
        public string? PosterUrl { get; set; }

        [Display(Name = "Продолжительность в минутах")]
        [Range(1, 500)]
        public int DurationMinutes { get; set; }

        public ICollection<Session> Sessions { get; set; } = [];
    }
}
