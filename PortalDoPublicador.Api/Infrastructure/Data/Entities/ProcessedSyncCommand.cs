namespace PortalDoPublicador.Api.Infrastructure.Data.Entities;

public class ProcessedSyncCommand
{
    public Guid Id { get; set; }
    public DateTime ProcessadoEm { get; set; } = DateTime.UtcNow;
}
