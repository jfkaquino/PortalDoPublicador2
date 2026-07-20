namespace PortalDoPublicador.Shared.Features.Eventos;

public abstract class Evento
{
    public Guid Id { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Notas { get; set; }
}
