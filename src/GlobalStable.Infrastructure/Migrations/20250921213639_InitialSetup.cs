using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GlobalStable.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relkind = 'S' AND relname = 'global_order_id_seq') THEN
                        CREATE SEQUENCE global_order_id_seq START WITH 1 INCREMENT BY 1;
                    END IF;
                END
                $$;
            ");

            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    precision = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    tax_id = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    quote_spread = table.Column<decimal>(type: "numeric", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ref_order_status",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_order_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    wallet_address = table.Column<string>(type: "text", nullable: true),
                    withdrawal_percentage_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    withdrawal_flat_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    deposit_percentage_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    deposit_flat_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "balance_snapshots",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    interval_balance = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    total_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    last_transaction_id = table.Column<long>(type: "bigint", nullable: true),
                    previous_balance_snapshot_id = table.Column<long>(type: "bigint", nullable: true),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId1 = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_balance_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_balance_snapshots_currency_CurrencyId1",
                        column: x => x.CurrencyId1,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_balance_snapshots_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "blockchain_network",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    NativeCurrencyId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchain_network", x => x.id);
                    table.ForeignKey(
                        name: "FK_blockchain_network_currency_NativeCurrencyId",
                        column: x => x.NativeCurrencyId,
                        principalTable: "currency",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "deposit_order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('global_order_id_seq')"),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    requested_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    fee_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    status_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    e2e_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    wallet_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    blockchain_network_id = table.Column<string>(type: "text", nullable: true),
                    expire_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_deposit_order_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pending_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    order_type = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_pending_transactions_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    order_type = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "withdrawal_order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('global_order_id_seq')"),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    status_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    requested_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    fee_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    e2e_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    receiver_tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_account_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_wallet_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    blockchain_network_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawal_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_withdrawal_order_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quote_order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    status_description = table.Column<string>(type: "text", nullable: true),
                    base_currency_id = table.Column<long>(type: "bigint", nullable: false),
                    quote_currency_id = table.Column<long>(type: "bigint", nullable: false),
                    side = table.Column<int>(type: "integer", nullable: false),
                    base_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: true),
                    quote_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: true),
                    price = table.Column<decimal>(type: "numeric(38,18)", nullable: true),
                    fee_amount = table.Column<decimal>(type: "numeric(38,18)", nullable: true),
                    base_account_id = table.Column<long>(type: "bigint", nullable: false),
                    quote_account_id = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quote_order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quote_order_account_base_account_id",
                        column: x => x.base_account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quote_order_account_quote_account_id",
                        column: x => x.quote_account_id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quote_order_currency_base_currency_id",
                        column: x => x.base_currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_quote_order_currency_quote_currency_id",
                        column: x => x.quote_currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "currency_blockchain",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    blockchain_network_id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_blockchain", x => x.id);
                    table.ForeignKey(
                        name: "FK_currency_blockchain_blockchain_network_blockchain_network_id",
                        column: x => x.blockchain_network_id,
                        principalTable: "blockchain_network",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_currency_blockchain_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    withdrawal_order_id = table.Column<long>(type: "bigint", nullable: true),
                    deposit_order_id = table.Column<long>(type: "bigint", nullable: true),
                    QuoteOrderId = table.Column<long>(type: "bigint", nullable: true),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    status_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    order_type = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_history_withdrawal_order",
                        column: x => x.withdrawal_order_id,
                        principalTable: "withdrawal_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "idx_order_history_deposit_order_id",
                        column: x => x.deposit_order_id,
                        principalTable: "deposit_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_currency_id",
                table: "account",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "idx_balance_snapshot_account_created_at",
                table: "balance_snapshots",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_balance_snapshot_account_id",
                table: "balance_snapshots",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_balance_snapshots_currency_id",
                table: "balance_snapshots",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_balance_snapshots_CurrencyId1",
                table: "balance_snapshots",
                column: "CurrencyId1");

            migrationBuilder.CreateIndex(
                name: "IX_blockchain_network_NativeCurrencyId",
                table: "blockchain_network",
                column: "NativeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_currency_blockchain_blockchain_network_id",
                table: "currency_blockchain",
                column: "blockchain_network_id");

            migrationBuilder.CreateIndex(
                name: "IX_currency_blockchain_currency_id_blockchain_network_id",
                table: "currency_blockchain",
                columns: new[] { "currency_id", "blockchain_network_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_deposit_order_account_id",
                table: "deposit_order",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_deposit_order_status_id",
                table: "deposit_order",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposit_order_currency_id",
                table: "deposit_order",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_history_deposit_order_id",
                table: "order_history",
                column: "deposit_order_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_history_withdrawal_order_id",
                table: "order_history",
                column: "withdrawal_order_id");

            migrationBuilder.CreateIndex(
                name: "idx_pending_transaction_account_created_at",
                table: "pending_transactions",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_pending_transaction_account_id",
                table: "pending_transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_pending_transaction_order_id",
                table: "pending_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_pending_transactions_currency_id",
                table: "pending_transactions",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_quote_order_base_account_id",
                table: "quote_order",
                column: "base_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_quote_order_base_currency_id",
                table: "quote_order",
                column: "base_currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_quote_order_quote_account_id",
                table: "quote_order",
                column: "quote_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_quote_order_quote_currency_id",
                table: "quote_order",
                column: "quote_currency_id");

            migrationBuilder.CreateIndex(
                name: "idx_transaction_account_created_at",
                table: "transactions",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_transaction_account_id",
                table: "transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_transaction_account_order_created_at",
                table: "transactions",
                columns: new[] { "account_id", "order_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_transaction_order_id",
                table: "transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_currency_id",
                table: "transactions",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "idx_withdrawal_order_account_id",
                table: "withdrawal_order",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "idx_withdrawal_order_status_id",
                table: "withdrawal_order",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawal_order_currency_id",
                table: "withdrawal_order",
                column: "currency_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "balance_snapshots");

            migrationBuilder.DropTable(
                name: "currency_blockchain");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "order_history");

            migrationBuilder.DropTable(
                name: "pending_transactions");

            migrationBuilder.DropTable(
                name: "quote_order");

            migrationBuilder.DropTable(
                name: "ref_order_status");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "blockchain_network");

            migrationBuilder.DropTable(
                name: "withdrawal_order");

            migrationBuilder.DropTable(
                name: "deposit_order");

            migrationBuilder.DropTable(
                name: "account");

            migrationBuilder.DropTable(
                name: "currency");

            migrationBuilder.DropSequence(
                name: "global_order_id_seq");
        }
    }
}
