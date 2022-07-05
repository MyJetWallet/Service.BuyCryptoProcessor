﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Service.BuyCryptoProcessor.Postgres;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("cryptobuy")
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Service.BuyCryptoProcessor.Domain.Models.CryptoBuyIntention", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("BrandId")
                        .HasColumnType("text");

                    b.Property<string>("BrokerId")
                        .HasColumnType("text");

                    b.Property<decimal>("BuyAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("BuyAsset")
                        .HasColumnType("text");

                    b.Property<decimal>("BuyFeeAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("BuyFeeAsset")
                        .HasColumnType("text");

                    b.Property<string>("CardId")
                        .HasColumnType("text");

                    b.Property<string>("CardLast4")
                        .HasColumnType("text");

                    b.Property<long>("CircleDepositId")
                        .HasColumnType("bigint");

                    b.Property<string>("ClientId")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

                    b.Property<string>("DepositCheckoutLink")
                        .HasColumnType("text");

                    b.Property<string>("DepositIntegration")
                        .HasColumnType("text");

                    b.Property<string>("DepositOperationId")
                        .HasColumnType("text");

                    b.Property<string>("DepositProfileId")
                        .HasColumnType("text");

                    b.Property<DateTime>("DepositTimestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

                    b.Property<string>("ExecuteQuoteId")
                        .HasColumnType("text");

                    b.Property<DateTime>("ExecuteTimestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

                    b.Property<decimal>("FeeAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("FeeAsset")
                        .HasColumnType("text");

                    b.Property<string>("LastError")
                        .HasColumnType("text");

                    b.Property<decimal>("PaymentAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("PaymentAsset")
                        .HasColumnType("text");

                    b.Property<string>("PaymentDetails")
                        .HasColumnType("text");

                    b.Property<int>("PaymentMethod")
                        .HasColumnType("integer");

                    b.Property<DateTime>("PreviewConvertTimestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValue(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

                    b.Property<string>("PreviewQuoteId")
                        .HasColumnType("text");

                    b.Property<decimal>("ProvidedCryptoAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("ProvidedCryptoAsset")
                        .HasColumnType("text");

                    b.Property<decimal>("QuotePrice")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Rate")
                        .HasColumnType("numeric");

                    b.Property<int>("RetriesCount")
                        .HasColumnType("integer");

                    b.Property<string>("ServiceWalletId")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<decimal>("SwapFeeAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("SwapFeeAsset")
                        .HasColumnType("text");

                    b.Property<string>("SwapProfileId")
                        .HasColumnType("text");

                    b.Property<string>("WalletId")
                        .HasColumnType("text");

                    b.Property<int>("WorkflowState")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("Status");

                    b.ToTable("intentions", "cryptobuy");
                });
#pragma warning restore 612, 618
        }
    }
}
