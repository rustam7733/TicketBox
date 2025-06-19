namespace TicketBox.Models
{
    public class ReceiptViewModel
    {
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
        public string MovieTitle { get; set; } = "";
        public string HallName { get; set; } = "";
        public DateTime SessionTime { get; set; }
        public decimal Price { get; set; }
        public bool IsReservation { get; set; }
    }
}
