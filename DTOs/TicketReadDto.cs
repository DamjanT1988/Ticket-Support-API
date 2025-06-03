// TicketReadDto.cs

// Importerar grundläggande .NET-typer, t.ex. DateTime.
using System;
// Inkluderar generiska kollektioner, här används List<T> för att hålla kommentarer.
using System.Collections.Generic;

namespace SupportTicketApi.DTOs
{
    /// <summary>
    /// DTO-klass som representerar data för ett ticket som skickas tillbaka till klienten.
    /// Den innehåller alla fält som klienten behöver se när ett ticket hämtas, inklusive en lista av kommentarer.
    /// </summary>
    public class TicketReadDto
    {
        /// <summary>
        /// Unikt ID för ticket. Primärnyckel som genereras av databasen.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Titel för ärendet.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Beskrivning av ärendet.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status för ärendet.
        /// Kan vara "Open", "In Progress" eller "Closed".
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Tidsstämpel (UTC) för när ärendet skapades.
        /// Används för att sortera och visa skapelsedatum.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tidsstämpel (UTC) för när ärendet senast uppdaterades.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// En lista av kommentarer kopplade till detta ticket.
        /// Varje kommentar representeras av en CommentReadDto.
        /// Om inga kommentarer finns kan listan vara tom.
        /// </summary>
        public List<CommentReadDto> Comments { get; set; }
    }
}
