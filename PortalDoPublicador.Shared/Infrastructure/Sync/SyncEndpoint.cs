namespace PortalDoPublicador.Shared.Infrastructure.Sync;

public class SyncRequest
{
    public DateTime UltimaSincronizacao { get; set; }
    public List<SyncPayload> DadosPush { get; set; } = [];
}

public class SyncResponse
{
    public List<SyncResult> SyncResults { get; set; } = [];
    public List<SyncPayload> DadosPull { get; set; } = [];
    public DateTime NovaDataSincronizacao { get; set; }
}

public class SyncResult
{
    public Guid PayloadId { get; set; }
    public bool Sucesso { get; set; }
    public string? MensagemErro { get; set; }
}
