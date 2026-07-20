namespace PortalDoPublicador.Shared.Infrastructure.Sync;

public class SyncRequest
{
    public DateTime UltimaSincronizacao { get; set; }
    public List<ComandoSync> ComandosPush { get; set; } = [];
}

public class SyncResponse
{
    public List<SyncCommandResult> RelatorioPush { get; set; } = new();
    public SyncPayload DadosPull { get; set; } = new();
    public DateTime NovaDataSincronizacao { get; set; }
}

public class SyncCommandResult
{
    public Guid Id { get; set; }
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
}

public class SyncPayload
{
    public Dictionary<string, IEnumerable<object>> EntidadesAtualizadas { get; set; } = new();
    public Dictionary<string, List<Guid>> EntidadesExcluidas { get; set; } = new();
}
