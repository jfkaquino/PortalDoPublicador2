using Microsoft.EntityFrameworkCore;
using PortalDoPublicador.Api.Infrastructure.Data.Entities;
using PortalDoPublicador.Shared.Infrastructure.Data;

namespace PortalDoPublicador.Api.Infrastructure.Data;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : SharedDbContext(options)
{
    public DbSet<ProcessedSyncCommand> ComandosProcessados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração específica da API
        modelBuilder.Entity<ProcessedSyncCommand>()
            .HasKey(c => c.Id);
    }
}
