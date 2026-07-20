using Microsoft.EntityFrameworkCore;
using PortalDoPublicador.Shared.Features.Designacoes;
using PortalDoPublicador.Shared.Features.Eventos;
using PortalDoPublicador.Shared.Features.Perfis;

namespace PortalDoPublicador.Shared.Infrastructure.Data;

public class SharedDbContext(DbContextOptions options) : DbContext(options)
{
    // Pessoas
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Perfil> Perfis { get; set; }
    public DbSet<Familia> Familias { get; set; }
    public DbSet<Contato> Contatos { get; set; }
    public DbSet<Pioneiro> Pioneiros { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<Responsabilidade> Responsabilidades { get; set; }
    public DbSet<Relatorio> Relatorios { get; set; }

    // Eventos (TPH)
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<ReuniaoMeioDeSemana> ReunioesMeioDeSemana { get; set; }
    public DbSet<ReuniaoFimDeSemana> ReunioesFimDeSemana { get; set; }
    public DbSet<SaidaServicoCampo> SaidasServicoCampo { get; set; }
    
    public DbSet<Parte> Partes { get; set; }
    public DbSet<SecaoReuniao> SecoesReuniao { get; set; }
    public DbSet<AssistenciaReuniao> Assistencias { get; set; }

    // Designações (TPH)
    public DbSet<Designacao> Designacoes { get; set; }
    public DbSet<DesignacaoMecanica> DesignacoesMecanicas { get; set; }
    public DbSet<DesignacaoParte> DesignacoesPartes { get; set; }
    public DbSet<DesignacaoServicoCampo> DesignacoesServicoCampo { get; set; }
    public DbSet<DesignacaoLimpeza> DesignacoesLimpeza { get; set; }
    public DbSet<DesignacaoManutencao> DesignacoesManutencao { get; set; }
    
    public DbSet<TipoDesignacao> TiposDesignacao { get; set; }
    public DbSet<PerfilDesignacao> PerfisDesignacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configurar Chave Composta da Tabela Associativa via Shadow Properties
        modelBuilder.Entity<PerfilDesignacao>()
            .HasKey("PerfilId", "DesignacaoId");

        // 2. Configurar Relação 1:1 entre Reuniao e Assistencia
        modelBuilder.Entity<Reuniao>()
            .HasOne(r => r.Assistencia)
            .WithOne(a => a.Reuniao)
            .HasForeignKey<AssistenciaReuniao>("ReuniaoId");

        // 3. Configurar Relação 1:1 Usuario -> Perfil
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Perfil)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Perfil>("UsuarioId");

        // 4. Configurar Relação 1:1 Perfil -> Contato
        modelBuilder.Entity<Perfil>()
            .HasOne(p => p.Contato)
            .WithOne(c => c.Perfil)
            .HasForeignKey<Contato>("PerfilId");

        // 5. Garantir que as heranças TPH sejam mapeadas claramente
        modelBuilder.Entity<Evento>()
            .HasDiscriminator<string>("TipoEvento")
            .HasValue<ReuniaoMeioDeSemana>("MeioDeSemana")
            .HasValue<ReuniaoFimDeSemana>("FimDeSemana")
            .HasValue<SaidaServicoCampo>("ServicoCampo");

        modelBuilder.Entity<Designacao>()
            .HasDiscriminator<string>("TipoCategoria")
            .HasValue<DesignacaoMecanica>("Mecanica")
            .HasValue<DesignacaoParte>("Parte")
            .HasValue<DesignacaoServicoCampo>("Campo")
            .HasValue<DesignacaoLimpeza>("Limpeza")
            .HasValue<DesignacaoManutencao>("Manutencao");
    }
}
