using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(AppDbContext db, ILogger<TicketsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// GET api/tickets
        /// Hämtar alla tickets, ev. filtrerade på status (query param).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TicketReadDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetAll([FromQuery] string status)
        {
            try
            {
                IQueryable<Ticket> query = _db.Tickets.Include(t => t.Comments);

                if (!string.IsNullOrEmpty(status))
                {
                    var allowed = new[] { "Open", "In Progress", "Closed" };
                    if (!allowed.Contains(status))
                    {
                        return BadRequest(new { error = "Ogiltigt statusvärde. Måste vara 'Open', 'In Progress' eller 'Closed'." });
                    }
                    query = query.Where(t => t.Status == status);
                }

                var tickets = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av alla tickets.");
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av ärenden.", details = ex.Message });
            }
        }

        /// <summary>
        /// GET api/tickets/{id}
        /// Hämtar ett enskilt ticket med alla kommentarer.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TicketReadDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TicketReadDto>> GetById(int id)
        {
            try
            {
                var ticket = await _db.Tickets
                    .Include(t => t.Comments)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                    return NotFound(new { error = $"Ticket med id={id} kunde inte hittas." });

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av ticket med id={TicketId}.", id);
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av ärende.", details = ex.Message });
            }
        }

        /// <summary>
        /// POST api/tickets
        /// Skapar ett nytt ticket.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TicketReadDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TicketReadDto>> Create([FromBody] CreateTicketDto payload)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var ticket = new Ticket
            {
                Title = payload.Title,
                Description = payload.Description,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                _db.Tickets.Add(ticket);
                await _db.SaveChangesAsync();

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
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Databasfel vid skapande av ticket.");
                return StatusCode(500, new { error = "Fel vid sparande i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett oväntat fel uppstod vid skapande av ticket.");
                return StatusCode(500, new { error = "Internt serverfel vid skapande av ärende.", details = ex.Message });
            }
        }

        /// <summary>
        /// PATCH api/tickets/{id}
        /// Uppdaterar status och/eller beskrivning på ett ticket.
        /// </summary>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketDto payload)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var ticket = await _db.Tickets.FindAsync(id);
                if (ticket == null)
                    return NotFound(new { error = $"Ticket med id={id} kunde inte hittas." });

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
                    return BadRequest(new { error = "Inga nya eller giltiga fält att uppdatera." });

                ticket.UpdatedAt = DateTime.UtcNow;

                _db.Tickets.Update(ticket);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Databasfel vid uppdatering av ticket med id={TicketId}.", id);
                return StatusCode(500, new { error = "Fel vid uppdatering i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett oväntat fel inträffade vid uppdatering av ticket med id={TicketId}.", id);
                return StatusCode(500, new { error = "Internt serverfel vid uppdatering av ärende.", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE api/tickets/{id}
        /// Tar bort ett ticket (och samtliga kommentarer tack vare Cascade).
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ticket = await _db.Tickets.FindAsync(id);
                if (ticket == null)
                    return NotFound(new { error = $"Ticket med id={id} kunde inte hittas." });

                _db.Tickets.Remove(ticket);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Databasfel vid borttagning av ticket med id={TicketId}.", id);
                return StatusCode(500, new { error = "Fel vid borttagning i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett oväntat fel uppstod vid borttagning av ticket med id={TicketId}.", id);
                return StatusCode(500, new { error = "Internt serverfel vid borttagning av ärende.", details = ex.Message });
            }
        }
    }
}
