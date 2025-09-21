using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GlobalStable.Infrastructure.Persistence
{
    public class ServiceDbContext(DbContextOptions<ServiceDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        public DbSet<DepositOrder> DepositOrders { get; set; }

        public DbSet<WithdrawalOrder> WithdrawalOrders { get; set; }

        public DbSet<QuoteOrder> QuoteOrders { get; set; }

        public DbSet<OrderHistory> OrderHistories { get; set; }

        public DbSet<OrderStatus> OrderStatuses { get; set; }

        public DbSet<BlockchainNetwork?> BlockchainNetworks { get; set; }

        public DbSet<CurrencyBlockchain> CurrencyBlockchains { get; set; }

        public DbSet<BalanceSnapshot?> BalanceSnapshots { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<PendingTransaction> PendingTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("global_order_id_seq")
                .StartsAt(1)
                .IncrementsBy(1);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();
                
                entity.Property(c => c.Name)
                    .HasColumnName("name")
                    .IsRequired();
                
                entity.Property(c => c.TaxId)
                    .HasColumnName("tax_id")
                    .IsRequired();
                
                entity.Property(c => c.Country)
                    .HasColumnName("country")
                    .IsRequired();
                
                entity.Property(c => c.QuoteSpread)
                    .HasColumnName("quote_spread")
                    .IsRequired();
                
                entity.Property(c => c.Enabled)
                    .HasColumnName("enabled")
                    .IsRequired();
                
                entity.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();
                
                entity.Property(c => c.CreatedBy)
                    .HasColumnName("created_by")
                    .IsRequired();
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");
                
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();
                
                entity.Property(a => a.Name)
                    .HasColumnName("name")
                    .IsRequired();
                
                entity.Property(a => a.CustomerId)
                    .HasColumnName("customer_id")
                    .IsRequired();
                
                entity.Property(a => a.CurrencyId)
                    .HasColumnName("currency_id")
                    .IsRequired();
                entity.Property(a => a.WalletAddress)
                    .HasColumnName("wallet_address");
                
                entity.Property(a => a.WithdrawalPercentageFee)
                    .HasColumnName("withdrawal_percentage_fee");
                
                entity.Property(a => a.WithdrawalFlatFee)
                    .HasColumnName("withdrawal_flat_fee");
                
                entity.Property(a => a.DepositPercentageFee)
                    .HasColumnName("deposit_percentage_fee");
                
                entity.Property(a => a.DepositFlatFee)
                    .HasColumnName("deposit_flat_fee");
                
                entity.Property(a => a.Enabled)
                    .HasColumnName("enabled")
                    .IsRequired();
                
                entity.Property(a => a.LastUpdatedAt)
                    .HasColumnName("last_updated_at")
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(a => a.LastUpdatedBy)
                    .HasColumnName("last_updated_by")
                    .HasMaxLength(100)
                    .IsRequired();
                
                entity.Property(a => a.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(a => a.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasOne(a => a.Currency)
                    .WithMany()
                    .HasForeignKey(a => a.CurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("currency");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(c => c.Name).HasColumnName("name").IsRequired();
                entity.Property(c => c.Code).HasColumnName("code").IsRequired();
                entity.Property(c => c.Precision).HasColumnName("precision").IsRequired();
                entity.Property(c => c.Type).HasColumnName("type")
                    .HasConversion(new EnumToStringConverter<CurrencyType>()).IsRequired();
                entity.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(c => c.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<DepositOrder>(entity =>
            {
                entity.ToTable("deposit_order");

                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('global_order_id_seq')");

                entity.Property(d => d.AccountId).HasColumnName("account_id").IsRequired();
                entity.Property(d => d.CustomerId).HasColumnName("customer_id").IsRequired();
                entity.Property(d => d.RequestedAmount).HasColumnName("requested_amount").HasColumnType("numeric(38, 18)")
                    .IsRequired();
                entity.Property(d => d.FeeAmount).HasColumnName("fee_amount").HasColumnType("numeric(38, 18)")
                    .IsRequired();
                entity.Property(d => d.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(38, 18)")
                    .IsRequired();
                entity.Property(w => w.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(d => d.StatusId).HasColumnName("status_id").IsRequired();
                entity.Property(d => d.StatusDescription).HasColumnName("status_description").HasMaxLength(200);
                entity.Property(d => d.E2EId).HasColumnName("e2e_id").HasMaxLength(50);
                entity.Property(d => d.BankReference).HasColumnName("bank_reference").HasMaxLength(50);
                entity.Property(d => d.WalletAddress).HasColumnName("wallet_address").HasMaxLength(50);
                entity.Property(d => d.BlockchainNetworkId).HasColumnName("blockchain_network_id");
                entity.Property(d => d.ExpireAt).HasColumnName("expire_at").HasDefaultValueSql("NOW()");
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

                entity.Property(w => w.AccountId)
                    .HasColumnName("account_id")
                    .IsRequired();
                
                entity.Property(w => w.CustomerId)
                    .HasColumnName("customer_id")
                    .IsRequired();
                
                entity.Property(w => w.StatusId)
                    .HasColumnName("status_id")
                    .IsRequired();
                
                entity.Property(w => w.StatusDescription)
                    .HasColumnName("status_description")
                    .HasMaxLength(200);
                
                entity.Property(w => w.RequestedAmount)
                    .HasColumnName("requested_amount")
                    .HasColumnType("numeric(38, 18)")
                    .IsRequired();
                entity.Property(w => w.FeeAmount).HasColumnName("fee_amount").HasColumnType("numeric(38, 18)")
                    .IsRequired();
                entity.Property(w => w.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(38, 18)")
                    .IsRequired();
                entity.Property(w => w.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(w => w.E2EId).HasColumnName("e2e_id").HasMaxLength(50);
                entity.Property(w => w.ReceiverName).HasColumnName("receiver_name").HasMaxLength(50).IsRequired();
                entity.Property(w => w.ReceiverTaxId).HasColumnName("receiver_tax_id").HasMaxLength(50);
                entity.Property(w => w.ReceiverAccountKey).HasColumnName("receiver_account_key").HasMaxLength(50);
                entity.Property(w => w.ReceiverWalletAddress).HasColumnName("receiver_wallet_address").HasMaxLength(50);
                entity.Property(w => w.BlockchainNetworkId).HasColumnName("blockchain_network_id").HasMaxLength(50);
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

            modelBuilder.Entity<QuoteOrder>(builder =>
            {
                builder.ToTable("quote_order");

                builder.HasKey(q => q.Id);

                builder.Property(q => q.CustomerId)
                    .HasColumnName("customer_id")
                    .IsRequired();

                builder.Property(q => q.StatusId)
                    .HasColumnName("status_id")
                    .IsRequired();

                builder.Property(q => q.StatusDescription)
                    .HasColumnName("status_description")
                    .IsRequired(false);

                builder.Property(q => q.BaseCurrencyId)
                    .HasColumnName("base_currency_id")
                    .IsRequired();

                builder.Property(q => q.QuoteCurrencyId)
                    .HasColumnName("quote_currency_id")
                    .IsRequired();

                builder.Property(q => q.Side)
                    .HasColumnName("side")
                    .IsRequired();

                builder.Property(q => q.BaseAmount)
                    .HasColumnName("base_amount")
                    .HasColumnType("numeric(38, 18)")
                    .IsRequired(false);

                builder.Property(q => q.QuoteAmount)
                    .HasColumnName("quote_amount")
                    .HasColumnType("numeric(38, 18)")
                    .IsRequired(false);

                builder.Property(q => q.Price)
                    .HasColumnName("price")
                    .HasColumnType("numeric(38, 18)")
                    .IsRequired(false);

                builder.Property(q => q.FeeAmount)
                    .HasColumnName("fee_amount")
                    .HasColumnType("numeric(38, 18)")
                    .IsRequired(false);

                builder.Property(q => q.BaseAccountId)
                    .HasColumnName("base_account_id")
                    .IsRequired();

                builder.Property(q => q.QuoteAccountId)
                    .HasColumnName("quote_account_id")
                    .IsRequired();

                builder.Property(q => q.LastUpdatedAt)
                    .HasColumnName("last_updated_at")
                    .IsRequired();

                builder.Property(q => q.LastUpdatedBy)
                    .HasColumnName("last_updated_by")
                    .IsRequired(false);

                builder.HasOne(q => q.BaseCurrency)
                    .WithMany()
                    .HasForeignKey(q => q.BaseCurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(q => q.QuoteCurrency)
                    .WithMany()
                    .HasForeignKey(q => q.QuoteCurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            modelBuilder.Entity<OrderHistory>(entity =>
            {
                entity.ToTable("order_history");

                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(o => o.WithdrawalOrderId).HasColumnName("withdrawal_order_id").IsRequired(false);
                entity.Property(o => o.DepositOrderId).HasColumnName("deposit_order_id").IsRequired(false);
                entity.Property(o => o.StatusId).HasColumnName("status_id").IsRequired();
                entity.Property(d => d.StatusDescription).HasColumnName("status_description").HasMaxLength(200);
                entity.Property(o => o.OrderType).HasColumnName("order_type")
                    .HasConversion(new EnumToStringConverter<OrderType>()).IsRequired();
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

            modelBuilder.Entity<BalanceSnapshot>(entity =>
            {
                entity.ToTable("balance_snapshots");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id)
                    .HasColumnName("id")
                    .UseIdentityColumn();
                
                entity.Property(t => t.CustomerId)
                    .HasColumnName("customer_id")
                    .IsRequired();
                
                entity.Property(t => t.AccountId)
                    .HasColumnName("account_id")
                    .IsRequired();
                
                entity.Property(t => t.IntervalBalance)
                    .HasColumnName("interval_balance")
                    .HasColumnType("numeric(38, 18)")
                    .IsRequired();
                
                entity.Property(t => t.TotalBalance)
                    .HasColumnName("total_balance")
                    .IsRequired();
                
                entity.Property(t => t.LastTransactionId)
                    .HasColumnName("last_transaction_id");
                
                entity.Property(t => t.PreviousBalanceSnapshotId)
                    .HasColumnName("previous_balance_snapshot_id");
                
                entity.Property(t => t.CurrencyId)
                    .HasColumnName("currency_id");
                
                entity.Property(t => t.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("NOW()");
                
                entity.Property(t => t.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(t => t.AccountId).HasDatabaseName("idx_balance_snapshot_account_id");

                entity.HasIndex(t => new { t.AccountId, t.CreatedAt })
                    .HasDatabaseName("idx_balance_snapshot_account_created_at");

                entity.HasOne<Currency>()
                    .WithMany()
                    .HasForeignKey(b => b.CurrencyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PendingTransaction>(entity =>
            {
                entity.ToTable("pending_transactions");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(t => t.AccountId).HasColumnName("account_id").IsRequired();
                entity.Property(t => t.CustomerId).HasColumnName("customer_id").IsRequired();
                entity.Property(t => t.Type).HasColumnName("type")
                    .HasConversion(new EnumToStringConverter<TransactionType>()).IsRequired();
                entity.Property(t => t.Amount).HasColumnName("amount").HasColumnType("numeric(38, 18)").IsRequired();
                entity.Property(t => t.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(t => t.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(t => t.OrderType).HasColumnName("order_type")
                    .HasConversion(new EnumToStringConverter<TransactionOrderType>()).IsRequired();
                entity.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(t => t.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();

                entity.HasOne(t => t.Currency)
                    .WithMany()
                    .HasForeignKey(t => t.CurrencyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(t => t.AccountId).HasDatabaseName("idx_pending_transaction_account_id");
                entity.HasIndex(t => t.OrderId).HasDatabaseName("idx_pending_transaction_order_id");
                entity.HasIndex(t => new { t.AccountId, t.CreatedAt })
                    .HasDatabaseName("idx_pending_transaction_account_created_at");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(t => t.AccountId).HasColumnName("account_id").IsRequired();
                entity.Property(t => t.CustomerId).HasColumnName("customer_id").IsRequired();
                entity.Property(t => t.Type).HasColumnName("type")
                    .HasConversion(new EnumToStringConverter<TransactionType>()).IsRequired();
                entity.Property(t => t.Amount).HasColumnName("amount").HasColumnType("numeric(38, 18)").IsRequired();
                entity.Property(t => t.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(t => t.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(t => t.OrderType).HasColumnName("order_type")
                    .HasConversion(new EnumToStringConverter<TransactionOrderType>()).IsRequired();
                entity.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(t => t.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();

                entity.HasOne(t => t.Currency)
                    .WithMany()
                    .HasForeignKey(t => t.CurrencyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(t => t.AccountId).HasDatabaseName("idx_transaction_account_id");
                entity.HasIndex(t => t.OrderId).HasDatabaseName("idx_transaction_order_id");
                entity.HasIndex(t => new { t.AccountId, t.CreatedAt })
                    .HasDatabaseName("idx_transaction_account_created_at");
                entity.HasIndex(t => new { t.AccountId, t.OrderId, t.CreatedAt })
                    .HasDatabaseName("idx_transaction_account_order_created_at");
            });

            modelBuilder.Entity<BlockchainNetwork>(entity =>
            {
                entity.ToTable("blockchain_network");
                entity.HasKey(bn => bn.Id);
                entity.Property(bn => bn.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(bn => bn.Name).HasColumnName("name").IsRequired();
            });

            modelBuilder.Entity<CurrencyBlockchain>(entity =>
            {
                entity.ToTable("currency_blockchain");
                entity.HasKey(cb => cb.Id);
                entity.Property(cb => cb.Id).HasColumnName("id").UseIdentityColumn();
                entity.Property(cb => cb.CurrencyId).HasColumnName("currency_id").IsRequired();
                entity.Property(cb => cb.BlockchainNetworkId).HasColumnName("blockchain_network_id").IsRequired();
                
                entity.HasIndex(cb => new { cb.CurrencyId, cb.BlockchainNetworkId }).IsUnique();
                
                entity.HasOne(cb => cb.Currency)
                    .WithMany(c => c.BlockchainNetworks)
                    .HasForeignKey(cb => cb.CurrencyId);
                
                entity.HasOne(cb => cb.BlockchainNetwork)
                    .WithMany(bn => bn.SupportedCurrencies)
                    .HasForeignKey(cb => cb.BlockchainNetworkId);
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
        }
    }
}