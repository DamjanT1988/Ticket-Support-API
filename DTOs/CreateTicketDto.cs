// CreateTicketDto.cs

// Importerar attribut för datavalidering, såsom [Required] och [StringLength].
using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.DTOs
{
    /// <summary>
    /// DTO-klass som används för att ta emot data från klienten när ett nytt ticket skapas.
    /// Den innehåller endast de fält som klienten behöver skicka via API:et.
    /// </summary>
    public class CreateTicketDto
    {
        /// <summary>
        /// Titel för det nya ticket.
        /// - [Required] innebär att fältet måste finnas i inkommande JSON, annars returneras 400 Bad Request.
        /// - [StringLength(100)] begränsar längden på strängen till max 100 tecken.
        ///   Om titeln är längre än 100 tecken misslyckas modellvalideringen.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// Beskrivning av det nya ticket.
        /// - [Required] innebär att fältet måste finnas i inkommande JSON, annars returneras 400 Bad Request.
        /// - [StringLength(1000)] begränsar längden på beskrivningstexten till max 1000 tecken.
        ///   Om beskrivningen är längre än 1000 tecken misslyckas modellvalideringen.
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
    }
}
