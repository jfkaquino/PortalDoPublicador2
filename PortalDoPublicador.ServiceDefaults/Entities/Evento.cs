namespace PortalDoPublicador.ServiceDefaults.Entities;

public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTimeOffset DataHoraInicio { get; set; }
    public DateTimeOffset DataHoraFim { get; set; }
}
