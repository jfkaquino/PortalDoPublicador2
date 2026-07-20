using PortalDoPublicador.Shared.Features.Designacoes;

namespace PortalDoPublicador.Shared.Features.Eventos;

public abstract class Reuniao : Evento
{
    public AssistenciaReuniao? Assistencia { get; set; }
    public List<DesignacaoMecanica> DesignacoesMecanicas { get; set; } = [];
    public List<Parte> Partes { get; set; } = [];
}

public class ReuniaoMeioDeSemana : Reuniao { }

public class ReuniaoFimDeSemana : Reuniao { }

public class SaidaServicoCampo : Evento
{
    public required string PontoDeEncontro { get; set; }
    public required string TerritorioAlocado { get; set; }
    
    public List<DesignacaoServicoCampo> Designacoes { get; set; } = [];
}
