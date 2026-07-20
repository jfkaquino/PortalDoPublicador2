using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.JSInterop;
using PortalDoPublicador.Shared.Infrastructure.Data;
using PortalDoPublicador.Shared.Infrastructure.Sync;
using Blazor.IndexedDB;

namespace PortalDoPublicador.Client.Infrastructure.Sync;

public class OfflineSyncWrapper(IServiceProvider serviceProvider, SharedDbContext dbLocal, IndexedDBManager indexedDb, IJSRuntime jsRuntime)
{
    public async Task ExecutarAsync<TService>(Expression<Func<TService, Task>> expressao) where TService : notnull
    {
        var servicoLocal = serviceProvider.GetRequiredService<TService>();
        var funcaoCompilada = expressao.Compile();
        await funcaoCompilada(servicoLocal);

        if (expressao.Body is MethodCallExpression methodCall)
        {
            var nomeServico = typeof(TService).Name;
            var nomeMetodo = methodCall.Method.Name;

            var argumentosAtuais = methodCall.Arguments
                .Select(arg => Expression.Lambda(arg).Compile().DynamicInvoke())
                .ToArray();

            var payloadObj = argumentosAtuais.Length == 1
                ? argumentosAtuais[0]
                : (argumentosAtuais.Length > 1 ? argumentosAtuais : null);

            var comando = new ComandoSync
            {
                Id = Guid.NewGuid(),
                Servico = nomeServico,
                Metodo = nomeMetodo,
                Payload = payloadObj != null ? JsonSerializer.Serialize(payloadObj) : string.Empty
            };

            try
            {
                var record = new StoreRecord<ComandoSync>
                {
                    Storename = "FilaComandos",
                    Data = comando
                };
                await indexedDb.AddRecord(record);
                await dbLocal.SaveChangesAsync();
                await jsRuntime.InvokeVoidAsync("SincronizacaoOffline.registrarSync");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar offline: {ex.Message}");
                throw;
            }
        }
    }
}