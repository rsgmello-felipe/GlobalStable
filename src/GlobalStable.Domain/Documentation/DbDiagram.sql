CREATE TABLE "Currency" (
  "Id" bigint PRIMARY KEY,
  "Code" varchar UNIQUE NOT NULL,
  "Name" varchar NOT NULL,
  "Precision" int NOT NULL
);

CREATE TABLE "BlockchainNetwork" (
  "Id" bigint PRIMARY KEY,
  "Name" varchar NOT NULL,
  "NativeCurrencyId" bigint
);

CREATE TABLE "CurrencyBlockchain" (
  "Id" bigint PRIMARY KEY,
  "CurrencyId" bigint,
  "BlockchainNetworkId" bigint
);

CREATE TABLE "OrderStatus" (
  "Id" bigint PRIMARY KEY,
  "Name" varchar NOT NULL
);

CREATE TABLE "Customer" (
  "Id" bigint PRIMARY KEY,
  "Name" varchar NOT NULL,
  "TaxId" varchar NOT NULL,
  "Country" varchar NOT NULL,
  "Enabled" boolean NOT NULL,
  "CreatedAt" datetime NOT NULL
);

CREATE TABLE "Account" (
  "Id" bigint PRIMARY KEY,
  "CustomerId" bigint,
  "CurrencyId" bigint,
  "Enabled" boolean NOT NULL,
  "CreatedAt" datetime NOT NULL
);

CREATE TABLE "WithdrawalOrder" (
  "Id" bigint PRIMARY KEY,
  "CreatedAt" datetime NOT NULL,
  "StatusId" bigint,
  "Description" varchar,
  "GatewayPaymentId" varchar,
  "Amount" decimal NOT NULL,
  "AccountId" bigint,
  "CurrencyId" bigint,
  "BlockchainNetworkId" bigint
);

CREATE TABLE "DepositOrder" (
  "Id" bigint PRIMARY KEY,
  "CreatedAt" datetime NOT NULL,
  "StatusId" bigint,
  "Description" varchar,
  "ReferenceId" varchar,
  "AccountInformation" varchar,
  "Amount" decimal NOT NULL,
  "AccountId" bigint,
  "CurrencyId" bigint,
  "BlockchainNetworkId" bigint
);

CREATE TABLE "QuoteOrder" (
  "Id" bigint PRIMARY KEY,
  "CreatedAt" datetime NOT NULL,
  "StatusId" bigint,
  "Description" varchar,
  "CustomerId" bigint,
  "BaseCurrencyId" bigint,
  "QuoteCurrencyId" bigint,
  "Side" varchar NOT NULL,
  "BaseAmount" decimal,
  "Price" decimal,
  "QuoteAmount" decimal,
  "FeeAmount" decimal,
  "BaseAccountId" bigint,
  "QuoteAccountId" bigint,
  "LastUpdatedAt" datetime NOT NULL
);

CREATE TABLE "OrderHistory" (
  "Id" bigint PRIMARY KEY,
  "WithdrawalOrderId" bigint,
  "DepositOrderId" bigint,
  "QuoteOrderId" bigint,
  "StatusId" bigint,
  "Reason" varchar,
  "ChangedBy" varchar NOT NULL,
  "CreatedAt" datetime NOT NULL
);

CREATE TABLE "BalanceSnapshot" (
  "Id" bigint PRIMARY KEY,
  "AccountId" bigint,
  "Amount" decimal NOT NULL,
  "CreatedAt" datetime NOT NULL
);

CREATE TABLE "Transaction" (
  "Id" bigint PRIMARY KEY,
  "AccountId" bigint,
  "CurrencyId" bigint,
  "Type" varchar NOT NULL,
  "OrderType" varchar NOT NULL,
  "WithdrawalOrderId" bigint,
  "DepositOrderId" bigint,
  "QuoteOrderId" bigint,
  "Amount" decimal NOT NULL,
  "CreatedAt" datetime NOT NULL
);

CREATE TABLE "PendingTransaction" (
  "Id" bigint PRIMARY KEY,
  "AccountId" bigint,
  "CurrencyId" bigint,
  "Type" varchar NOT NULL,
  "OrderType" varchar NOT NULL,
  "WithdrawalOrderId" bigint,
  "DepositOrderId" bigint,
  "QuoteOrderId" bigint,
  "Amount" decimal NOT NULL,
  "CreatedAt" datetime NOT NULL
);

CREATE UNIQUE INDEX ON "CurrencyBlockchain" ("CurrencyId", "BlockchainNetworkId");

CREATE UNIQUE INDEX ON "Account" ("CustomerId", "CurrencyId");

CREATE INDEX ON "OrderHistory" ("WithdrawalOrderId", "CreatedAt");

CREATE INDEX ON "OrderHistory" ("DepositOrderId", "CreatedAt");

CREATE INDEX ON "OrderHistory" ("QuoteOrderId", "CreatedAt");

CREATE INDEX ON "OrderHistory" ("CreatedAt");

CREATE UNIQUE INDEX ON "BalanceSnapshot" ("AccountId", "CreatedAt");

CREATE INDEX ON "BalanceSnapshot" ("CreatedAt");

CREATE UNIQUE INDEX ON "Transaction" ("WithdrawalOrderId", "OrderType", "AccountId");

CREATE UNIQUE INDEX ON "Transaction" ("DepositOrderId", "OrderType", "AccountId");

CREATE UNIQUE INDEX ON "Transaction" ("QuoteOrderId", "OrderType", "AccountId");

CREATE INDEX ON "Transaction" ("AccountId", "CreatedAt");

CREATE INDEX ON "Transaction" ("AccountId", "CurrencyId", "CreatedAt");

CREATE UNIQUE INDEX ON "PendingTransaction" ("WithdrawalOrderId", "AccountId");

CREATE UNIQUE INDEX ON "PendingTransaction" ("DepositOrderId", "AccountId");

CREATE UNIQUE INDEX ON "PendingTransaction" ("QuoteOrderId", "AccountId");

ALTER TABLE "BlockchainNetwork" ADD FOREIGN KEY ("NativeCurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "CurrencyBlockchain" ADD FOREIGN KEY ("CurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "CurrencyBlockchain" ADD FOREIGN KEY ("BlockchainNetworkId") REFERENCES "BlockchainNetwork" ("Id");

ALTER TABLE "Account" ADD FOREIGN KEY ("CustomerId") REFERENCES "Customer" ("Id");

ALTER TABLE "Account" ADD FOREIGN KEY ("CurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "WithdrawalOrder" ADD FOREIGN KEY ("StatusId") REFERENCES "OrderStatus" ("Id");

ALTER TABLE "WithdrawalOrder" ADD FOREIGN KEY ("AccountId") REFERENCES "Account" ("Id");

ALTER TABLE "WithdrawalOrder" ADD FOREIGN KEY ("CurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "WithdrawalOrder" ADD FOREIGN KEY ("BlockchainNetworkId") REFERENCES "BlockchainNetwork" ("Id");

ALTER TABLE "DepositOrder" ADD FOREIGN KEY ("StatusId") REFERENCES "OrderStatus" ("Id");

ALTER TABLE "DepositOrder" ADD FOREIGN KEY ("AccountId") REFERENCES "Account" ("Id");

ALTER TABLE "DepositOrder" ADD FOREIGN KEY ("CurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "DepositOrder" ADD FOREIGN KEY ("BlockchainNetworkId") REFERENCES "BlockchainNetwork" ("Id");

ALTER TABLE "QuoteOrder" ADD FOREIGN KEY ("StatusId") REFERENCES "OrderStatus" ("Id");

ALTER TABLE "QuoteOrder" ADD FOREIGN KEY ("CustomerId") REFERENCES "Customer" ("Id");

ALTER TABLE "QuoteOrder" ADD FOREIGN KEY ("BaseCurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "QuoteOrder" ADD FOREIGN KEY ("QuoteCurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "QuoteOrder" ADD FOREIGN KEY ("BaseAccountId") REFERENCES "Account" ("Id");

ALTER TABLE "QuoteOrder" ADD FOREIGN KEY ("QuoteAccountId") REFERENCES "Account" ("Id");

ALTER TABLE "OrderHistory" ADD FOREIGN KEY ("WithdrawalOrderId") REFERENCES "WithdrawalOrder" ("Id");

ALTER TABLE "OrderHistory" ADD FOREIGN KEY ("DepositOrderId") REFERENCES "DepositOrder" ("Id");

ALTER TABLE "OrderHistory" ADD FOREIGN KEY ("QuoteOrderId") REFERENCES "QuoteOrder" ("Id");

ALTER TABLE "OrderHistory" ADD FOREIGN KEY ("StatusId") REFERENCES "OrderStatus" ("Id");

ALTER TABLE "BalanceSnapshot" ADD FOREIGN KEY ("AccountId") REFERENCES "Account" ("Id");

ALTER TABLE "Transaction" ADD FOREIGN KEY ("AccountId") REFERENCES "Account" ("Id");

ALTER TABLE "Transaction" ADD FOREIGN KEY ("CurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "Transaction" ADD FOREIGN KEY ("WithdrawalOrderId") REFERENCES "WithdrawalOrder" ("Id");

ALTER TABLE "Transaction" ADD FOREIGN KEY ("DepositOrderId") REFERENCES "DepositOrder" ("Id");

ALTER TABLE "Transaction" ADD FOREIGN KEY ("QuoteOrderId") REFERENCES "QuoteOrder" ("Id");

ALTER TABLE "PendingTransaction" ADD FOREIGN KEY ("AccountId") REFERENCES "Account" ("Id");

ALTER TABLE "PendingTransaction" ADD FOREIGN KEY ("CurrencyId") REFERENCES "Currency" ("Id");

ALTER TABLE "PendingTransaction" ADD FOREIGN KEY ("WithdrawalOrderId") REFERENCES "WithdrawalOrder" ("Id");

ALTER TABLE "PendingTransaction" ADD FOREIGN KEY ("DepositOrderId") REFERENCES "DepositOrder" ("Id");

ALTER TABLE "PendingTransaction" ADD FOREIGN KEY ("QuoteOrderId") REFERENCES "QuoteOrder" ("Id");
