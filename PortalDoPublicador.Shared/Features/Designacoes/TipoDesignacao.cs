namespace PortalDoPublicador.Shared.Features.Designacoes;

public class TipoDesignacao
{
    public Guid Id { get; set; }
    public required string Nome { get; set; }
    public CategoriaDesignacao Categoria { get; set; }
    public bool Ativo { get; set; } = true;
}
