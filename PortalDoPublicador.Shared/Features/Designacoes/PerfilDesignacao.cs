using PortalDoPublicador.Shared.Features.Perfis;

namespace PortalDoPublicador.Shared.Features.Designacoes;

public class PerfilDesignacao
{
    public required Perfil Perfil { get; set; }
    public required Designacao Designacao { get; set; }

    public bool? Confirmado { get; set; }
    public DateTime DataAtribuicao { get; set; } = DateTime.UtcNow;
}
