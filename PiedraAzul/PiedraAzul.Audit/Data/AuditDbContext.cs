using Microsoft.EntityFrameworkCore;
using PiedraAzul.Audit.Models;

namespace PiedraAzul.Audit.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditEntry> Entries => Set<AuditEntry>();
}
