using System.Text.Json;
using DnetIndexedDb;
using Microsoft.EntityFrameworkCore;
using PortalDoPublicador.Shared.Infrastructure.Sync;

namespace PortalDoPublicador.Client.Infrastructure.Sync;

public class PullProcessor(IndexedDbInterop indexedDb, ClientDbContext context)
{
    public async Task ProcessarFilaPullAsync()
    {
        var payloads = await indexedDb.GetAll<SyncPayload>("SyncPullQueue");
        if (payloads?.Count == 0)
        {
            return;
        }

        var entityTypes = context.Model.GetEntityTypes()
            .Select(t => t.ClrType)
            .Where(t => typeof(ISyncable).IsAssignableFrom(t))
            .ToDictionary(t => t.Name, t => t);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var payloadsProcessados = new List<Guid>();
        foreach (var payload in payloads)
        {
            if (entityTypes.TryGetValue(payload.EntityName, out var type))
            {
                var obj = JsonSerializer.Deserialize(payload.Entity, type, options);
                if (obj == null)
                {
                    continue;
                }

                if (payload.EntityState == EntityState.Deleted)
                {
                    context.Remove(obj);
                }
                else
                {
                    var entry = context.Entry(obj);
                    if (entry.State == EntityState.Detached)
                    {
                        context.Update(obj);
                    }
                }
            }

            payloadsProcessados.Add(payload.Id);
        }

        await context.SaveLocalChangesAsync();
        if (payloadsProcessados.Count > 0)
        {
            var tarefasDelecao = payloadsProcessados.Select(id =>
                indexedDb.DeleteByKey<Guid>("SyncPullQueue", id).AsTask()
            );

            await Task.WhenAll(tarefasDelecao);
        }
    }
}