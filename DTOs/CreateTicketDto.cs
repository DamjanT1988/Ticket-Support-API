using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.DTOs
{
    public class CreateTicketDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
    }
}
