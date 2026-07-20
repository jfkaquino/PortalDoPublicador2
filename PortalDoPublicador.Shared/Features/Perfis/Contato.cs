namespace PortalDoPublicador.Shared.Features.Perfis;

public class Contato
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required string Endereco { get; set; }
    
    public required Perfil Perfil { get; set; }
}
