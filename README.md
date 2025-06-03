# SupportTicketApi

## Beskrivning
SupportTicketApi är ett enkelt ärendehanteringssystem (support tickets) byggt med ASP.NET Core Web API och SQLite. Lösningen inkluderar fullständig CRUD-funktionalitet för ärenden (tickets) samt möjlighet att lägga till och hantera kommentarer för varje ärende. Syftet är att demonstrera en RESTful-arkitektur med tydlig separation av ansvar och användning av Entity Framework Core för datalagring.

## Teknologier
- .NET 9 (C#)
- ASP.NET Core Web API
- Entity Framework Core med SQLite
- Swagger (OpenAPI) för dokumentation och testning av API
- Visual Studio 2022

## Funktioner
- Skapa, läsa, uppdatera och ta bort ärenden (tickets)
- Filtrering av ärenden per status (Open, In Progress, Closed)
- Lägga till, läsa och ta bort kommentarer kopplade till ett specifikt ärende
- Automatiska tidsstämplar (`CreatedAt` och `UpdatedAt`) för ärenden och kommentarer
- Konsistent felhantering med korrekta HTTP-statuskoder
- CORS-inställningar för att tillåta anrop från externa klienter (t.ex. en React-frontend)

## Mappstruktur
```
SupportTicketApi/
│
├─ Controllers/
│   ├─ TicketsController.cs
│   └─ CommentsController.cs
│
├─ Data/
│   └─ AppDbContext.cs
│
├─ DTOs/
│   ├─ CreateTicketDto.cs
│   ├─ UpdateTicketDto.cs
│   ├─ TicketReadDto.cs
│   ├─ CommentDto.cs
│   └─ CommentReadDto.cs
│
├─ Models/
│   ├─ Ticket.cs
│   └─ Comment.cs
│
├─ Migrations/
│   └─ [Auto-genererade migrationsfiler]
│
├─ appsettings.json
├─ Program.cs
└─ SupportTickets.db    (genererad SQLite-fil)
```

## Förutsättningar
- .NET 9 SDK installerat
- Visual Studio 2022 (eller annan IDE med stöd för .NET 9)
- (Frivilligt) SQLite-verktyg för att inspektera databasen, t.ex. DB Browser for SQLite

## Installation och Konfiguration

1. **Klona projektet**  
   ```bash
   git clone <repository-url>
   cd SupportTicketApi
   ```

2. **Konfigurera `appsettings.json`**  
   Säkerställ att filen `appsettings.json` innehåller rätt connection string för SQLite:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=SupportTickets.db"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

3. **Kör Entity Framework-migrationer**  
   Öppna Package Manager Console i Visual Studio (Tools → NuGet Package Manager → Package Manager Console) och kör:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```
   Detta skapar filen `SupportTickets.db` med tabellerna `Tickets` och `Comments`.

4. **Starta API-servern**  
   I Visual Studio, tryck på **F5** eller klicka på den gröna “Play”-knappen. Alternativt kan du öppna en terminal i projektmappen och köra:
   ```bash
   dotnet run
   ```
   Applikationen kommer att lyssna på:
   - `https://localhost:7207`
   - `http://localhost:5148`

5. **Öppna Swagger UI**  
   Öppna webbläsaren och navigera till:
   ```
   https://localhost:7207/swagger/index.html
   ```
   Här kan du testa alla definierade endpoints interaktivt.

## API Endpoints

### Tickets
- **GET /api/tickets**  
  Hämtar alla tickets. Valfritt query-param `status` för att filtrera (t.ex. `/api/tickets?status=Open`).

- **GET /api/tickets/{id}**  
  Hämtar ett enskilt ticket och dess kommentarer.

- **POST /api/tickets**  
  Skapar ett nytt ticket. Exempel på JSON-body:
  ```json
  {
    "title": "Problem med utloggning",
    "description": "När jag försöker logga ut så får jag ett felmeddelande."
  }
  ```
  Returnerar `201 Created` med det skapade ärendet.

- **PATCH /api/tickets/{id}**  
  Uppdaterar ett befintligt ticket (status och/eller description). Exempel:
  ```json
  {
    "status": "In Progress",
    "description": "Ändrad beskrivning"
  }
  ```
  Returnerar `204 No Content` vid framgång.

- **DELETE /api/tickets/{id}**  
  Tar bort ett ticket samt alla kopplade kommentarer (cascade delete). Returnerar `204 No Content`.

### Comments
- **GET /api/tickets/{ticketId}/comments**  
  Hämtar alla kommentarer för ett givet ticket.

- **GET /api/tickets/{ticketId}/comments/{id}**  
  Hämtar en specifik kommentar.

- **POST /api/tickets/{ticketId}/comments**  
  Skapar en ny kommentar för ett ticket. Exempel:
  ```json
  {
    "text": "Här är en kommentar."
  }
  ```
  Returnerar `201 Created` med den skapade kommentaren.

- **DELETE /api/tickets/{ticketId}/comments/{id}**  
  Tar bort en kommentar. Returnerar `204 No Content`.

## Data- och Objektmodeller

### Ticket (Models/Ticket.cs)
- `Id` (int)
- `Title` (string, [Required], max 100 tecken)
- `Description` (string, [Required], max 1000 tecken)
- `Status` (string, [Required], bara “Open”, “In Progress” eller “Closed”)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime)
- `Comments` (ICollection<Comment>)

### Comment (Models/Comment.cs)
- `Id` (int)
- `TicketId` (int, FK)
- `Text` (string, [Required], max 500 tecken)
- `CreatedAt` (DateTime)

## DTO-klasser (se mappen `DTOs/`)
- **CreateTicketDto**: `Title`, `Description`
- **UpdateTicketDto**: `Status` (validerat mot giltiga värden), `Description`
- **TicketReadDto**: `Id`, `Title`, `Description`, `Status`, `CreatedAt`, `UpdatedAt`, `Comments`
- **CommentDto**: `Text`
- **CommentReadDto**: `Id`, `TicketId`, `Text`, `CreatedAt`

## CORS-inställningar
I `Program.cs` finns en CORS-policy “AllowAll” som tillåter alla origin, headers och metoder. Detta underlättar anrop från en separat frontend (t.ex. React) på annan port.

## Swagger (OpenAPI)
Swagger UI aktiveras automatiskt i utvecklingsmiljön. Du hittar dokumentationen på:
```
https://localhost:7207/swagger/index.html
```
Här kan du interagera med alla endpoints och se request-/response-format.

## Framtida frontend-integration
En React-baserad frontend kan pekas mot API:t genom att sätta i React-projektets `package.json`:
```json
"proxy": "https://localhost:7207"
```
Då räcker det att anropa `fetch("/api/tickets")` i React, och anropet proxas till .NET-backenden.

## Utveckling och bidrag
- Forka gärna detta repo och skicka pull requests för förbättringar eller nya funktioner.
- Överväg att lägga till enhets- och integrationstester för controllers och tjänstelager.

## Licens
Detta projekt är licensierat under MIT License. Se filen `LICENSE` för mer information.
