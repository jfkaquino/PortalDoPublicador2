using System.ComponentModel;

namespace PortalDoPublicador.Shared.Enums;

public enum SituacaoEspiritual
{
    [Description("Nenhum")]
    Nenhum,
    
    [Description("Publicador Não Batizado")]
    PublicadorNaoBatizado,
    
    [Description("Publicador Batizado")]
    Publicador,
    
    [Description("Servo Ministerial")]
    ServoMinisterial,
    
    [Description("Ancião")]
    Anciao,
    
    [Description("Removido")]
    Removido
}
