namespace PortalDoPublicador.Shared.Features.Perfis;

public class Pioneiro
{
    public Guid Id { get; set; }
    
    public required Perfil Perfil { get; set; }
    
    public ModalidadePioneiro ModalidadePioneiro { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
