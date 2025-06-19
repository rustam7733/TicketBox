using System.ComponentModel.DataAnnotations;

namespace TicketBox.Models
{
    public class CheckTicketViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите код билета.")]
        public string? Code { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
