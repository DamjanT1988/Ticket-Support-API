using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) L�gg till AppDbContext med SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) L�gg in CORS (om frontend senare k�rs p� annan host)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// 3) L�gg till controllers
builder.Services.AddControllers();

// 4) L�gg till Swagger/OpenAPI (valfritt, men underl�ttar test)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Aktivera CORS�policy
app.UseCors("AllowAll");

// Aktivera Swagger (i Development-l�ge)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
