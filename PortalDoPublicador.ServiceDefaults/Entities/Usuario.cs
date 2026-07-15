using PortalDoPublicador.Shared.Enums;

namespace PortalDoPublicador.Api.Data.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataBatismo { get; set; }
    public EnumPrivilegio Privilegio { get; set; }
    public string? IdentityUserId { get; set; }
}
