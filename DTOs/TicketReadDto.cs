using System;
using System.Collections.Generic;

namespace SupportTicketApi.DTOs
{
    public class TicketReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Vi inkluderar en lista av CommentReadDto (tom vid GET /tickets om vi vill)
        public List<CommentReadDto> Comments { get; set; }
    }
}
