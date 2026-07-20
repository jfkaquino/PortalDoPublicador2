namespace PortalDoPublicador.Shared.Features.Perfis;

public class Relatorio
{
    public Guid Id { get; set; }
    
    public required Perfil Perfil { get; set; }

    public DateOnly MesReferencia { get; set; }

    public bool ParticipouNoMinisterio { get; set; }
    public int EstudosBiblicos { get; set; }
    
    public int? Horas { get; set; }
    
    public string? Observacoes { get; set; }
}
