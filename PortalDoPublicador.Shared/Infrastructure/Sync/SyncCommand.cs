namespace PortalDoPublicador.Shared.Infrastructure.Sync;

public class ComandoSync
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Servico { get; set; } = string.Empty;
    public string Metodo { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public StatusComandoSync Status { get; set; } = StatusComandoSync.Pendente;
    public string? Mensagem { get; set; }
}

public enum StatusComandoSync
{
    Pendente,
    Processando,
    Concluido,
    Conflito,
    Erro
}
