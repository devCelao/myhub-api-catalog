using System.Linq.Expressions;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
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

    public async Task<bool> Commit()
    {
        try
        {
            await base.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                entityType.SetQueryFilter(filter);
            }
        }

        // PLANO
        modelBuilder.Entity<Plano>(e =>
        {
            e.ToTable(name: "Plano");

            e.Property(x => x.CodPlano)
                .HasConversion(
                    vo => vo.Valor,
                    valor => new CodigoPlano(valor)
                )
                .HasColumnName("CodPlano")
                .HasMaxLength(20)
                .IsRequired();

            e.HasKey(x => x.CodPlano);

            e.OwnsOne(x => x.ValorBase, dinheiro =>
            {
                dinheiro.Property(d => d.Valor)
                    .HasColumnName("ValorBase")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                dinheiro.Property(d => d.Moeda)
                    .HasColumnName("Moeda")
                    .HasMaxLength(3)
                    .HasDefaultValue("BRL")
                    .IsRequired();
            });

            e.Property(x => x.NomePlano)
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.IndAtivo).IsRequired();
            e.Property(x => x.IndGeraCobranca).IsRequired();

            e.Property(x => x.DataCriacao).IsRequired();
            e.Property(x => x.CriadoPor).HasMaxLength(100);
            e.Property(x => x.DataAtualizacao);
            e.Property(x => x.AtualizadoPor).HasMaxLength(100);
            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.DataExclusao);
            e.Property(x => x.ExcluidoPor).HasMaxLength(100);

            e.HasMany(x => x.PlanoServicos)
                .WithOne(ps => ps.Plano)
                .HasForeignKey(ps => ps.CodPlano)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.NomePlano).IsUnique();
            e.HasIndex(x => x.IsDeleted);
        });

        // SERVICO
        modelBuilder.Entity<Servico>(e =>
        {
            e.ToTable(name: "Servico");
            e.HasKey(x => x.CodServico);
            e.Property(x => x.NomeServico).IsRequired();
            e.Property(x => x.Descricao).IsRequired(false);

            e.Property(x => x.DataCriacao).IsRequired();
            e.Property(x => x.CriadoPor).HasMaxLength(100);
            e.Property(x => x.DataAtualizacao);
            e.Property(x => x.AtualizadoPor).HasMaxLength(100);
            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.DataExclusao);
            e.Property(x => x.ExcluidoPor).HasMaxLength(100);

            e.HasMany(x => x.PlanoServicos)
                .WithOne(ps => ps.Servico)
                .HasForeignKey(ps => ps.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Funcoes)
                .WithOne(f => f.Servico)
                .HasForeignKey(f => f.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.NomeServico).IsUnique();
            e.HasIndex(x => x.IsDeleted);
        });

        // PLANO_SERVICO
        modelBuilder.Entity<PlanoServico>(e =>
        {
            e.ToTable("PlanoServico");

            e.Property(x => x.CodPlano)
                .HasConversion(
                    vo => vo.Valor,
                    valor => new CodigoPlano(valor)
                )
                .HasColumnName("CodPlano")
                .HasMaxLength(20)
                .IsRequired();

            e.Property(x => x.CodServico).IsRequired();

            e.HasKey(x => new { x.CodPlano, x.CodServico });

            e.HasOne(x => x.Plano)
                .WithMany(p => p.PlanoServicos)
                .HasForeignKey(x => x.CodPlano)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Servico)
                .WithMany(s => s.PlanoServicos)
                .HasForeignKey(x => x.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

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

            e.Property(x => x.DataCriacao).IsRequired();
            e.Property(x => x.CriadoPor).HasMaxLength(100);
            e.Property(x => x.DataAtualizacao);
            e.Property(x => x.AtualizadoPor).HasMaxLength(100);
            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.DataExclusao);
            e.Property(x => x.ExcluidoPor).HasMaxLength(100);

            e.HasOne(x => x.Servico)
                .WithMany(s => s.Funcoes)
                .HasForeignKey(x => x.CodServico)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.CodServico);
            e.HasIndex(x => x.NumOrdem);
            e.HasIndex(x => new { x.CodServico, x.Label }).IsUnique();
            e.HasIndex(x => x.IsDeleted);
        });
    }
}
