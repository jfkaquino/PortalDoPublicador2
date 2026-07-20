namespace PortalDoPublicador.Shared.Infrastructure.Sync;

public interface ISyncable
{
    DateTime DataModificacao { get; set; }
    bool Excluido { get; set; }
}
