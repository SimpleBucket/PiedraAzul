using PiedraAzul.Notifications.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient("WhatsApp");
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddSingleton<WhatsAppSender>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
