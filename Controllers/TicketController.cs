// TicketsController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ASP.NET Core MVC-bibliotek för att skapa API-kontrollers och använda attribut som [ApiController].
using Microsoft.AspNetCore.Mvc;
// Entity Framework Core för att interagera med databasen, inklusive eager loading (Include) och asynkrona operationer.
using Microsoft.EntityFrameworkCore;
// Inkluderar ILogger för loggning.
using Microsoft.Extensions.Logging;
// Inkluderar datakontexten (AppDbContext) som definierar databastabellerna.
using SupportTicketApi.Data;
// Inkluderar DTO-klasser (TicketReadDto, CreateTicketDto, UpdateTicketDto).
using SupportTicketApi.DTOs;
// Inkluderar domänmodellerna (Ticket, Comment).
using SupportTicketApi.Models;

namespace SupportTicketApi.Controllers
{
    // Markerar att det här är en Web API-kontroller och aktiverar automatisk modellvalidering med mera.
    [ApiController]
    // Definierar grund-routen för alla metoder i denna kontroller (api/tickets).
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        // Privat, readonly fält för AppDbContext (database context).
        private readonly AppDbContext _db;
        // Privat, readonly fält för ILogger<TicketsController>.
        private readonly ILogger<TicketsController> _logger;

        // Konstruktor som tar emot AppDbContext och ILogger via dependency injection.
        public TicketsController(AppDbContext db, ILogger<TicketsController> logger)
        {
            _db = db;         // Spara databas-kontexten.
            _logger = logger; // Spara logg-instansen.
        }

        /// <summary>
        /// GET api/tickets
        /// Hämtar alla tickets, ev. filtrerade på status (query param).
        /// </summary>
        [HttpGet]
        // Anger att en lyckad (200 OK) returnerar en lista av TicketReadDto.
        [ProducesResponseType(typeof(IEnumerable<TicketReadDto>), 200)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetAll([FromQuery] string status)
        {
            try
            {
                // Bygg en grundläggande IQueryable för Tickets. Include(t => t.Comments) gör så att kommentarer hämtas samtidigt (eager loading).
                IQueryable<Ticket> query = _db.Tickets.Include(t => t.Comments);

                // Om query-parametern "status" är satt (ej null eller tom sträng).
                if (!string.IsNullOrEmpty(status))
                {
                    // Definiera vilka statusvärden som är tillåtna.
                    var allowed = new[] { "Open", "In Progress", "Closed" };
                    // Om status inte finns i tillåtna listan, returnera 400 Bad Request.
                    if (!allowed.Contains(status))
                    {
                        return BadRequest(new { error = "Ogiltigt statusvärde. Måste vara 'Open', 'In Progress' eller 'Closed'." });
                    }
                    // Filtrera queryn så att endast tickets med angivet status hämtas.
                    query = query.Where(t => t.Status == status);
                }

                // Kör queryn asynkront, sortera tickets så att de senast skapade kommer först.
                var tickets = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                // Mappa varje Ticket-entitet till TicketReadDto, inklusive kommentarer.
                var result = tickets.Select(t => new TicketReadDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    // Mappa relaterade kommentarer till CommentReadDto.
                    Comments = t.Comments.Select(c => new CommentReadDto
                    {
                        Id = c.Id,
                        TicketId = c.TicketId,
                        Text = c.Text,
                        CreatedAt = c.CreatedAt
                    }).ToList()
                }).ToList();

                // Returnera 200 OK med listan av TicketReadDto som JSON.
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Logga oväntat fel vid hämtning av tickets.
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av alla tickets.");
                // Returnera 500 Internal Server Error med felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av ärenden.", details = ex.Message });
            }
        }

        /// <summary>
        /// GET api/tickets/{id}
        /// Hämtar ett enskilt ticket med alla kommentarer.
        /// </summary>
        [HttpGet("{id:int}")]
        // Anger att en lyckad (200 OK) returnerar en TicketReadDto.
        [ProducesResponseType(typeof(TicketReadDto), 200)]
        // Anger att 404 Not Found kan returneras om ticket inte hittas.
        [ProducesResponseType(404)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<ActionResult<TicketReadDto>> GetById(int id)
        {
            try
            {
                // Hämta ticket-entiteten inklusive relaterade kommentarer (eager loading).
                var ticket = await _db.Tickets
                    .Include(t => t.Comments)
                    .FirstOrDefaultAsync(t => t.Id == id);

                // Om ticket inte hittas, returnera 404 Not Found.
                if (ticket == null)
                    return NotFound(new { error = $"Ticket med id={id} kunde inte hittas." });

                // Mappa entiteten till DTO inklusive kommentarer.
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

                // Returnera 200 OK med DTO:n.
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // Logga oväntat fel vid hämtning av ett enskilt ticket.
                _logger.LogError(ex, "Ett fel uppstod vid hämtning av ticket med id={TicketId}.", id);
                // Returnera 500 Internal Server Error med felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid hämtning av ärende.", details = ex.Message });
            }
        }

        /// <summary>
        /// POST api/tickets
        /// Skapar ett nytt ticket.
        /// </summary>
        [HttpPost]
        // Anger att en lyckad (201 Created) returnerar en TicketReadDto.
        [ProducesResponseType(typeof(TicketReadDto), 201)]
        // Anger att 400 Bad Request kan returneras vid ogiltig payload.
        [ProducesResponseType(400)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<ActionResult<TicketReadDto>> Create([FromBody] CreateTicketDto payload)
        {
            // Kontrollera att DTO:n är giltig utifrån valideringsattribut.
            if (!ModelState.IsValid)
                // Returnera 400 Bad Request automatiskt med valideringsfel.
                return ValidationProblem(ModelState);

            // Skapa en ny Ticket-entitet med data från payload och sätt default-värden.
            var ticket = new Ticket
            {
                Title = payload.Title,
                Description = payload.Description,
                Status = "Open",               // Standardstatus vid skapande är "Open".
                CreatedAt = DateTime.UtcNow,   // Använd UTC för att undvika tidszonsproblem.
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                // Lägg till ticket i kontexten.
                _db.Tickets.Add(ticket);
                // Spara till databasen asynkront (INSERT).
                await _db.SaveChangesAsync();

                // Mappa entiteten till DTO (med en tom lista för kommentarer eftersom det är nytt).
                var dto = new TicketReadDto
                {
                    Id = ticket.Id,
                    Title = ticket.Title,
                    Description = ticket.Description,
                    Status = ticket.Status,
                    CreatedAt = ticket.CreatedAt,
                    UpdatedAt = ticket.UpdatedAt,
                    Comments = new List<CommentReadDto>()  // Inga kommentarer ännu.
                };

                // Returnera 201 Created och inkludera headern Location som pekar på GetById-metoden för det nyskapade ticket.
                return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, dto);
            }
            catch (DbUpdateException dbEx)
            {
                // Logga databasfel vid skapande av ticket.
                _logger.LogError(dbEx, "Databasfel vid skapande av ticket.");
                // Returnera 500 Internal Server Error med detaljer om databasfelet.
                return StatusCode(500, new { error = "Fel vid sparande i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Logga oväntat fel vid skapande av ticket.
                _logger.LogError(ex, "Ett oväntat fel uppstod vid skapande av ticket.");
                // Returnera 500 Internal Server Error med generellt felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid skapande av ärende.", details = ex.Message });
            }
        }

        /// <summary>
        /// PATCH api/tickets/{id}
        /// Uppdaterar status och/eller beskrivning på ett ticket.
        /// </summary>
        [HttpPatch("{id:int}")]
        // Anger att en lyckad uppdatering returnerar 204 No Content.
        [ProducesResponseType(204)]
        // Anger att 400 Bad Request kan returneras vid ogiltig payload eller om inget finns att uppdatera.
        [ProducesResponseType(400)]
        // Anger att 404 Not Found kan returneras om ticket inte hittas.
        [ProducesResponseType(404)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketDto payload)
        {
            // Kontrollera att DTO:n är giltig utifrån valideringsattribut.
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                // Hämta befintlig ticket-entitet baserat på primärnyckel (id).
                var ticket = await _db.Tickets.FindAsync(id);
                if (ticket == null)
                    // Om ticket inte finns, returnera 404 Not Found.
                    return NotFound(new { error = $"Ticket med id={id} kunde inte hittas." });

                // Flagga för att upptäcka om något värde faktiskt ändras.
                bool changed = false;

                // Om payload.Description är satt (ej null/empty) och skiljer sig från befintligt värde:
                if (!string.IsNullOrEmpty(payload.Description) && payload.Description != ticket.Description)
                {
                    ticket.Description = payload.Description; // Uppdatera beskrivning.
                    changed = true;                            // Markera att något har ändrats.
                }

                // Om payload.Status är satt (ej null/empty) och skiljer sig från befintligt värde:
                if (!string.IsNullOrEmpty(payload.Status) && payload.Status != ticket.Status)
                {
                    ticket.Status = payload.Status; // Uppdatera status.
                    changed = true;                 // Markera att något har ändrats.
                }

                // Om inga fält ändrats (changed == false), returnera 400 Bad Request.
                if (!changed)
                    return BadRequest(new { error = "Inga nya eller giltiga fält att uppdatera." });

                // Uppdatera tidsstämpeln UpdatedAt eftersom något ändrats.
                ticket.UpdatedAt = DateTime.UtcNow;

                // Markera entiteten som uppdaterad och spara ändringar i databasen (UPDATE).
                _db.Tickets.Update(ticket);
                await _db.SaveChangesAsync();

                // Returnera 204 No Content (inga data i body).
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                // Logga databasfel vid uppdatering.
                _logger.LogError(dbEx, "Databasfel vid uppdatering av ticket med id={TicketId}.", id);
                // Returnera 500 Internal Server Error med detaljer om databasfelet.
                return StatusCode(500, new { error = "Fel vid uppdatering i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Logga oväntat fel vid uppdatering.
                _logger.LogError(ex, "Ett oväntat fel inträffade vid uppdatering av ticket med id={TicketId}.", id);
                // Returnera 500 Internal Server Error med generellt felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid uppdatering av ärende.", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE api/tickets/{id}
        /// Tar bort ett ticket (och samtliga kommentarer tack vare Cascade).
        /// </summary>
        [HttpDelete("{id:int}")]
        // Anger att en lyckad borttagning returnerar 204 No Content.
        [ProducesResponseType(204)]
        // Anger att 404 Not Found kan returneras om ticket inte hittas.
        [ProducesResponseType(404)]
        // Anger att 500 Internal Server Error kan returneras vid oväntat fel.
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Hämta ticket-entiteten baserat på primärnyckel (id).
                var ticket = await _db.Tickets.FindAsync(id);
                if (ticket == null)
                    // Om ticket inte finns, returnera 404 Not Found.
                    return NotFound(new { error = $"Ticket med id={id} kunde inte hittas." });

                // Markera ticket-entiteten som borttagen.
                _db.Tickets.Remove(ticket);
                // Spara ändringar i databasen (DELETE). Tack vare Cascade tas alla relaterade kommentarer också bort.
                await _db.SaveChangesAsync();

                // Returnera 204 No Content.
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                // Logga databasfel vid borttagning.
                _logger.LogError(dbEx, "Databasfel vid borttagning av ticket med id={TicketId}.", id);
                // Returnera 500 Internal Server Error med detaljer om databasfelet.
                return StatusCode(500, new { error = "Fel vid borttagning i databasen.", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Logga oväntat fel vid borttagning.
                _logger.LogError(ex, "Ett oväntat fel uppstod vid borttagning av ticket med id={TicketId}.", id);
                // Returnera 500 Internal Server Error med generellt felmeddelande och undantagsdetaljer.
                return StatusCode(500, new { error = "Internt serverfel vid borttagning av ärende.", details = ex.Message });
            }
        }
    }
}
