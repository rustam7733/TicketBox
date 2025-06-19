using System.ComponentModel.DataAnnotations;

namespace TicketBox.Models
{
    public class Hall
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        [Required]
        public string Name { get; set; } = null!;

        [Display(Name = "Количество рядов")]
        [Range(1, 50)]
        public int Rows { get; set; }

        [Display(Name = "Количество мест в ряду")]
        [Range(1, 50)]
        public int Columns { get; set; }

        public ICollection<Session> Sessions { get; set; } = [];
    }
}
