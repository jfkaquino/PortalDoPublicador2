using System.Text.Json;
using Blazor.IndexedDB;
using Microsoft.EntityFrameworkCore;
using PortalDoPublicador.Shared.Infrastructure.Data;
using PortalDoPublicador.Shared.Infrastructure.Sync;

namespace PortalDoPublicador.Client.Infrastructure.Sync;

public class PullProcessorService(IndexedDBManager indexedDb, SharedDbContext dbLocal)
{
    public async Task ProcessarFilaPullAsync()
    {
        // 1. Busca todos os registros na FilaPull
        var registros = await indexedDb.GetRecords<FilaPullRecord>("FilaPull");
        if (registros == null || !registros.Any())
            return;

        // Pega todos os tipos de entidades do banco de dados local
        var entityTypes = dbLocal.Model.GetEntityTypes()
            .Select(t => t.ClrType)
            .Where(t => typeof(ISyncable).IsAssignableFrom(t))
            .ToDictionary(t => t.Name, t => t);

        foreach (var registro in registros)
        {
            var payload = registro.Dados;

            // 2. Processa as Atualizações/Inserções
            if (payload.EntidadesAtualizadas != null)
            {
                foreach (var (entityName, elements) in payload.EntidadesAtualizadas)
                {
                    if (entityTypes.TryGetValue(entityName, out var type))
                    {
                        foreach (var element in elements)
                        {
                            if (element is JsonElement jsonEl)
                            {
                                var obj = jsonEl.Deserialize(type, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                if (obj != null)
                                {
                                    // O EF Core detecta se já existe (Update) ou se é novo (Insert)
                                    // Como recebemos a entidade completa, podemos forçar o Update
                                    var entry = dbLocal.Entry(obj);
                                    if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                                    {
                                        dbLocal.Update(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 3. Processa as Exclusões
            if (payload.EntidadesExcluidas != null)
            {
                foreach (var (entityName, idsExcluidos) in payload.EntidadesExcluidas)
                {
                    if (entityTypes.TryGetValue(entityName, out var type))
                    {
                        foreach (var id in idsExcluidos)
                        {
                            var entity = await dbLocal.FindAsync(type, id);
                            if (entity != null)
                            {
                                dbLocal.Remove(entity);
                            }
                        }
                    }
                }
            }

            // Salva as alterações no SQLite local
            await dbLocal.SaveChangesAsync();

            // 4. Remove o registro já processado da fila do IndexedDB
            await indexedDb.DeleteRecord("FilaPull", registro.Id);
        }
    }
}

public class FilaPullRecord
{
    public long Id { get; set; }
    public SyncPayload Dados { get; set; } = new();
}
