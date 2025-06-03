// CommentsController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ASP.NET Core MVC-bibliotek för att skapa API-kontrollers och använda attribut som [ApiController].
using Microsoft.AspNetCore.Mvc;
// Entity Framework Core för asynkrona databasoperationer (AnyAsync, ToListAsync osv).
using Microsoft.EntityFrameworkCore;
// Inkluderar ILogger för loggning.
using Microsoft.Extensions.Logging;
// Inkluderar datakontexten (AppDbContext) som definierar databastabellerna.
using SupportTicketApi.Data;
// Inkluderar DTO-klasser (CommentDto och CommentReadDto).
using SupportTicketApi.DTOs;
// Inkluderar domänmodellerna (Comment, Ticket).
using SupportTicketApi.Models;

namespace SupportTicketApi.Controllers
{
    // Markerar att det här är en Web API-kontroller och ger automatisk validering av modeller, bindningsfelhantering etc.
    [ApiController]
    // Definierar den grundläggande routen för alla metoder i denna kontroller.
    // Här blir det: api/tickets/{ticketId}/comments
    [Route("api/tickets/{ticketId:int}/[controller]")]
    public class CommentsController : ControllerBase
    {
        // Privat, readonly fält för att spara instansen av AppDbContext (Entity Framework Core).
        private readonly AppDbContext _db;
        // Privat, readonly fält för att spara instansen av ILogger för den här kontrollern.
        private readonly ILogger<CommentsController> _logger;

        // Konstruktor som tar emot AppDbContext och ILogger via dependency injection.
        public CommentsController(AppDbContext db, ILogger<CommentsController> logger)
        {
            _db = db;           // Spara databas-kontexten i det privata fältet.
            _logger = logger;   // Spara logg-instansen i det privata fältet.
        }

        /// <summary>
        /// GET api/tickets/{ticketId}/comments
        /// Hämtar alla kommentarer för ett specifikt ticket.
        /// </summary>
        [HttpGet]
        // Anger att en lyckad (200 OK) returnering ger en lista av CommentReadDto.
        [ProducesResponseType(typeof(IEnumerable<CommentReadDto>), 200)]
        // Anger att 404 Not Found kan returneras om ticket inte hittas.
        [ProducesResponseType(404)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<CommentReadDto>>> GetAll(int ticketId)
        {
            try
            {
                // Kontrollera om ticket med det angivna ticketId existerar i databasen.
                var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
                if (!ticketExists)
                    // Om ticket inte finns, returnera 404 Not Found med ett felmeddelande.
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                // Hämta alla kommentarer som hör till ticketId, sorterade på CreatedAt (äldsta först).
                var comments = await _db.Comments
                    .Where(c => c.TicketId == ticketId)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                // Mappa varje Comment-entitet till en CommentReadDto för att skicka till klienten.
                var dtos = comments.Select(c => new CommentReadDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList();

                // Returnera 200 OK med listan av CommentReadDto som JSON.
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                // Logga felet med loggnivå Error och inkludera ticketId för kontext.
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av kommentarer för ticket med id={TicketId}.", ticketId);
                // Returnera 500 Internal Server Error med felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av kommentarer.", details = ex.Message });
            }
        }

        /// <summary>
        /// POST api/tickets/{ticketId}/comments
        /// Skapar en ny kommentar kopplad till ett ticket.
        /// </summary>
        [HttpPost]
        // Anger att en lyckad (201 Created) returnerar en CommentReadDto.
        [ProducesResponseType(typeof(CommentReadDto), 201)]
        // Anger att 400 Bad Request kan returneras vid ogiltig payload.
        [ProducesResponseType(400)]
        // Anger att 404 Not Found kan returneras om ticket inte hittas.
        [ProducesResponseType(404)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<ActionResult<CommentReadDto>> Create(int ticketId, [FromBody] CommentDto payload)
        {
            // Kontrollera om inkommande DTO (CommentDto) är giltig utifrån valideringsattribut (t.ex. [Required]).
            if (!ModelState.IsValid)
                // Returnera automatiskt en 400 Bad Request med valideringsfel om ModelState inte är giltigt.
                return ValidationProblem(ModelState);

            try
            {
                // Hitta ticket-entiteten med FindAsync baserat på primärnyckel (ticketId).
                var ticket = await _db.Tickets.FindAsync(ticketId);
                if (ticket == null)
                    // Om ticket inte finns, returnera 404 Not Found.
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                // Skapa en ny Comment-entitet med data från payload och sätt ticketId och tidsstämpel.
                var comment = new Comment
                {
                    TicketId = ticketId,
                    Text = payload.Text,
                    CreatedAt = DateTime.UtcNow  // Använd UTC för att undvika tidszonsproblem.
                };

                // Lägg till den nya kommentaren i datakontexten.
                _db.Comments.Add(comment);
                // Spara ändringarna asynkront i databasen (INSERT).
                await _db.SaveChangesAsync();

                // Mappa den sparade Comment-entiteten till en CommentReadDto för svar.
                var dto = new CommentReadDto
                {
                    Id = comment.Id,
                    TicketId = comment.TicketId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                };

                // Returnera 201 Created och inkludera headern Location som pekar på GetById-metoden för den nya kommentaren.
                return CreatedAtAction(nameof(GetById), new { ticketId = ticketId, id = comment.Id }, dto);
            }
            catch (DbUpdateException dbEx)
            {
                // Om det uppstår ett databasfel (t.ex. constraint-violations), logga det.
                _logger.LogError(dbEx, "Databasfel vid skapande av kommentar för ticket med id={TicketId}.", ticketId);
                // Returnera 500 Internal Server Error med specifika databas-felmeddelanden.
                return StatusCode(500, new { error = "Fel vid sparande i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Fångar alla andra oväntade fel, logga dem.
                _logger.LogError(ex, "Ett oväntat fel inträffade vid skapande av kommentar för ticket med id={TicketId}.", ticketId);
                // Returnera 500 Internal Server Error med generellt felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid skapande av kommentar.", details = ex.Message });
            }
        }

        /// <summary>
        /// GET api/tickets/{ticketId}/comments/{id}
        /// Hämtar en enskild kommentar.
        /// </summary>
        [HttpGet("{id:int}")]
        // Anger att 200 OK returnerar en CommentReadDto.
        [ProducesResponseType(typeof(CommentReadDto), 200)]
        // Anger att 404 Not Found kan returneras om ticket eller kommentar inte hittas.
        [ProducesResponseType(404)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<ActionResult<CommentReadDto>> GetById(int ticketId, int id)
        {
            try
            {
                // Kontrollera först att ticket med ticketId existerar.
                var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
                if (!ticketExists)
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                // Hämta den enskilda kommentaren som matchar både id och ticketId.
                var comment = await _db.Comments
                    .FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
                if (comment == null)
                    // Om ingen matchande kommentar hittas, returnera 404 Not Found.
                    return NotFound(new { error = $"Kommentar med id={id} kunde inte hittas för ticket id={ticketId}." });

                // Mappa Comment-entiteten till CommentReadDto.
                var dto = new CommentReadDto
                {
                    Id = comment.Id,
                    TicketId = comment.TicketId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                };

                // Returnera 200 OK med DTO:n i body.
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // Logga oväntat fel med kontext (ticketId och commentId).
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av kommentar med id={CommentId} för ticket id={TicketId}.", id, ticketId);
                // Returnera 500 Internal Server Error med felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av kommentar.", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE api/tickets/{ticketId}/comments/{id}
        /// Tar bort en specifik kommentar.
        /// </summary>
        [HttpDelete("{id:int}")]
        // Anger att en lyckad borttagning returnerar 204 No Content.
        [ProducesResponseType(204)]
        // Anger att 404 Not Found kan returneras om ticket eller kommentar inte hittas.
        [ProducesResponseType(404)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int ticketId, int id)
        {
            try
            {
                // Kontrollera att ticket med ticketId existerar.
                var ticketExists = await _db.Tickets.AnyAsync(t => t.Id == ticketId);
                if (!ticketExists)
                    return NotFound(new { error = $"Ticket med id={ticketId} kunde inte hittas." });

                // Hämta kommentaren som matchar både id och ticketId.
                var comment = await _db.Comments
                    .FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
                if (comment == null)
                    // Om kommentaren inte finns, returnera 404 Not Found.
                    return NotFound(new { error = $"Kommentar med id={id} kunde inte hittas för ticket id={ticketId}." });

                // Markera kommentaren som borttagen.
                _db.Comments.Remove(comment);
                // Spara ändringarna i databasen (DELETE).
                await _db.SaveChangesAsync();

                // Returnera 204 No Content (ingen body).
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                // Logga databasfel vid borttagning.
                _logger.LogError(dbEx, "Databasfel vid borttagning av kommentar med id={CommentId} för ticket id={TicketId}.", id, ticketId);
                // Returnera 500 Internal Server Error med detaljer om databasfelet.
                return StatusCode(500, new { error = "Fel vid borttagning i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Logga oväntat fel vid borttagning.
                _logger.LogError(ex, "Ett oväntat fel uppstod vid borttagning av kommentar med id={CommentId} för ticket id={TicketId}.", id, ticketId);
                // Returnera 500 Internal Server Error med generellt felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid borttagning av kommentar.", details = ex.Message });
            }
        }
    }
}
