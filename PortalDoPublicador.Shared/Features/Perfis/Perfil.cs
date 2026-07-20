using System.ComponentModel.DataAnnotations.Schema;
using PortalDoPublicador.Shared.Enums;
using PortalDoPublicador.Shared.Features.Designacoes;

namespace PortalDoPublicador.Shared.Features.Perfis;

public class Perfil
{
    public Guid Id { get; set; }
    
    public required Usuario Usuario { get; set; }
    
    public SituacaoEspiritual SituacaoEspiritual { get; set; }
    public DateTime? DataBatismo { get; set; }
    
    public Familia? Familia { get; set; }
    
    [InverseProperty("Membros")]
    public Grupo? Grupo { get; set; }
    
    public Contato? Contato { get; set; }
    
    public List<Pioneiro> HistoricoPioneiro { get; set; } = [];
    
    public List<TipoDesignacao> Permissoes { get; set; } = [];

    public List<PerfilDesignacao> DesignacoesAtribuidas { get; set; } = [];
    
    public List<Relatorio> Relatorios { get; set; } = [];

    [InverseProperty("Titular")]
    public List<Responsabilidade> ResponsabilidadesComoTitular { get; set; } = [];
    
    [InverseProperty("Ajudantes")]
    public List<Responsabilidade> ResponsabilidadesComoAjudante { get; set; } = [];
}
