using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TicketBox.Models
{
    public class SessionIndexViewModel
    {
        public List<Session> UpcomingSessions { get; set; } = [];
        public List<Session> PastSessions { get; set; } = [];
    }
}
