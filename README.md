# TicketSupportAPI

## Beskrivning
TicketSupportAPI är en ASP.NET Core Web API-lösning med enhetstester för att hantera support-ärenden (tickets) och kommentarer. Projektet använder SQLite som databas via Entity Framework Core och xUnit för enhetstester. Det är ett kodtest.

## Förutsättningar
- .NET 9 SDK installerad
- Visual Studio 2022 (eller motsvarande IDE med .NET 9-stöd)
- (Valfritt) SQLite-verktyg för att inspektera databasen, t.ex. DB Browser for SQLite

## Mappstruktur
```
C:\kodprojekt  TicketSupportAPI    TicketSupportAPI.csproj
    Controllers    Data    DTOs    Models    Program.cs
  TicketSupportAPI.Tests    TicketSupportAPI.Tests.csproj
    ControllersTests    Helpers    README.md
  TicketSupportAPI.sln
```

- `TicketSupportAPI`: Huvud-API-projekt med controllers, datalager, modeller och konfiguration.  
- `TicketSupportAPI.Tests`: Separat xUnit-projekt för enhetstester, med egen README.md.  

## Installation och körning

### 1. Klona projektet
```bash
git clone <repository-url>
cd TicketSupportAPI
```

### 2. Öppna i Visual Studio
1. Starta Visual Studio 2022.  
2. Välj **Open a project or solution** och öppna `TicketSupportAPI.sln`.  
3. Se till att både `TicketSupportAPI` och `TicketSupportAPI.Tests` visas i Solution Explorer.

### 3. Återställ och bygg
- **Via Visual Studio**  
  1. Gå till **Build → Rebuild Solution** (Ctrl+Shift+B).  
  2. Kontrollera att ingen byggvarning eller fel uppstår.

- **Via kommandoraden**  
  ```bash
  cd C:\kodprojekt  dotnet restore
  dotnet build
  ```

### 4. Konfigurera databas
Projektet använder SQLite. När du kör API-projektet första gången skapas SQLite-filen `SupportTickets.db` automatiskt via EF Core-migrationer. Du kan köra migrationer manuellt om så önskas:
```bash
cd TicketSupportAPI
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Kör API-servern
- **Via Visual Studio**  
  1. Högerklicka på `TicketSupportAPI` i Solution Explorer och välj **Set as Startup Project**.  
  2. Tryck F5 eller klicka på **Start**-knappen för att starta servern (HTTPS på `https://localhost:7207`, HTTP på `http://localhost:5148`).

- **Via kommandoraden**  
  ```bash
  cd TicketSupportAPI
  dotnet run
  ```

Swagger UI är tillgängligt i utvecklingsläge på `https://localhost:7207/swagger/index.html`.

## Felhantering

### Program.cs
Global felhantering är konfigurerad via `app.UseExceptionHandler(...)` i `Program.cs`. Vid oväntade undantag returneras statuskod 500 och ett JSON-objekt med ett generellt felmeddelande och undantagsdetaljer:
```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandlerFeature != null)
        {
            var ex = exceptionHandlerFeature.Error;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Ett internt serverfel har inträffat",
                details = ex.Message
            });
        }
    });
});
```

### Controllers
- **TicketsController** och **CommentsController** har injicerade `ILogger<T>` och omger databasoperationer i `try/catch`.  
- Typiska feltyper:
  - `DbUpdateException` fångas för databasrelaterade fel och returnerar `500 Internal Server Error` med JSON–felmeddelande.  
  - Generella `Exception` fångas för oväntade fel och returnerar `500 Internal Server Error`.  
- Vid valideringsfel (`ModelState`) returnerar metoder `ValidationProblem(ModelState)`, vilket ger en enhetlig JSON-struktur för fel.

## API Endpoints

### Tickets
- **GET /api/tickets**  
  Hämtar alla tickets. Valfritt query-param `status` för att filtrera (t.ex. `/api/tickets?status=Open`). Returnerar 200 eller 500 vid fel.

- **GET /api/tickets/{id}**  
  Hämtar ett enskilt ticket och dess kommentarer. Returnerar 200, 404 om ej hittas, eller 500 vid serverfel.

- **POST /api/tickets**  
  Skapar ett nytt ticket. Exempel på JSON-body:
  ```json
  {
    "title": "Problem med utloggning",
    "description": "När jag försöker logga ut så får jag ett felmeddelande."
  }
  ```
  Returnerar 201, 400 vid valideringsfel, eller 500 vid databas- eller serverfel.

- **PATCH /api/tickets/{id}**  
  Uppdaterar ett befintligt ticket (status och/eller description). Exempel:
  ```json
  {
    "status": "In Progress",
    "description": "Ändrad beskrivning"
  }
  ```
  Returnerar 204, 400 vid ogiltiga data, 404 om ej hittas, eller 500 vid databas- eller serverfel.

- **DELETE /api/tickets/{id}**  
  Tar bort ett ticket samt alla kopplade kommentarer. Returnerar 204, 404 om ej hittas, eller 500 vid fel.

### Comments
- **GET /api/tickets/{ticketId}/comments**  
  Hämtar alla kommentarer för ett givet ticket. Returnerar 200, 404 om ticket ej finns, eller 500 vid fel.

- **GET /api/tickets/{ticketId}/comments/{id}**  
  Hämtar en specifik kommentar. Returnerar 200, 404 om ej hittas, eller 500 vid fel.

- **POST /api/tickets/{ticketId}/comments**  
  Skapar en ny kommentar för ett ticket. Exempel:
  ```json
  {
    "text": "Här är en kommentar."
  }
  ```
  Returnerar 201, 400 vid valideringsfel, 404 om ticket ej finns, eller 500 vid fel.

- **DELETE /api/tickets/{ticketId}/comments/{id}**  
  Tar bort en kommentar. Returnerar 204, 404 om ej hittas, eller 500 vid fel.

## xUnit Enhetstester

### Tests-projekt
Enhetstesterna ligger i separata projektmappen **`TicketSupportAPI.Tests`**. Där finns:
- `ControllersTests/` – Testklasser för `TicketsController` och `CommentsController`.
- `Helpers/` – Hjälparklasser för att initiera en InMemory-databas.
- `README.md` – Egna instruktioner för att köra tester.

### Köra testerna
1. **Öppna testprojektets README**  
   Navigera till `TicketSupportAPI.Tests/README.md` för detaljerade instruktioner om testkörning.

2. **Via Visual Studio**  
   - Öppna **Test Explorer** (Test → Test Explorer).  
   - Klicka på **Run All** för att köra alla tester i `TicketSupportAPI.Tests`.

3. **Via kommandoraden**  
   ```bash
   cd TicketSupportAPI.Tests
   dotnet test
   ```
   Detta kör alla xUnit-tester i det separata testprojektet.

4. **Kontinuerlig testkörning (valfritt)**  
   För automatisk testkörning vid kodändringar:
   ```bash
   dotnet watch test
   ```

## CORS och Proxy (om du kör en separat frontend)
Om du utvecklar en React-frontend som anropar API:t på annan port (t.ex. 3000) är CORS konfigurerat i `Program.cs` med politik “AllowAll”. För Create React App kan du lägga in följande i `package.json`:
```json
"proxy": "https://localhost:7207"
```
Då proxas alla anrop från React till API:t.

## XML-kommentarer (< summary >)

Följande punkter förklarar varför vi använder < summary >-taggar i C#-koden:

    Automatisk dokumentation: Verktyg som DocFX eller Sandcastle kan läsa XML-kommentarerna och generera en strukturerad referensdokumentation för API:et.

    IntelliSense-stöd: När man hovrar över klasser, metoder eller egenskaper i Visual Studio/VS Code visas texten i <summary> som hjälptext, vilket underlättar förståelse utan att behöva läsa hela implementationen.

    Standardpraxis: Genom att dokumentera publika klasser och metoder med XML-kommentarer blir koden mer underhållbar och enklare att använda för andra utvecklare.

## Design Decisions

När vi byggde upp backend-API:t har vi gjort flera medvetna val för att säkerställa ett tydligt, testbart och underhållsbart system. Här är några av de viktigaste:

### 1. Separation mellan Domänmodeller och Externa API-modeller (DTOs)
- **Varför DTOs?**  
  - Skyddar interna domänklasser från överexponering (över-/under-posting).  
  - Ger full kontroll över exakt vilka fält som kan skickas in respektive returneras.  
  - Förenklar versionering av API:t: vi kan ändra våra interna modeller utan att bryta klienter som använder DTO-kontraktet.  
- **Hur?**  
  - `CreateTicketDto`/`UpdateTicketDto` för inkommande data (write models).  
  - `TicketReadDto`/`CommentReadDto` för utgående data (read models).  
  - Manuell mappning (eller via AutoMapper) mellan DTOs och domänmodeller.

### 2. Tydlig Layering & Ansvarsuppdelning

├─ Controllers/ ← Tar emot HTTP-anrop, ansvarar för routing & statuskoder

├─ DTOs/ ← Dataöverföringsobjekt (in/ut)

├─ Models/ ← Domän­modell + valideringar (DataAnnotations)

├─ Data/ ← DbContext + migrations (EF Core)

- **Fördelar**:  
  - Måttlig komplexitet i varje lager  
  - Enklare att skriva enhetstester (t.ex. mocka DbContext i service-lagret)  
  - Klar separation mellan affärslogik, datalager och presentation

### 3. Entity Framework Core + SQLite
- **Val av EF Core** för enkel definition av modeller, relationer och automatiska migrations.  
- **SQLite** i utveckling för minimal setup (ingen extern databas krävs), men lätt att byta till t.ex. SQL Server i produktion.

### 4. DataAnnotations & Automatisk Validering
- Vi använder `[Required]`, `[StringLength]`, `[RegularExpression]` direkt på domänmodeller.  
- **ModelState-kontroller** i controllers avslår felaktiga payloads med `400 Bad Request`.

### 5. Middleware-pipeline & Felhantering
- Globala felhandlers med `UseExceptionHandler` för enhetliga svar på otippade undantag.  
- CORS-policy (`AllowAll`) för enkel integration med React-frontenden under utveckling.  
- Swagger/OpenAPI aktiverat i utvecklingsläge för interaktiv dokumentation och test.