﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Trabahub.Data;

#nullable disable

namespace Trabahub.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Trabahub.Models.Credentials", b =>
                {
                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnOrder(0)
                        .UseCollation("SQL_Latin1_General_CP1_CS_AS");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.HasKey("Email");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("Trabahub.Models.Listing", b =>
                {
                    b.Property<string>("ESTABNAME")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("ENDTIME")
                        .HasColumnType("datetime2");

                    b.Property<string>("ESTABADD")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ESTABDESC")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("ESTABIMAGEPATH")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("ESTABPRICE")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<DateTime>("STARTTIME")
                        .HasColumnType("datetime2");

                    b.HasKey("ESTABNAME");

                    b.ToTable("Listing");
                });
#pragma warning restore 612, 618
        }
    }
}
