using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.DTOs
{
    public class UpdateTicketDto
    {
        [StringLength(1000)]
        public string Description { get; set; }

        [RegularExpression("Open|In Progress|Closed", ErrorMessage = "Status måste vara 'Open', 'In Progress' eller 'Closed'.")]
        public string Status { get; set; }
    }
}
