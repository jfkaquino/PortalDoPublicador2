using PortalDoPublicador.Api.Data.Entities;

namespace PortalDoPublicador.ServiceDefaults.Entities;

public class DesignacaoMecanica
{
    public int Id { get; set; }
    public required TipoDesignacaoMecanica TipoDesignacaoMecanica { get; set; }
    public required Usuario Usuario { get; set; }
    public required Evento Evento { get; set; }
}
