namespace PortalDoPublicador.Shared.Features.Designacoes;

public abstract class Designacao
{
    public Guid Id { get; set; }
    public List<PerfilDesignacao> PerfisDesignados { get; set; } = [];
}
