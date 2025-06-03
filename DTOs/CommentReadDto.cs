using System;

namespace SupportTicketApi.DTOs
{
    public class CommentReadDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
