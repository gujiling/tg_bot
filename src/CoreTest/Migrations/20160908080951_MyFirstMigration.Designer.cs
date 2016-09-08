using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using CoreTest;

namespace CoreTest.Migrations
{
    [DbContext(typeof(SqliteContext))]
    [Migration("20160908080951_MyFirstMigration")]
    partial class MyFirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("CoreTest.StatisticInfo", b =>
                {
                    b.Property<int>("RuleId");

                    b.Property<string>("UserId");

                    b.Property<int>("Count");

                    b.HasKey("RuleId", "UserId");

                    b.ToTable("Infos");
                });

            modelBuilder.Entity("CoreTest.StatisticRule", b =>
                {
                    b.Property<int>("RuleId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CharId");

                    b.Property<string>("ChatType");

                    b.Property<string>("StatisticWord");

                    b.HasKey("RuleId");

                    b.ToTable("Rules");
                });
        }
    }
}
