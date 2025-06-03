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
    [Route("api/tickets/{ticketId:int}/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CommentsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET api/tickets/{ticketId}/comments
        /// Hämtar alla kommentarer för ett specifikt ticket.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentReadDto>>> GetAll(int ticketId)
        {
            // Kontrollera att ticket finns
            var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
            if (!ticketExists)
                return NotFound(new { error = "Ticket not found." });

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

        /// <summary>
        /// POST api/tickets/{ticketId}/comments
        /// Skapar en ny kommentar kopplad till ett ticket.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CommentReadDto>> Create(int ticketId, [FromBody] CommentDto payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kontrollera att ticket finns
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound(new { error = "Ticket not found." });

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

        /// <summary>
        /// GET api/tickets/{ticketId}/comments/{id}
        /// Hämtar en enskild kommentar (om du vill stödja detta).
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CommentReadDto>> GetById(int ticketId, int id)
        {
            // Kontrollera ticket
            var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
            if (!ticketExists)
                return NotFound(new { error = "Ticket not found." });

            var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (comment == null)
                return NotFound(new { error = "Comment not found." });

            var dto = new CommentReadDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// DELETE api/tickets/{ticketId}/comments/{id}
        /// Tar bort en specifik kommentar.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int ticketId, int id)
        {
            // Kontrollera ticket
            var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
            if (!ticketExists)
                return NotFound(new { error = "Ticket not found." });

            var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (comment == null)
                return NotFound(new { error = "Comment not found." });

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
