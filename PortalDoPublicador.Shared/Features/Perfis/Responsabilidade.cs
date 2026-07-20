using System.ComponentModel.DataAnnotations.Schema;

namespace PortalDoPublicador.Shared.Features.Perfis;

public class Responsabilidade
{
    public Guid Id { get; set; }
    public required string Nome { get; set; } 
    public string? Descricao { get; set; } 
    
    [InverseProperty("ResponsabilidadesComoTitular")]
    public Perfil? Titular { get; set; }

    [InverseProperty("ResponsabilidadesComoAjudante")]
    public List<Perfil> Ajudantes { get; set; } = [];
}
