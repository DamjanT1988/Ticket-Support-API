using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SupportTicketApi.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Text får vara max 500 tecken.")]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property bakåt till ticket
        [JsonIgnore] // För att undvika eventuella loopar vid serialisering
        public Ticket Ticket { get; set; }
    }
}
