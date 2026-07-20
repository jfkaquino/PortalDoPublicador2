namespace PortalDoPublicador.Shared.Features.Perfis;

public class Usuario
{
    public Guid Id { get; set; }
    public required string NomeCompleto { get; set; }
    public required string NomeExibicao { get; set; }
    public DateTime DataNascimento { get; set; }
    public bool IsMasculino { get; set; } = true;
    
    public Perfil? Perfil { get; set; }
}
