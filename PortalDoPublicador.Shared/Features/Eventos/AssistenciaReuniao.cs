namespace PortalDoPublicador.Shared.Features.Eventos;

public class AssistenciaReuniao
{
    public Guid Id { get; set; }
    
    public required Reuniao Reuniao { get; set; }

    public int QuantidadePresentes { get; set; }
    public int QuantidadeConectados { get; set; }
    
    public int TotalAssistencia => QuantidadePresentes + QuantidadeConectados;
}
