using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;



namespace T_FANCY_Back.Models
{

    public class TfancyContext : IdentityDbContext<User>
    {

        public TfancyContext(DbContextOptions<TfancyContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseLazyLoadingProxies();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
               .HasMany(pt => pt.UserRefreshTokens)
               .WithOne(p => p.user);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>()
               .HasMany(pt => pt.prod_image)
               .WithOne(p => p.product);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Brand>()
               .HasMany(pt => pt.products)
               .WithOne(p => p.brand);

        }
        public virtual DbSet<Manager> manager { get; set; }
        public virtual DbSet<Client> client { get; set; }
        public DbSet<Product> product { get; set; }
        public DbSet<Prod_image> prod_Image  { get; set; }
        public DbSet<UserRefreshToken> userRefreshToken { get; set; }
        public  virtual DbSet<Brand> brand { get; set; }
        public  virtual DbSet<FrontHome> frontHome { get; set; }


    }
}
