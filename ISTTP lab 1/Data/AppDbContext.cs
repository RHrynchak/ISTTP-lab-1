using System;
using System.Collections.Generic;
using ISTTP_lab_1.Models;
using Microsoft.EntityFrameworkCore;

namespace ISTTP_lab_1.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cpu> Cpus { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Gpu> Gpus { get; set; }

    public virtual DbSet<PcConfig> PcConfigs { get; set; }

    public virtual DbSet<Requirement> Requirements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<OsEnum>("os_enum")
            .HasPostgresEnum<RequirementType>("requirement_type");

        modelBuilder.Entity<Cpu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cpus_pkey");

            entity.ToTable("cpus");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BenchmarkScore).HasColumnName("benchmark_score");
            entity.Property(e => e.CoresNumber).HasColumnName("cores_number");
            entity.Property(e => e.ModelName).HasColumnName("model_name");

            entity.HasIndex(e => e.ModelName).IsUnique();
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("games_pkey");

            entity.ToTable("games");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.SizeGb).HasColumnName("size_gb");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasIndex(e => e.Title).IsUnique();
        });

        modelBuilder.Entity<Gpu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gpus_pkey");

            entity.ToTable("gpus");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BenchmarkScore).HasColumnName("benchmark_score");
            entity.Property(e => e.ModelName).HasColumnName("model_name");
            entity.Property(e => e.VramGb).HasColumnName("vram_gb");

            entity.HasIndex(e => e.ModelName).IsUnique();
        });

        modelBuilder.Entity<PcConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pc_configs_pkey");

            entity.ToTable("pc_configs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CpuId).HasColumnName("cpu_id");
            entity.Property(e => e.GpuId).HasColumnName("gpu_id");
            entity.Property(e => e.RamGb).HasColumnName("ram_gb");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Os).HasColumnName("os");

            entity.HasOne(d => d.Cpu).WithMany(p => p.PcConfigs)
                .HasForeignKey(d => d.CpuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cpu_id");

            entity.HasOne(d => d.Gpu).WithMany(p => p.PcConfigs)
                .HasForeignKey(d => d.GpuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gpu_id");

            entity.HasOne(d => d.User).WithMany(p => p.PcConfigs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_id");
        });

        modelBuilder.Entity<Requirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("requirements_pkey");

            entity.ToTable("requirements");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CpuCores).HasColumnName("cpu_cores");
            entity.Property(e => e.CpuId).HasColumnName("cpu_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.GpuId).HasColumnName("gpu_id");
            entity.Property(e => e.OSes).HasColumnName("OSes").HasColumnType("os_enum[]");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.RamGb).HasColumnName("ram_gb");
            entity.Property(e => e.VramGb).HasColumnName("vram_gb");

            entity.HasOne(d => d.Cpu).WithMany(p => p.Requirements)
                .HasForeignKey(d => d.CpuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cpu_id");

            entity.HasOne(d => d.Game).WithMany(p => p.Requirements)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_id");

            entity.HasOne(d => d.Gpu).WithMany(p => p.Requirements)
                .HasForeignKey(d => d.GpuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gpu_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('user_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasDefaultValueSql("'User'::character varying");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
