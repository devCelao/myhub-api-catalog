using CatalogDomain.Entities;
using DomainObjects.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MicroserviceCore.Extensions;

namespace CatalogInfrastructure.Context;

public class CatalogContext : DbContext, IUnitOfWork
{
    public CatalogContext(DbContextOptions<CatalogContext> options,
                          IOptions<ConnectionSettings> connectionOptions) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
        Schema = connectionOptions.Value.Schema;
    }
    private string Schema { get; set; }
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<PlanoServico> PlanoServicos => Set<PlanoServico>();
    public DbSet<Funcao> Funcoes => Set<Funcao>();

    public async Task<bool> Commit() => await base.SaveChangesAsync() > 0;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);

        // PLANO
        modelBuilder.Entity<Plano>(e =>
        {
            e.ToTable(name: "Plano");
            e.HasKey(x => x.CodPlano);
            e.Property(x => x.NomePlano).IsRequired();
            e.Property(x => x.IndAtivo).IsRequired();
            e.Property(x => x.IndGeraCobranca).IsRequired();
            e.Property(x => x.ValorBase).IsRequired();
            // Relationships N:N através de PlanoServicos
            e.HasMany(x => x.PlanoServicos)
                .WithOne(ps => ps.Plano)
                .HasForeignKey(ps => ps.CodPlano)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            e.HasIndex(x => x.NomePlano).IsUnique();
        });

        // SERVICO
        modelBuilder.Entity<Servico>(e =>
        {
            e.ToTable(name: "Servico");
            e.HasKey(x => x.CodServico);
            e.Property(x => x.NomeServico).IsRequired();
            e.Property(x => x.Descricao).IsRequired(false);

            // Relationships N:N através de PlanoServicos
            e.HasMany(x => x.PlanoServicos)
                .WithOne(ps => ps.Servico)
                .HasForeignKey(ps => ps.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship 1:N com Funcoes
            e.HasMany(x => x.Funcoes)
                .WithOne(f => f.Servico)
                .HasForeignKey(f => f.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            e.HasIndex(x => x.NomeServico).IsUnique();
        });

        // PLANO_SERVICO - Tabela de junção N:N
        modelBuilder.Entity<PlanoServico>(e => {
            e.ToTable("PlanoServico");
            e.HasKey(x => new { x.CodPlano, x.CodServico });
            e.Property(x => x.CodPlano).IsRequired();
            e.Property(x => x.CodServico).IsRequired();

            // Relationships
            e.HasOne(x => x.Plano)
                .WithMany(p => p.PlanoServicos)
                .HasForeignKey(x => x.CodPlano)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Servico)
                .WithMany(s => s.PlanoServicos)
                .HasForeignKey(x => x.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            e.HasIndex(x => new { x.CodPlano, x.CodServico }).IsUnique();
        });

        // FUNCAO
        modelBuilder.Entity<Funcao>(e =>
        {
            e.ToTable(name: "Funcao");
            e.HasKey(x => x.CodFuncao);
            e.Property(x => x.CodFuncao).IsRequired();
            e.Property(x => x.CodServico).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.Descricao).IsRequired(false);
            e.Property(x => x.Icone).IsRequired(false);
            e.Property(x => x.NumOrdem).IsRequired();
            e.Property(x => x.IndAtivo).IsRequired();

            // Relationship com Servico
            e.HasOne(x => x.Servico)
                .WithMany(s => s.Funcoes)
                .HasForeignKey(x => x.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            e.HasIndex(x => x.CodServico);
            e.HasIndex(x => x.NumOrdem);
            e.HasIndex(x => new { x.CodServico, x.Label }).IsUnique();
        });
    }
}
