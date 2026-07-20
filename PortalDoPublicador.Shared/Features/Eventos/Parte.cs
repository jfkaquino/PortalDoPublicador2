using PortalDoPublicador.Shared.Features.Designacoes;

namespace PortalDoPublicador.Shared.Features.Eventos;

public class Parte
{
    public Guid Id { get; set; }
    
    public required Evento Evento { get; set; }

    public required SecaoReuniao SecaoReuniao { get; set; }

    public required string Tema { get; set; }
    public int TempoMinutos { get; set; }
    public string? FonteMateria { get; set; } 
    
    public List<DesignacaoParte> Designacoes { get; set; } = [];
}
