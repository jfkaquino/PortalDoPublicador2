using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalDoPublicador.Api.Infrastructure.Data;
using PortalDoPublicador.Api.Infrastructure.Data.Entities;
using PortalDoPublicador.Shared.Infrastructure.Sync;

namespace PortalDoPublicador.Api.Features;

public static class SyncEndpoint
{
    // Fazemos cache de todas as interfaces que terminam com "Service" dentro do projeto Shared.
    // Isso roda apenas 1x quando a aplicação inicia, garantindo O(1) de performance no Endpoint e 100% Type-Safety.
    private static readonly Dictionary<string, Type> _syncableTypes = typeof(SyncRequest).Assembly
        .GetTypes()
        .Where(t => t.IsInterface && t.Name.EndsWith("Service"))
        .ToDictionary(t => t.Name, t => t);

    public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/sync", async (
            [FromBody] SyncRequest requisicao,
            ApiDbContext context,
            IServiceProvider serviceProvider) =>
        {
            var resposta = new SyncResponse();

            // 1. FASE DE PUSH: Processar a fila enviada pelo cliente
            if (requisicao.ComandosPush != null && requisicao.ComandosPush.Count != 0)
            {
                // Pega os IDs dos comandos que já processamos antes para garantir Idempotência
                var idsRecebidos = requisicao.ComandosPush.Select(c => c.Id).ToList();
                var comandosJaProcessados = await context.ComandosProcessados
                    .Where(c => idsRecebidos.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var comando in requisicao.ComandosPush.OrderBy(c => c.Timestamp))
                {
                    // Proteção contra duplicação (Se a rede caiu na ida e o celular mandou de novo)
                    if (comandosJaProcessados.Contains(comando.Id))
                    {
                        resposta.RelatorioPush.Add(new SyncCommandResult { Id = comando.Id, Sucesso = true });
                        continue;
                    }

                    try
                    {
                        if (!_syncableTypes.TryGetValue(comando.Servico, out var tipoServico))
                        {
                            resposta.RelatorioPush.Add(new SyncCommandResult 
                            { 
                                Id = comando.Id, 
                                Sucesso = false, 
                                MensagemErro = $"Serviço {comando.Servico} não encontrado ou não permitido."
                            });
                            continue;
                        }

                        // Busca o método usando nosso invocador ultrarrápido (cacheado)
                        var metodoRapido = FastMethodInvoker.GetInvoker(tipoServico, comando.Metodo);
                        if (metodoRapido == null)
                        {
                            resposta.RelatorioPush.Add(new SyncCommandResult 
                            { 
                                Id = comando.Id, 
                                Sucesso = false, 
                                MensagemErro = $"Método {comando.Metodo} não encontrado no serviço {comando.Servico}."
                            });
                            continue;
                        }

                        var servico = serviceProvider.GetRequiredService(tipoServico);
                        object[] argumentos = null;

                        // Como cacheamos também os tipos dos parâmetros, evitamos GetParameters()
                        var parametroTipo = FastMethodInvoker.GetParameterType(tipoServico, comando.Metodo);
                        if (parametroTipo != null)
                        {
                            var dto = JsonSerializer.Deserialize(comando.Payload, parametroTipo);
                            argumentos = [dto];
                        }

                        // Executa na velocidade da luz (sem MethodInfo.Invoke)
                        var resultado = metodoRapido(servico, argumentos);
                        if (resultado is Task task)
                        {
                            await task;
                        }

                        // Registra o sucesso
                        resposta.RelatorioPush.Add(new SyncCommandResult { Id = comando.Id, Sucesso = true });

                        // Salva o ID no histórico para nunca mais processar de novo
                        context.ComandosProcessados.Add(new ProcessedSyncCommand { Id = comando.Id });
                    }
                    catch (Exception ex)
                    {
                        var erroReal = ex.InnerException ?? ex;
                        resposta.RelatorioPush.Add(new SyncCommandResult
                        {
                            Id = comando.Id,
                            Sucesso = false,
                            MensagemErro = erroReal.Message
                        });
                    }
                }

                try
                {
                    // Salva TUDO: As alterações de negócio E o histórico de comandos processados
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries[0];
                    var dadosAtuaisBanco = await entry.GetDatabaseValuesAsync();
                    
                    // Retorna HTTP 409 (Conflict) informando os dados atualizados
                    return Results.Conflict(new {
                        Mensagem = "Conflito detectado: O registro foi alterado por outro usuário enquanto você estava offline.",
                        DadosAtualizados = dadosAtuaisBanco?.ToObject()
                    });
                }
            }

            // 2. FASE DE PULL: Buscar as novidades (Deltas) dinamicamente
            var dataFiltro = requisicao.UltimaSincronizacao;

            // Pega todos os tipos de entidades mapeadas no EF Core que implementam ISyncable
            var tiposSincronizaveis = context.Model.GetEntityTypes()
                .Select(t => t.ClrType)
                .Where(t => typeof(ISyncable).IsAssignableFrom(t))
                .ToList();

            foreach (var tipo in tiposSincronizaveis)
            {
                // Busca os atualizados e inseridos
                var atualizados = await FastMethodInvoker.BuscarAtualizacoesDinamicamenteAsync(context, tipo, dataFiltro, false);
                if (atualizados.Any())
                {
                    resposta.DadosPull.EntidadesAtualizadas[tipo.Name] = atualizados;
                }

                // Busca os excluídos (soft delete) e retorna apenas os IDs para o cliente deletar localmente
                var excluidos = await FastMethodInvoker.BuscarAtualizacoesDinamicamenteAsync(context, tipo, dataFiltro, true);
                if (excluidos.Any())
                {
                    // Usa reflection para pegar a chave primária (Id)
                    var idsExcluidos = excluidos.Select(e => (Guid)tipo.GetProperty("Id").GetValue(e)).ToList();
                    resposta.DadosPull.EntidadesExcluidas[tipo.Name] = idsExcluidos;
                }
            }

            // 3. Define a nova data oficial baseada no relógio do Servidor
            resposta.NovaDataSincronizacao = DateTime.UtcNow;

            // Retorna HTTP 200 com o JSON
            return Results.Ok(resposta);
        });
    }
}

// Classe Utilitária para compilação nativa de Reflection (Alta Performance)
public static class FastMethodInvoker
{
    private static readonly ConcurrentDictionary<string, Func<object, object[], object>> _invokerCache = new();
    private static readonly ConcurrentDictionary<string, Type> _parameterTypeCache = new();

    public static Func<object, object[], object> GetInvoker(Type tipo, string nomeMetodo)
    {
        string key = $"{tipo.FullName}.{nomeMetodo}";

        return _invokerCache.GetOrAdd(key, _ =>
        {
            var metodo = tipo.GetMethod(nomeMetodo);
            if (metodo == null) return null;

            var parametros = metodo.GetParameters();
            
            // Salva o tipo do primeiro argumento (assumimos max 1 parâmetro pelo payload JSON)
            if (parametros.Length > 0)
            {
                _parameterTypeCache[key] = parametros[0].ParameterType;
            }

            var targetParam = Expression.Parameter(typeof(object), "target");
            var argsParam = Expression.Parameter(typeof(object[]), "args");

            var targetCast = Expression.Convert(targetParam, tipo);
            var argExpressions = new Expression[parametros.Length];

            for (int i = 0; i < parametros.Length; i++)
            {
                var indexExpr = Expression.Constant(i);
                var arrayAccess = Expression.ArrayIndex(argsParam, indexExpr);
                argExpressions[i] = Expression.Convert(arrayAccess, parametros[i].ParameterType);
            }

            var callExpr = Expression.Call(targetCast, metodo, argExpressions);
            Expression returnExpr;

            if (metodo.ReturnType == typeof(void))
            {
                var nullObj = Expression.Constant(null, typeof(object));
                returnExpr = Expression.Block(callExpr, nullObj);
            }
            else
            {
                returnExpr = Expression.Convert(callExpr, typeof(object));
            }

            return Expression.Lambda<Func<object, object[], object>>(returnExpr, targetParam, argsParam).Compile();
        });
    }

    public static Type GetParameterType(Type tipo, string nomeMetodo)
    {
        string key = $"{tipo.FullName}.{nomeMetodo}";
        _parameterTypeCache.TryGetValue(key, out var tipoParam);
        return tipoParam;
    }

    // --- MÉTODOS PARA O PULL DINÂMICO ---
    
    public static async Task<IEnumerable<object>> BuscarAtualizacoesDinamicamenteAsync(DbContext context, Type tipoEntidade, DateTime dataFiltro, bool buscarExcluidos)
    {
        var metodoInfo = typeof(FastMethodInvoker).GetMethod(nameof(BuscarAtualizacoesGenAsync), BindingFlags.NonPublic | BindingFlags.Static);
        var metodoGenerico = metodoInfo.MakeGenericMethod(tipoEntidade);
        
        var task = (Task<IEnumerable<object>>)metodoGenerico.Invoke(null, [context, dataFiltro, buscarExcluidos]);
        return await task;
    }

    private static async Task<IEnumerable<object>> BuscarAtualizacoesGenAsync<TEntity>(DbContext context, DateTime dataFiltro, bool buscarExcluidos) where TEntity : class, ISyncable
    {
        return await context.Set<TEntity>()
            .AsNoTracking()
            .Where(x => x.DataModificacao > dataFiltro && x.Excluido == buscarExcluidos)
            .Cast<object>()
            .ToListAsync();
    }
}