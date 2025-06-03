using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Data;
using SupportTicketApi.DTOs;
using SupportTicketApi.Models;

namespace SupportTicketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TicketsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET api/tickets
        /// Hämtar alla tickets, ev. filtrerade på status (query param).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetAll([FromQuery] string status)
        {
            IQueryable<Ticket> query = _db.Tickets.Include(t => t.Comments);

            if (!string.IsNullOrEmpty(status))
            {
                // Validera att statusvärdet är giltigt
                var allowed = new[] { "Open", "In Progress", "Closed" };
                if (!allowed.Contains(status))
                    return BadRequest(new { error = "Ogiltigt statusvärde. Måste vara 'Open', 'In Progress' eller 'Closed'." });

                query = query.Where(t => t.Status == status);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Mappa till TicketReadDto
            var result = tickets.Select(t => new TicketReadDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Comments = t.Comments.Select(c => new CommentReadDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// GET api/tickets/{id}
        /// Hämtar ett enskilt ticket med alla kommentarer.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketReadDto>> GetById(int id)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound(new { error = "Ticket not found." });

            var dto = new TicketReadDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Comments = ticket.Comments.Select(c => new CommentReadDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return Ok(dto);
        }

        /// <summary>
        /// POST api/tickets
        /// Skapar ett nytt ticket.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TicketReadDto>> Create([FromBody] CreateTicketDto payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = new Ticket
            {
                Title = payload.Title,
                Description = payload.Description,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            // Hämta kommentar-listan (tom) för DTO
            var dto = new TicketReadDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Comments = new List<CommentReadDto>()
            };

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, dto);
        }

        /// <summary>
        /// PATCH api/tickets/{id}
        /// Uppdaterar status och/eller beskrivning på ett ticket.
        /// </summary>
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketDto payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new { error = "Ticket not found." });

            // Uppdatera fälten om de är satta
            bool changed = false;
            if (!string.IsNullOrEmpty(payload.Description) && payload.Description != ticket.Description)
            {
                ticket.Description = payload.Description;
                changed = true;
            }

            if (!string.IsNullOrEmpty(payload.Status) && payload.Status != ticket.Status)
            {
                ticket.Status = payload.Status;
                changed = true;
            }

            if (!changed)
                return BadRequest(new { error = "Inga giltiga fält att uppdatera eller värdena är oförändrade." });

            ticket.UpdatedAt = DateTime.UtcNow;
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// DELETE api/tickets/{id}
        /// Tar bort ett ticket (och samtliga kommentarer tack vare Cascade).
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new { error = "Ticket not found." });

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
