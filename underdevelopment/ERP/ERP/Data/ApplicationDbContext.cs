using ERP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <-- Add this using directive


namespace ERP.Data
{
    // Itt IdentityUser helyett használhatsz saját ApplicationUser-t is, ha bõvítenéd
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EZ NAGYON FONTOS: maradjon az elsõ sor, ez konfigurálja az Identity táblákat
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Warehouse)
                .WithMany()
                .HasForeignKey(i => i.WarehouseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)   
                .IsUnique();

            modelBuilder.Entity<StockItem>()
                .HasOne(s => s.Warehouse)
                .WithMany(w => w.StockItems)
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade); 

        }
    }
}