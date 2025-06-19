using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBox.Data;
using TicketBox.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace TicketBox.Controllers
{
    [Authorize]
    public class SessionsController : Controller
    {
        private readonly CinemaContext _context;

        public SessionsController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Sessions
        public async Task<IActionResult> Index()
        {
            // Получаем актуальные и завершенные сеансы
            var sessions = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return View(sessions);
        }

        // GET: Sessions/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name");
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        // POST: Sessions/Create
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Session session)
        {
            if (ModelState.IsValid)
            {
                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();

                // Создаем билеты для каждого места в зале
                var hall = await _context.Halls.FindAsync(session.HallId);
                if (hall != null)
                {
                    for (int row = 1; row <= hall.Rows; row++)
                    {
                        for (int number = 1; number <= hall.Columns; number++)
                        {
                            var ticket = new Ticket
                            {
                                SessionId = session.Id,
                                Row = row,
                                Number = number,
                                Code = Guid.NewGuid().ToString(),
                                Status = TicketStatus.Available
                            };
                            _context.Tickets.Add(ticket);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            return View(session);
        }

        // GET: Sessions/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            return View(session);
        }

        // POST: Sessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, Session session)
        {
            if (id != session.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            return View(session);
        }

        // GET: Sessions/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                _context.Sessions.Remove(session);

                // Удаляем связанные билеты
                var tickets = _context.Tickets.Where(t => t.SessionId == id);
                _context.Tickets.RemoveRange(tickets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Sessions/Seats/5
        public async Task<IActionResult> Seats(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            ViewData["Session"] = session;

            // Получаем все билеты для сеанса
            var tickets = await _context.Tickets
                .Where(t => t.SessionId == id)
                .ToListAsync();

            ViewBag.Tickets = tickets;

            return View(session);
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }
    }
}