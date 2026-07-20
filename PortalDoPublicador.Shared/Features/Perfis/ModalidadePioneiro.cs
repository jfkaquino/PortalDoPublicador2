using System.ComponentModel;

namespace PortalDoPublicador.Shared.Features.Perfis;

public enum ModalidadePioneiro
{
    [Description("Pioneiro Auxiliar (15 Horas)")]
    Auxiliar15horas,
    
    [Description("Pioneiro Auxiliar (30 Horas)")]
    Auxiliar30horas,
    
    [Description("Pioneiro Regular")]
    Regular,
    
    [Description("Pioneiro Especial")]
    Especial
}
