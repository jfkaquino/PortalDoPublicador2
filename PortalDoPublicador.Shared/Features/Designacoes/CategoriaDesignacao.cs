using System.ComponentModel;

namespace PortalDoPublicador.Shared.Features.Designacoes;

public enum CategoriaDesignacao
{
    [Description("Mecânica")]
    Mecanica,
    
    [Description("Parte")]
    Parte,
    
    [Description("Pregação")]
    Pregacao,
    
    [Description("Testemunho Público")]
    TestemunhoPublico,
    
    [Description("Limpeza")]
    Limpeza,
    
    [Description("Manutenção")]
    Manutencao
}
