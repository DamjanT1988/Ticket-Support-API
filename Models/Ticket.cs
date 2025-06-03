// Ticket.cs

// Inkluderar grundläggande .NET-typer som DateTime.
using System;
// Ger tillgång till generiska kollektioner som ICollection<T>.
using System.Collections.Generic;
// Ger tillgång till attribut för validering, t.ex. [Required], [StringLength], [RegularExpression].
using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.Models
{
    /// <summary>
    /// Modelklass som representerar ett supportärende (ticket).
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Primärnyckel för ticket. Autogenereras av databasen.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Titel för ärendet. Obligatoriskt fält ([Required]) och får vara max 100 tecken.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Title får vara max 100 tecken.")]
        public string Title { get; set; }

        /// <summary>
        /// Beskrivning av ärendet. Obligatoriskt fält ([Required]) och får vara max 1000 tecken.
        /// </summary>
        [Required]
        [StringLength(1000, ErrorMessage = "Description får vara max 1000 tecken.")]
        public string Description { get; set; }

        /// <summary>
        /// Status för ärendet. Obligatoriskt fält ([Required]).
        /// Endast värdena "Open", "In Progress" eller "Closed" är tillåtna (valideras via RegularExpression).
        /// </summary>
        [Required]
        [RegularExpression("Open|In Progress|Closed", ErrorMessage = "Status måste vara 'Open', 'In Progress' eller 'Closed'.")]
        public string Status { get; set; }

        /// <summary>
        /// Tidsstämpel (UTC) då ärendet skapades.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tidsstämpel (UTC) då ärendet senast uppdaterades.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation-egenskap som representerar samlingen av kommentarer kopplade till detta ticket.
        /// Eftersom en Ticket kan ha många Comments, används en ICollection<Comment>.
        /// Bekräftar 1→M-relationen.
        /// </summary>
        public ICollection<Comment> Comments { get; set; }
    }
}
