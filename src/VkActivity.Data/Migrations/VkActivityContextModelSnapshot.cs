﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VkActivity.Data;

#nullable disable

namespace VkActivity.Data.Migrations
{
    [DbContext(typeof(VkActivityContext))]
    partial class VkActivityContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseSerialColumns(modelBuilder);

            modelBuilder.Entity("VkActivity.Data.Models.ActivityLogItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("insert_date")
                        .HasDefaultValueSql("now()");

                    b.Property<bool?>("IsOnline")
                        .HasColumnType("bool")
                        .HasColumnName("is_online");

                    b.Property<int>("LastSeen")
                        .HasColumnType("int")
                        .HasColumnName("last_seen");

                    b.Property<int>("Platform")
                        .HasColumnType("int")
                        .HasColumnName("platform");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "LastSeen", "InsertDate");

                    b.ToTable("activity_log", "vk");

                    b.HasComment("Vk users activity log item");
                });

            modelBuilder.Entity("VkActivity.Data.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<string>("FirstName")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("first_name");

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("insert_date")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("LastName")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("last_name");

                    b.Property<string>("RawData")
                        .HasMaxLength(50)
                        .HasColumnType("json")
                        .HasColumnName("raw_data");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("update_date")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id");

                    b.ToTable("users", "vk");
                });

            modelBuilder.Entity("VkActivity.Data.Models.ActivityLogItem", b =>
                {
                    b.HasOne("VkActivity.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
