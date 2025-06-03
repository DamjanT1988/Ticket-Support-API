// UpdateTicketDto.cs

// Importerar attribut för datavalidering, såsom [StringLength] och [RegularExpression].
using System.ComponentModel.DataAnnotations;

namespace SupportTicketApi.DTOs
{
    /// <summary>
    /// DTO-klass som används för att ta emot data när ett befintligt ticket ska uppdateras delvis (PATCH).
    /// Här kan klienten skicka nytt värde för Description och/eller Status.
    /// </summary>
    public class UpdateTicketDto
    {
        /// <summary>
        /// Den nya beskrivningen för ticket.
        /// - [StringLength(1000)]: Begränsar längden på strängen till max 1000 tecken.
        ///   Om klienten skickar en beskrivning längre än 1000 tecken misslyckas modellvalideringen
        ///   och API:et returnerar 400 Bad Request.
        /// - Fältet är frivilligt (ingen [Required]), vilket innebär att om ingen ny beskrivning skickas,
        ///   lämnas befintlig Description oförändrad.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Den nya statusen för ticket.
        /// - [RegularExpression("Open|In Progress|Closed", ErrorMessage = "Status måste vara 'Open', 'In Progress' eller 'Closed'.")]:
        ///   Valideringen säkerställer att endast de tre tillåtna värdena accepteras.
        ///   Om klienten skickar något annat värde än dessa returneras 400 Bad Request med angivet felmeddelande.
        /// - Fältet är frivilligt (ingen [Required]), så om klienten inte inkluderar Status i JSON behålls den befintliga statusen.
        /// </summary>
        [RegularExpression("Open|In Progress|Closed", ErrorMessage = "Status måste vara 'Open', 'In Progress' eller 'Closed'.")]
        public string Status { get; set; }
    }
}
