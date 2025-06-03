// AppDbContext.cs

// Inkluderar Entity Framework Core för att definiera databas-kontext och entitetskonfiguration.
using Microsoft.EntityFrameworkCore;
// Inkluderar domänmodellklasserna (Ticket och Comment).
using SupportTicketApi.Models;
// (Observera att System.Net.Sockets här inte används i koden; det kan tas bort utan påverkan.)
using System.Net.Sockets;

namespace SupportTicketApi.Data
{
    // AppDbContext är EF Core:s databas-kontext, som definierar tabeller (DbSet) och relationer.
    public class AppDbContext : DbContext
    {
        // Konstruktor som tar emot DbContextOptions för att konfigurera koppling till databas (t.ex. anslutningssträng).
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            // Bas-konstruktorn ansvarar för att initiera kontexten med de givna alternativ.
        }

        // DbSet<Ticket> representerar tabellen "Tickets" i databasen.
        public DbSet<Ticket> Tickets { get; set; }
        // DbSet<Comment> representerar tabellen "Comments" i databasen.
        public DbSet<Comment> Comments { get; set; }

        // OnModelCreating används för att konfigurera entitetsrelationer, constraints och andra modelleringsregler.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Anropa basklassens implementation för att säkerställa ev. standardkonfiguration.
            base.OnModelCreating(modelBuilder);

            // Konfigurera en 1→M-relation mellan Ticket och Comment:
            // - En Comment har exakt ett relaterat Ticket (c.Ticket).
            // - Ett Ticket kan ha många Comments (t.Comments).
            // - Utländsk nyckel för Comment är TicketId.
            // - OnDelete(DeleteBehavior.Cascade) gör att när ett Ticket tas bort, raderas alla dess relaterade kommentarer automatiskt.
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Ticket)             // Varje Comment har en navigeringsegenskap Ticket.
                .WithMany(t => t.Comments)         // Varje Ticket har en samling Comments.
                .HasForeignKey(c => c.TicketId)    // TicketId i Comment är främmande nyckel.
                .OnDelete(DeleteBehavior.Cascade); // Cascade Delete: ta bort kommentarer när ticket tas bort.

            // Här kan man lägga till ytterligare konfiguration, t.ex. defaultvärden, index, constraints, 
            // men för detta exempel (och test) är ingen ytterligare konfiguration nödvändig.
        }
    }
}
