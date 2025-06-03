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
    [Route("api/tickets/{ticketId:int}/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(AppDbContext db, ILogger<CommentsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// GET api/tickets/{ticketId}/comments
        /// Hämtar alla kommentarer för ett specifikt ticket.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CommentReadDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<CommentReadDto>>> GetAll(int ticketId)
        {
            try
            {
                var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
                if (!ticketExists)
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                var comments = await _db.Comments
                    .Where(c => c.TicketId == ticketId)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                var dtos = comments.Select(c => new CommentReadDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av kommentarer för ticket med id={TicketId}.", ticketId);
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av kommentarer.", details = ex.Message });
            }
        }

        /// <summary>
        /// POST api/tickets/{ticketId}/comments
        /// Skapar en ny kommentar kopplad till ett ticket.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CommentReadDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CommentReadDto>> Create(int ticketId, [FromBody] CommentDto payload)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var ticket = await _db.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                var comment = new Comment
                {
                    TicketId = ticketId,
                    Text = payload.Text,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Comments.Add(comment);
                await _db.SaveChangesAsync();

                var dto = new CommentReadDto
                {
                    Id = comment.Id,
                    TicketId = comment.TicketId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                };

                return CreatedAtAction(nameof(GetById), new { ticketId = ticketId, id = comment.Id }, dto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Databasfel vid skapande av kommentar för ticket med id={TicketId}.", ticketId);
                return StatusCode(500, new { error = "Fel vid sparande i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett oväntat fel inträffade vid skapande av kommentar för ticket med id={TicketId}.", ticketId);
                return StatusCode(500, new { error = "Internt serverfel vid skapande av kommentar.", details = ex.Message });
            }
        }

        /// <summary>
        /// GET api/tickets/{ticketId}/comments/{id}
        /// Hämtar en enskild kommentar.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CommentReadDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CommentReadDto>> GetById(int ticketId, int id)
        {
            try
            {
                var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
                if (!ticketExists)
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                var comment = await _db.Comments
                    .FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
                if (comment == null)
                    return NotFound(new { error = $"Kommentar med id={id} kunde inte hittas för ticket id={ticketId}." });

                var dto = new CommentReadDto
                {
                    Id = comment.Id,
                    TicketId = comment.TicketId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av kommentar med id={CommentId} för ticket id={TicketId}.", id, ticketId);
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av kommentar.", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE api/tickets/{ticketId}/comments/{id}
        /// Tar bort en specifik kommentar.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int ticketId, int id)
        {
            try
            {
                var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
                if (!ticketExists)
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                var comment = await _db.Comments
                    .FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
                if (comment == null)
                    return NotFound(new { error = $"Kommentar med id={id} kunde inte hittas för ticket id={ticketId}." });

                _db.Comments.Remove(comment);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Databasfel vid borttagning av kommentar med id={CommentId} för ticket id={TicketId}.", id, ticketId);
                return StatusCode(500, new { error = "Fel vid borttagning i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett oväntat fel uppstod vid borttagning av kommentar med id={CommentId} för ticket id={TicketId}.", id, ticketId);
                return StatusCode(500, new { error = "Internt serverfel vid borttagning av kommentar.", details = ex.Message });
            }
        }
    }
}
