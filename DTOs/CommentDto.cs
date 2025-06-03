// CommentDto.cs

// Ger tillgång till attribut för validering, t.ex. [Required] och [StringLength].
using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.DTOs
{
    /// <summary>
    /// DTO-klass som används för att ta emot data när en ny kommentar skapas via API:et.
    /// Innehåller endast de fält som klienten får skicka (i detta fall endast Text).
    /// </summary>
    public class CommentDto
    {
        /// <summary>
        /// Själva textinnehållet i kommentaren.
        /// - [Required]: Fältet måste vara ifyllt.
        /// - [StringLength(500)]: Texten får vara högst 500 tecken lång.
        /// Om valideringen misslyckas returnerar API:et en 400 Bad Request.
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Text { get; set; }
    }
}
