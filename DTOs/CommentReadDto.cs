// CommentReadDto.cs

// Importerar grundläggande .NET-typer, t.ex. DateTime.
using System;

namespace SupportTicketApi.DTOs
{
    /// <summary>
    /// DTO-klass som används för att skicka data om en kommentar tillbaka till klienten.
    /// Den representerar precis de fält som klienten ska få se när man hämtar kommentarer.
    /// </summary>
    public class CommentReadDto
    {
        /// <summary>
        /// Unikt ID för kommentaren. Detta är primärnyckeln som genereras av databasen.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Det ticket-ID som denna kommentar hör till. Detta kopplar kommentaren till ett specifikt ärende.
        /// </summary>
        public int TicketId { get; set; }

        /// <summary>
        /// Själva textinnehållet i kommentaren.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Tidsstämpel (UTC) för när kommentaren skapades. 
        /// Används för att visa ordningen på kommentarer och när de publicerades.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
