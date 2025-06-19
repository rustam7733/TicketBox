using System.ComponentModel.DataAnnotations;

namespace TicketBox.Models
{
    public class Seat
    {
        public int Id { get; set; }

        [Required]
        public int Row { get; set; }

        [Required]
        public int Number { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        // Навигация к билету (если место занято)
        public Ticket? Ticket { get; set; }

        // Свойство для проверки, занято ли место
        public bool IsBooked => Ticket != null;

        // Метод для бронирования места
        public void BookSeat(Ticket ticket)
        {
            if (Ticket == null)
            {
                Ticket = ticket;
            }
        }

        // Метод для освобождения места
        public void ReleaseSeat()
        {
            Ticket = null;
        }
    }
}
