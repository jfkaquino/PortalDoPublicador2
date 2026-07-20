using System.ComponentModel.DataAnnotations.Schema;

namespace PortalDoPublicador.Shared.Features.Perfis;

public class Grupo
{
    public Guid Id { get; set; }
    public required string Nome { get; set; }
    
    [ForeignKey("SuperintendenteId")]
    public Perfil? Superintendente { get; set; }
    
    [ForeignKey("AjudanteId")]
    public Perfil? Ajudante { get; set; }
    
    public List<Perfil> Membros { get; set; } = [];
}
