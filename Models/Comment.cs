// Comment.cs

// Inkluderar grundläggande .NET-typer som DateTime.
using System;
// Ger tillgång till attribut för validering, t.ex. [Required], [StringLength].
using System.ComponentModel.DataAnnotations;
// Ger möjlighet att ignorera vissa egenskaper vid JSON-serialisering.
using System.Text.Json.Serialization;

namespace SupportTicketApi.Models
{
    /// <summary>
    /// Kommentarmodell som representerar en kommentar kopplad till ett supportticket.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Primärnyckel för kommentaren. Autogenererat av databasen.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Utländsk nyckel som pekar på det Ticket (supportärende) som denna kommentar hör till.
        /// </summary>
        [Required]
        public int TicketId { get; set; }

        /// <summary>
        /// Själva textinnehållet i kommentaren.
        /// Är obligatorisk ([Required]) och får vara max 500 tecken ([StringLength]).
        /// </summary>
        [Required]
        [StringLength(500, ErrorMessage = "Text får vara max 500 tecken.")]
        public string Text { get; set; }

        /// <summary>
        /// Tidsstämpel (UTC) då kommentaren skapades.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation-egenskap som refererar tillbaka till det Ticket som kommentaren hör till.
        /// Döljs vid JSON-serialisering ([JsonIgnore]) för att undvika cirkulära referenser
        /// eller onödig data i API-svaret.
        /// </summary>
        [JsonIgnore]
        public Ticket Ticket { get; set; }
    }
}
