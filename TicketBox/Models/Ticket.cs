using System.ComponentModel.DataAnnotations;

namespace TicketBox.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        [Required]
        public int Row { get; set; }

        [Required]
        public int Number { get; set; } // Номер места

        [Required]
        public string Code { get; set; } = null!; // Уникальный код билета

        [Required]
        public TicketStatus Status { get; set; }
    }

    public enum TicketStatus
    {
        Available = 0,
        Reserved = 1,
        Sold = 2,
        Used = 3
    }
}
