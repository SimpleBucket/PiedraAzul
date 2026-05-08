using Microsoft.EntityFrameworkCore;
using PiedraAzul.Audit.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=audit.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AuditDbContext>().Database.EnsureCreated();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
