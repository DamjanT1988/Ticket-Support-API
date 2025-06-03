using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.DTOs
{
    public class CommentDto
    {
        [Required]
        [StringLength(500)]
        public string Text { get; set; }
    }
}
