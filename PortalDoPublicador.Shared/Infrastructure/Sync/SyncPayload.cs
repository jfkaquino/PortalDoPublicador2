using Microsoft.EntityFrameworkCore;

namespace PortalDoPublicador.Shared.Infrastructure.Sync;

public class SyncPayload()
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required DateTime Timestamp { get; init; }
    public required string Entity { get; init; }
    public required string EntityName { get; init; }
    public required EntityState EntityState { get; init; }
}