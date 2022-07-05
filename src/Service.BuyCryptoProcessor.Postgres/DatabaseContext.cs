using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Postgres;
using Service.BuyCryptoProcessor.Domain.Models;

namespace Service.BuyCryptoProcessor.Postgres
{
    public class DatabaseContext : MyDbContext
    {
        public const string Schema = "cryptobuy";

        public const string IntentionsCryptoBuy = "intentions";

        public DbSet<CryptoBuyIntention> Intentions { get; set; }


        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<CryptoBuyIntention>().ToTable(IntentionsCryptoBuy);
            modelBuilder.Entity<CryptoBuyIntention>().HasKey(e => e.Id);
            modelBuilder.Entity<CryptoBuyIntention>().Property(e => e.CreationTime).HasDefaultValue(DateTime.MinValue);
            modelBuilder.Entity<CryptoBuyIntention>().Property(e => e.DepositTimestamp).HasDefaultValue(DateTime.MinValue);
            modelBuilder.Entity<CryptoBuyIntention>().Property(e => e.ExecuteTimestamp).HasDefaultValue(DateTime.MinValue);
            modelBuilder.Entity<CryptoBuyIntention>().Property(e => e.PreviewConvertTimestamp).HasDefaultValue(DateTime.MinValue);
            
            modelBuilder.Entity<CryptoBuyIntention>().HasIndex(e => e.Status);
            modelBuilder.Entity<CryptoBuyIntention>().HasIndex(e => e.ClientId);

            base.OnModelCreating(modelBuilder);
        }

        public async Task<int> UpsertAsync(IEnumerable<CryptoBuyIntention> entities)
        {
            var result = await Intentions.UpsertRange(entities).AllowIdentityMatch().RunAsync();
            return result;
        }
        
    }
}
