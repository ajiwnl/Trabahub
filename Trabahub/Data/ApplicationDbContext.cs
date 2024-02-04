﻿using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Trabahub.Models;

namespace Trabahub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Credentials> Credentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Credentials>()
            .Property(c => c.Email)
            .UseCollation("SQL_Latin1_General_CP1_CS_AS");
        }
    }
}