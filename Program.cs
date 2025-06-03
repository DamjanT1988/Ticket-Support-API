using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) Lägg till AppDbContext med SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Lägg in CORS (om frontend senare körs på annan host)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// 3) Lägg till controllers
builder.Services.AddControllers();

// 4) Lägg till Swagger/OpenAPI (valfritt, men underlättar test)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5) Globalt felhanterings‐middleware
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

            // Skicka tillbaka en “friendly”‐version av felet:
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Ett internt serverfel har inträffat",
                details = ex.Message // Du kan välja att dölja detaljer i produktionsläge
            });
        }
    });
});

// Aktivera CORS–policy
app.UseCors("AllowAll");

// Aktivera Swagger (i Development-läge)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
