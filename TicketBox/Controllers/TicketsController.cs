using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBox.Data;
using TicketBox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TicketBox.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly CinemaContext _context;

        public TicketsController(CinemaContext context)
        {
            _context = context;
        }

        // POST: Tickets/Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int sessionId, string selectedSeats)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                if (string.IsNullOrWhiteSpace(selectedSeats))
                {
                    ViewBag.ErrorMessage = "Сеанс завершён или не найден";
                    ViewBag.SessionId = sessionId;
                    return View("ErrorMessage");
                }
            }

            if (session.StartTime < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Сеанс завершён или не найден";
                ViewBag.SessionId = sessionId;
                return View("ErrorMessage");
            }

            if (string.IsNullOrWhiteSpace(selectedSeats))
            {
                ViewBag.ErrorMessage = "Места не выбраны.";
                ViewBag.SessionId = sessionId;
                return View("ErrorMessage");
            }

            var seatList = selectedSeats.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var purchasedTickets = new List<Ticket>();

            foreach (var seatString in seatList)
            {
                var parts = seatString.Split(',');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int row) || !int.TryParse(parts[1], out int number))
                    return BadRequest($"Invalid seat format: {seatString}");

                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.SessionId == sessionId && t.Row == row && t.Number == number);

                if (ticket == null)
                    return NotFound($"Ticket for seat {row}-{number} not found");

                if (ticket.Status != TicketStatus.Available)
                    return BadRequest($"Ticket for seat {row}-{number} is not available");

                ticket.Status = TicketStatus.Sold;
                purchasedTickets.Add(ticket);
            }

            _context.Tickets.UpdateRange(purchasedTickets);
            await _context.SaveChangesAsync();

            return RedirectToAction("Checkout", new { ticketIds = purchasedTickets.Select(t => t.Id).ToList() });
        }

        // POST: Tickets/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int sessionId, string selectedSeats)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                if (string.IsNullOrWhiteSpace(selectedSeats))
                {
                    ViewBag.ErrorMessage = "Сеанс завершён или не найден";
                    ViewBag.SessionId = sessionId;
                    return View("ErrorMessage");
                }
            }

            if (session.StartTime < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Сеанс завершён или не найден";
                ViewBag.SessionId = sessionId;
                return View("ErrorMessage");
            }

            if (string.IsNullOrWhiteSpace(selectedSeats))
            {
                ViewBag.ErrorMessage = "Места не выбраны.";
                ViewBag.SessionId = sessionId;
                return View("ErrorMessage");
            }

            var seatList = selectedSeats.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var reservedTickets = new List<Ticket>();

            foreach (var seatString in seatList)
            {
                var parts = seatString.Split(',');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int row) || !int.TryParse(parts[1], out int number))
                {
                    return BadRequest($"Invalid seat format: {seatString}");
                }

                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.SessionId == sessionId && t.Row == row && t.Number == number);

                if (ticket == null)
                {
                    return NotFound($"Ticket for seat {row}-{number} not found");
                }

                if (ticket.Status != TicketStatus.Available)
                {
                    return BadRequest($"Ticket for seat {row}-{number} is not available");
                }

                ticket.Status = TicketStatus.Reserved;
                reservedTickets.Add(ticket);
            }

            _context.Tickets.UpdateRange(reservedTickets);
            await _context.SaveChangesAsync();

            return RedirectToAction("Checkout", new { ticketIds = reservedTickets.Select(t => t.Id).ToList() });
        }

        // GET: Tickets/Activate/5
        public async Task<IActionResult> Activate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status != TicketStatus.Sold)
            {
                return BadRequest("Ticket is not sold and cannot be activated");
            }

            ticket.Status = TicketStatus.Used;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Sessions", new { id = ticket.SessionId }); // Перенаправляем на детали сеанса
        }

        // GET: Tickets/Scan
        [HttpGet]
        public IActionResult Scan()
        {
            return View();
        }

        // POST: Tickets/Scan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Scan(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                ViewBag.Message = "Пожалуйста, введите код билета.";
                return View();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Session)
                .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(t => t.Code == code);

            if (ticket == null)
            {
                ViewBag.Message = "Билет не найден.";
                ViewBag.Success = false;
            }
            else if (ticket.Status == TicketStatus.Sold)
            {
                ticket.Status = TicketStatus.Used;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                ViewBag.Message = $"Билет на фильм \"{ticket.Session.Movie.Title}\" активирован.";
                ViewBag.Success = true;
            }
            else if (ticket.Status == TicketStatus.Used)
            {
                ViewBag.Message = "Билет уже использован.";
                ViewBag.Success = false;
            }
            else if (ticket.Status == TicketStatus.Reserved)
            {
                ViewBag.Message = "Билет только забронирован и не оплачен.";
                ViewBag.Success = false;
            }
            else
            {
                ViewBag.Message = "Билет не может быть активирован.";
                ViewBag.Success = false;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelReservation(int ticketId)
        {
            var ticket = _context.Tickets.Find(ticketId);
            if (ticket == null) return NotFound();

            if (ticket.Status == TicketStatus.Reserved)
            {
                ticket.Status = TicketStatus.Available;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelPurchase(int ticketId)
        {
            var ticket = _context.Tickets.Find(ticketId);
            if (ticket == null) return NotFound();

            if (ticket.Status == TicketStatus.Sold)
            {
                ticket.Status = TicketStatus.Available;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // GET: Tickets
        public async Task<IActionResult> Index(string searchCode)
        {
            var ticketsQuery = _context.Tickets
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Session.Hall)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchCode))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Code.Contains(searchCode));
            }

            var tickets = await ticketsQuery
                .OrderBy(t => t.Session.StartTime)
                .ThenBy(t => t.Row)
                .ThenBy(t => t.Number)
                .ToListAsync();

            ViewBag.SearchCode = searchCode;

            return View(tickets);
        }

        // TicketController.cs (или аналогичный контроллер)
        public async Task<IActionResult> Checkout(List<int> ticketIds)
        {
            if (ticketIds == null || !ticketIds.Any())
            {
                return BadRequest("Нет выбранных билетов.");
            }

            var tickets = await _context.Tickets
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Session.Hall)
                .Where(t => ticketIds.Contains(t.Id))
                .ToListAsync();

            if (!tickets.Any())
            {
                return NotFound("Билеты не найдены.");
            }

            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(List<int> ticketIds)
        {
            if (ticketIds == null || !ticketIds.Any())
            {
                return BadRequest("Нет выбранных билетов.");
            }

            var tickets = await _context.Tickets
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Session.Hall)
                .Where(t => ticketIds.Contains(t.Id))
                .ToListAsync();

            foreach (var ticket in tickets)
            {
                if (ticket.Status == TicketStatus.Available)
                {
                    ticket.Status = TicketStatus.Sold;
                }
            }

            _context.UpdateRange(tickets);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Билеты успешно оформлены.";
            return View("Checkout", tickets); // Возврат обратно на ту же страницу
        }

    }
}