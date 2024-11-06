using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Blockchain_Supply_Chain_Tracking_System.Models;

public partial class SupplyTrackingContext : DbContext
{
    public SupplyTrackingContext()
    {
    }

    public SupplyTrackingContext(DbContextOptions<SupplyTrackingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blockchainblock> Blockchainblocks { get; set; }

    public virtual DbSet<Blockchainid> Blockchainids { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Transporter> Transporters { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    public virtual DbSet<Vendorproduct> Vendorproducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=SupplyTracking;Username=postgres;Password=1721");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blockchainblock>(entity =>
        {
            entity.HasKey(e => e.Blockid).HasName("blockchainblocks_pkey");

            entity.ToTable("blockchainblocks");

            entity.Property(e => e.Blockid).HasColumnName("blockid");
            entity.Property(e => e.Batchid)
                .HasMaxLength(24)
                .HasColumnName("batchid");
            entity.Property(e => e.Blockchainid)
                .HasMaxLength(24)
                .HasColumnName("blockchainid");
            entity.Property(e => e.Carnumber)
                .HasMaxLength(20)
                .HasColumnName("carnumber");
            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Eventdetails)
                .HasMaxLength(255)
                .HasColumnName("eventdetails");
            entity.Property(e => e.Eventtype)
                .HasMaxLength(55)
                .HasColumnName("eventtype");
            entity.Property(e => e.Hash)
                .HasMaxLength(255)
                .HasColumnName("hash");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Nonce).HasColumnName("nonce");
            entity.Property(e => e.Previoushash)
                .HasMaxLength(255)
                .HasColumnName("previoushash");
            entity.Property(e => e.Publickey)
                .HasMaxLength(255)
                .HasColumnName("publickey");
            entity.Property(e => e.Timestampblock)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestampblock");
            entity.Property(e => e.Transporterid).HasColumnName("transporterid");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Vendorid).HasColumnName("vendorid");
        });

        modelBuilder.Entity<Blockchainid>(entity =>
        {
            entity.HasKey(e => e.Blockchainid1).HasName("blockchainids_pkey");

            entity.ToTable("blockchainids");

            entity.Property(e => e.Blockchainid1)
                .HasMaxLength(24)
                .HasDefaultValueSql("nextval('blockchainids_blockchainid_seq'::regclass)")
                .HasColumnName("blockchainid");
            entity.Property(e => e.Usergroupid)
                .HasMaxLength(24)
                .HasColumnName("usergroupid");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Clientid).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.Property(e => e.Clientid).HasColumnName("clientid");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Clientname)
                .HasMaxLength(255)
                .HasColumnName("clientname");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Clients)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("clients_userid_fkey");
        });

        modelBuilder.Entity<Transporter>(entity =>
        {
            entity.HasKey(e => e.Transporterid).HasName("transporters_pkey");

            entity.ToTable("transporters");

            entity.Property(e => e.Transporterid).HasColumnName("transporterid");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Transportername)
                .HasMaxLength(255)
                .HasColumnName("transportername");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Transporters)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("transporters_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Publickey)
                .HasMaxLength(255)
                .HasColumnName("publickey");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Vendorid).HasName("vendors_pkey");

            entity.ToTable("vendors");

            entity.Property(e => e.Vendorid).HasColumnName("vendorid");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Vendorname)
                .HasMaxLength(255)
                .HasColumnName("vendorname");

            entity.HasOne(d => d.User).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("vendors_userid_fkey");
        });

        modelBuilder.Entity<Vendorproduct>(entity =>
        {
            entity.HasKey(e => e.Productid).HasName("vendorproducts_pkey");

            entity.ToTable("vendorproducts");

            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Productname)
                .HasMaxLength(255)
                .HasColumnName("productname");
            entity.Property(e => e.Productprice).HasColumnName("productprice");
            entity.Property(e => e.Productweight).HasColumnName("productweight");
            entity.Property(e => e.Vendorid).HasColumnName("vendorid");

            entity.HasOne(d => d.Vendor).WithMany(p => p.Vendorproducts)
                .HasForeignKey(d => d.Vendorid)
                .HasConstraintName("vendorproducts_vendorid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
