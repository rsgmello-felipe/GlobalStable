using System.Text.Json;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GlobalStable.Infrastructure.Persistence
{
    public class ServiceDbContext(DbContextOptions<ServiceDbContext> options) : DbContext(options)
    {
        public DbSet<DepositOrder> DepositOrders { get; set; }

        public DbSet<WithdrawalOrder> WithdrawalOrders { get; set; }

        public DbSet<OrderHistory> OrderHistories { get; set; }

        public DbSet<OrderStatus> RefOrderStatuses { get; set; }

        public DbSet<FeeConfig> FeeConfigs { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("global_order_id_seq")
                .StartsAt(1)
                .IncrementsBy(1);

            modelBuilder.Entity<DepositOrder>(entity =>
            {
                entity.ToTable("deposit_order");

                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('global_order_id_seq')");

                entity.Property(d => d.AccountId).HasColumnName("account_id").IsRequired();
                entity.Property(d => d.CustomerId).HasColumnName("customer_id").IsRequired();
                entity.Property(d => d.IsAutomated).HasColumnName("is_automated").IsRequired();
                entity.Property(d => d.RequestedAmount).HasColumnName("requested_amount").HasColumnType("numeric(18,2)").IsRequired();
                entity.Property(d => d.FeeAmount).HasColumnName("fee_amount").HasColumnType("numeric(18,2)").IsRequired();
                entity.Property(w => w.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(d => d.Name).HasColumnName("name").HasMaxLength(50);
                entity.Property(d => d.Origin).HasColumnName("origin").HasMaxLength(20);
                entity.Property(d => d.E2EId).HasColumnName("e2e_id").HasMaxLength(50);
                entity.Property(d => d.BankId).HasColumnName("bank_id").HasMaxLength(50);
                entity.Property(d => d.PixCopyPaste).HasColumnName("pix_copy_paste").HasMaxLength(500);
                entity.Property(d => d.Cvu).HasColumnName("cvu").HasMaxLength(50);
                entity.Property(d => d.BankReference).HasColumnName("bank_reference").HasMaxLength(50);
                entity.Property(d => d.PayerTaxId).HasColumnName("payer_tax_id").HasMaxLength(50);
                entity.Property(d => d.WebhookUrl).HasColumnName("webhook_url").HasMaxLength(200);
                entity.Property(d => d.StatusId).HasColumnName("status_id").IsRequired();
                entity.Property(d => d.StatusDescription).HasColumnName("status_description").HasMaxLength(200);
                entity.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(d => d.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
                entity.Property(d => d.LastUpdatedAt).HasColumnName("last_updated_at");
                entity.Property(d => d.LastUpdatedBy).HasColumnName("last_updated_by").HasMaxLength(100);

                entity.HasOne(d => d.Currency)
                    .WithMany()
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasMany(w => w.OrderHistory)
                    .WithOne()
                    .HasForeignKey(o => o.DepositOrderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("idx_order_history_deposit_order_id");

                entity.HasIndex(d => d.StatusId).HasDatabaseName("idx_deposit_order_status_id");
                entity.HasIndex(d => d.AccountId).HasDatabaseName("idx_deposit_order_account_id");
            });


            modelBuilder.Entity<WithdrawalOrder>(entity =>
            {
                entity.ToTable("withdrawal_order");

                entity.HasKey(w => w.Id);
                entity.Property(w => w.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('global_order_id_seq')");

                entity.Property(w => w.AccountId).HasColumnName("account_id").IsRequired();
                entity.Property(w => w.CustomerId).HasColumnName("customer_id").IsRequired();
                entity.Property(w => w.RequestedAmount).HasColumnName("requested_amount").HasColumnType("numeric(18,2)").IsRequired();
                entity.Property(w => w.FeeAmount).HasColumnName("fee_amount").HasColumnType("numeric(18,2)").IsRequired();
                entity.Property(w => w.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(w => w.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
                entity.Property(w => w.Origin).HasColumnName("origin").HasMaxLength(20);
                entity.Property(w => w.E2EId).HasColumnName("e2e_id").HasMaxLength(50);
                entity.Property(w => w.BankId).HasColumnName("bank_id").HasMaxLength(50);
                entity.Property(w => w.StatusId).HasColumnName("status_id").IsRequired();
                entity.Property(w => w.StatusDescription).HasColumnName("status_description").HasMaxLength(200);
                entity.Property(w => w.ReceiverAccountKey).HasColumnName("receiver_account_key").HasMaxLength(50);
                entity.Property(w => w.ReceiverTaxId).HasColumnName("receiver_tax_id").HasMaxLength(50);
                entity.Property(w => w.WebhookUrl).HasColumnName("webhook_url").HasMaxLength(200);
                entity.Property(w => w.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(w => w.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
                entity.Property(w => w.LastUpdatedAt).HasColumnName("last_updated_at");
                entity.Property(w => w.LastUpdatedBy).HasColumnName("last_updated_by").HasMaxLength(100);

                entity.HasOne(w => w.Currency)
                    .WithMany()
                    .HasForeignKey(w => w.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(w => w.OrderHistory)
                    .WithOne()
                    .HasForeignKey(o => o.WithdrawalOrderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_order_history_withdrawal_order");

                entity.HasIndex(w => w.StatusId).HasDatabaseName("idx_withdrawal_order_status_id");
                entity.HasIndex(w => w.AccountId).HasDatabaseName("idx_withdrawal_order_account_id");
            });

            modelBuilder.Entity<OrderHistory>(entity =>
            {
                entity.ToTable("order_history");

                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(o => o.WithdrawalOrderId).HasColumnName("withdrawal_order_id").IsRequired(false);
                entity.Property(o => o.DepositOrderId).HasColumnName("deposit_order_id").IsRequired(false);
                entity.Property(o => o.StatusId).HasColumnName("status_id").IsRequired();
                entity.Property(d => d.Description).HasColumnName("status_description").HasMaxLength(200);
                entity.Property(o => o.TransactionOrderType).HasColumnName("order_type").HasConversion(new EnumToStringConverter<TransactionOrderType>()).IsRequired();
                entity.Property(o => o.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
                entity.Property(o => o.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

                entity.HasIndex(o => o.WithdrawalOrderId)
                    .HasDatabaseName("idx_order_history_withdrawal_order_id");

                entity.HasIndex(o => o.DepositOrderId)
                    .HasDatabaseName("idx_order_history_deposit_order_id");
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("ref_order_status");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(r => r.Name).HasColumnName("status").HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<FeeConfig>(entity =>
            {
                entity.ToTable("fee_config");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(f => f.AccountId).HasColumnName("account_id").IsRequired();
                entity.Property(f => f.TransactionOrderType).HasColumnName("order_type").HasConversion(new EnumToStringConverter<TransactionOrderType>()).HasMaxLength(20).IsRequired();
                entity.Property(f => f.FeePercentage).HasColumnName("fee_percentage").HasColumnType("numeric(5,4)").IsRequired();
                entity.Property(f => f.FlatFee).HasColumnName("flat_fee").HasColumnType("numeric(18,2)").IsRequired();
                entity.Property(f => f.Enabled).HasColumnName("enabled").IsRequired();
                entity.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(f => f.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
                entity.Property(w => w.LastUpdatedAt).HasColumnName("last_updated_at");
                entity.Property(w => w.LastUpdatedBy).HasColumnName("last_updated_by").HasMaxLength(100);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(a => a.Name).HasColumnName("name").IsRequired();
                entity.Property(a => a.CustomerId).HasColumnName("customer_id").IsRequired();
                entity.Property(a => a.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(a => a.MaxAllowedDebt).HasColumnName("max_allowed_debt").HasColumnType("numeric(18,2)").IsRequired();
                entity.Property(a => a.WalletAddress).HasColumnName("wallet_address");
                entity.Property(a => a.IsAutomaticApproval).HasColumnName("automatic_approval").IsRequired();
                entity.Property(a => a.AutoExecuteWithdrawal).HasColumnName("automatic_withdrawal_execute").IsRequired();
                entity.Property(a => a.Enabled).HasColumnName("enabled").IsRequired();
                entity.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(a => a.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();

                entity.HasOne(a => a.Currency)
                    .WithMany()
                    .HasForeignKey(a => a.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("currency");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(f => f.Name).HasColumnName("name").IsRequired();
                entity.Property(f => f.Code).HasColumnName("code").IsRequired();
                entity.Property(f => f.Precision).HasColumnName("precision").IsRequired();
                entity.Property(f => f.Type).HasColumnName("type").HasConversion(new EnumToStringConverter<CurrencyType>()).HasMaxLength(20).IsRequired();
                entity.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(f => f.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
        }
    }
}
 