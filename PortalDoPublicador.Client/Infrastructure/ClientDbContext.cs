using System.Text.Json;
using System.Text.Json.Serialization;
using DnetIndexedDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.JSInterop;
using PortalDoPublicador.Shared.Infrastructure.Sync;

namespace PortalDoPublicador.Client.Infrastructure;

public class ClientDbContext(IndexedDbInterop indexedDb, IJSRuntime jsRuntime) : DbContext
{
    public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        if (Database.CurrentTransaction != null)
        {
            return await base.SaveChangesAsync(ct);
        }

        using var transaction = await Database.BeginTransactionAsync(ct);

        try
        {
            var entradasModificadas = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(e => new { Entry = e, Estado = e.State })
                .ToList();

            var result = await base.SaveChangesAsync(ct);

            var datetime = DateTime.UtcNow;
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var syncPayloads = entradasModificadas.Select(x => new SyncPayload
            {
                Timestamp = datetime,
                Entity = JsonSerializer.Serialize(x.Entry.Entity, options),
                EntityName = x.Entry.Entity.GetType().Name,
                EntityState = x.Estado
            }).ToList();

            if (entradasModificadas.Count != 0)
            {
                await indexedDb.AddItems<SyncPayload>("SyncPushQueue", syncPayloads);
                await transaction.CommitAsync(ct);
            }

            await transaction.CommitAsync(ct);

            if (entradasModificadas.Count != 0)
            {
                await jsRuntime.InvokeVoidAsync("SincronizacaoOffline.registrarSync");
            }

            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public Task<int> SaveLocalChangesAsync(CancellationToken ct = default)
    {
        return base.SaveChangesAsync(ct);
    }
}
