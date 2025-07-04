using Microsoft.EntityFrameworkCore;
using HardLock.Notification.Data;
using HardLock.Notification.Services;
using HardLock.Notification.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapPost("/api/notifications", async (NotificationRequest request, INotificationService notificationService) =>
{
    try
    {
        var result = await notificationService.SendNotificationAsync(request);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = "Failed to send notification", Details = ex.Message });
    }
})
.WithName("SendNotification");

app.Run(); 