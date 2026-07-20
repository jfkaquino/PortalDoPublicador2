using PortalDoPublicador.Shared.Features.Eventos;
using PortalDoPublicador.Shared.Features.Perfis;

namespace PortalDoPublicador.Shared.Features.Designacoes;

public class DesignacaoMecanica : Designacao
{
    public required Evento Evento { get; set; }
    public required TipoDesignacao TipoDesignacao { get; set; }
}

public class DesignacaoParte : Designacao
{
    public required Parte Parte { get; set; }
    public required TipoDesignacao TipoDesignacao { get; set; }
}

public class DesignacaoServicoCampo : Designacao
{
    public required Evento Evento { get; set; }
    public required TipoDesignacao TipoDesignacao { get; set; }
}

public class DesignacaoLimpeza : Designacao
{
    public required Evento Evento { get; set; }
    public required TipoDesignacao TipoDesignacao { get; set; }
    public required Grupo Grupo { get; set; }
}

public class DesignacaoManutencao : Designacao
{
    public required Evento Evento { get; set; }
    public required TipoDesignacao TipoDesignacao { get; set; }
    public required Grupo Grupo { get; set; }
}
