using API_SSO.Modelos;
using API_SSO.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.Intrinsics.X86;

namespace API_SSO.Context
{
    public partial class SSOContext : IdentityDbContext
    {
        public SSOContext() { }

        public SSOContext(DbContextOptions<SSOContext> options)
        : base(options)
        {
        }

        public DbSet<Invitacion> Invitacions => Set<Invitacion>();

        public virtual DbSet<CatalogoClaim> CatalogoClaims { get; set; }

        public virtual DbSet<CatalogoMenu> CatalogoMenus { get; set; }

        public virtual DbSet<CatalogoSeccion> CatalogoSeccions { get; set; }
        public virtual DbSet<Cliente> Clientes { get; set; }

        public virtual DbSet<ComprobantePago> ComprobantePagos { get; set; }

        public virtual DbSet<Empresa> Empresas { get; set; }

        public virtual DbSet<EmpresaXcliente> EmpresaXclientes { get; set; }

        public virtual DbSet<Log> Logs { get; set; }

        public virtual DbSet<ProyectoActual> ProyectoActuals { get; set; }

        public virtual DbSet<Rol> Rols { get; set; }

        public virtual DbSet<UsuarioXempresa> UsuarioXempresas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseCollation("Modern_Spanish_CI_AS");

            modelBuilder.Entity<Invitacion>(b =>
            {
                b.ToTable("Invitacion");
                b.HasKey(x => x.Id);
                b.Property(x => x.Email).IsRequired().HasMaxLength(320);
                b.Property(x => x.TokenJti).IsRequired().HasMaxLength(64);
                // (opcionales) índices útiles:
                b.HasIndex(x => x.Email);
                b.HasIndex(x => x.TokenJti).IsUnique();
            });

            modelBuilder.Entity<CatalogoClaim>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Catalogo__3214EC07AB2A5E13");

                entity.Property(e => e.CodigoClaim).HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(150);
                entity.Property(e => e.Nombre).HasMaxLength(50);

                entity.HasOne(d => d.IdSeccionNavigation).WithMany(p => p.CatalogoClaims)
                    .HasForeignKey(d => d.IdSeccion)
                    .HasConstraintName("FK__CatalogoC__IdSec__2180FB33");
            });

            modelBuilder.Entity<CatalogoMenu>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Catalogo__3214EC072E6CE9B6");

                entity.ToTable("CatalogoMenu");

                entity.Property(e => e.Nombre).HasMaxLength(100);
            });

            modelBuilder.Entity<CatalogoSeccion>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Catalogo__3214EC0732AE614E");

                entity.ToTable("CatalogoSeccion");

                entity.Property(e => e.Descripcion).HasMaxLength(150);
                entity.Property(e => e.Nombre).HasMaxLength(50);

                entity.HasOne(d => d.IdMenuNavigation).WithMany(p => p.CatalogoSeccions)
                    .HasForeignKey(d => d.IdMenu)
                    .HasConstraintName("FK__CatalogoS__IdMen__339FAB6E");
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Cliente__3214EC076D9B1004");

                entity.ToTable("Cliente");

                entity.Property(e => e.CantidadUsuariosXempresa).HasColumnName("CantidadUsuariosXEmpresa");
                entity.Property(e => e.Correo).HasMaxLength(250);
                entity.Property(e => e.CostoXusuario)
                    .HasColumnType("decimal(28, 6)")
                    .HasColumnName("CostoXUsuario");
                entity.Property(e => e.Estatus).HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.PagoXempresa).HasColumnName("PagoXEmpresa");
                entity.Property(e => e.RazonSocial).HasMaxLength(150);
            });

            modelBuilder.Entity<ComprobantePago>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Comproba__3214EC07B303B862");

                entity.ToTable("ComprobantePago");

                entity.Property(e => e.FechaCarga).HasColumnType("datetime");
                entity.Property(e => e.IdUsuarioAutorizador).HasMaxLength(450);
                entity.Property(e => e.Ruta).HasMaxLength(250);
                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ComprobantePagos)
                    .HasForeignKey(d => d.IdCliente)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ComprobantePago_Cliente");

                entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.ComprobantePagos)
                    .HasForeignKey(d => d.IdEmpresa)
                    .HasConstraintName("Fk_ComprobantePagoEmpresa");
            });

            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Empresa__3214EC07F404C53A");

                entity.ToTable("Empresa");

                entity.Property(e => e.CodigoPostal).HasMaxLength(10);
                entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
                entity.Property(e => e.NombreComercial).HasMaxLength(200);
                entity.Property(e => e.Rfc).HasMaxLength(13);
                entity.Property(e => e.Sociedad)
                    .HasMaxLength(250)
                    .HasDefaultValue("");
            });

            modelBuilder.Entity<EmpresaXcliente>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__EmpresaX__3214EC07C02E4CB7");

                entity.ToTable("EmpresaXCliente");

                entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.EmpresaXclientes)
                    .HasForeignKey(d => d.IdCliente)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmpresaXCliente_Cliente");

                entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.EmpresaXclientes)
                    .HasForeignKey(d => d.IdEmpresa)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmpresaXCliente_Empresa");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Logs__3214EC074E498C08");

                entity.Property(e => e.EsSso).HasColumnName("EsSSO");
                entity.Property(e => e.Fecha).HasColumnType("datetime");
                entity.Property(e => e.Metodo).HasMaxLength(150);
                entity.Property(e => e.Nivel).HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Logs)
                    .HasForeignKey(d => d.IdEmpresa)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Logs_Empresa");
            });

            modelBuilder.Entity<ProyectoActual>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Proyecto__3214EC07CC15C45C");

                entity.ToTable("ProyectoActual");

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.ProyectoActuals)
                    .HasForeignKey(d => d.IdEmpresa)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProyectoActual_Empresa");
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Rol__3214EC076578E92A");

                entity.ToTable("Rol");

                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.Color).HasMaxLength(10);
                entity.Property(e => e.DeSistema).HasDefaultValue(false);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
                entity.Property(e => e.General).HasDefaultValue(false);
                entity.Property(e => e.IdAspNetRole).HasMaxLength(450);

                entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Rols)
                    .HasForeignKey(d => d.IdEmpresa)
                    .HasConstraintName("FK__Rol__IdEmpresa__02FC7413");
            });

            modelBuilder.Entity<UsuarioXempresa>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__UsuarioX__3214EC07780FFE8A");

                entity.ToTable("UsuarioXEmpresa");

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.UsuarioXempresas)
                    .HasForeignKey(d => d.IdEmpresa)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsuarioXEmpresa_Empresa");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
