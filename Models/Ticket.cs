using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title får vara max 100 tecken.")]
        public string Title { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Description får vara max 1000 tecken.")]
        public string Description { get; set; }

        [Required]
        [RegularExpression("Open|In Progress|Closed", ErrorMessage = "Status måste vara 'Open', 'In Progress' eller 'Closed'.")]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property för kommentarer
        public ICollection<Comment> Comments { get; set; }
    }
}
