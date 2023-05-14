﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TemperatureViewer.Database;

#nullable disable

namespace TemperatureViewer.Migrations
{
    [DbContext(typeof(DefaultContext))]
    [Migration("20230514085925_observer-id")]
    partial class observerid
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("ObserverSensor", b =>
                {
                    b.Property<int>("ObserversId")
                        .HasColumnType("int");

                    b.Property<int>("SensorsId")
                        .HasColumnType("int");

                    b.HasKey("ObserversId", "SensorsId");

                    b.HasIndex("SensorsId");

                    b.ToTable("ObserverSensor");
                });

            modelBuilder.Entity("SensorUser", b =>
                {
                    b.Property<int>("SensorsId")
                        .HasColumnType("int");

                    b.Property<int>("UsersId")
                        .HasColumnType("int");

                    b.HasKey("SensorsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("SensorUser");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Observer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Observers");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Sensor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("LocationId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ThresholdId")
                        .HasColumnType("int");

                    b.Property<string>("Uri")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("WasDisabled")
                        .HasColumnType("bit");

                    b.Property<string>("XPath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.HasIndex("ThresholdId");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Threshold", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("P1")
                        .HasColumnType("int");

                    b.Property<int>("P2")
                        .HasColumnType("int");

                    b.Property<int>("P3")
                        .HasColumnType("int");

                    b.Property<int>("P4")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Thresholds");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Value", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("MeasurementTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("SensorId")
                        .HasColumnType("int");

                    b.Property<decimal>("Temperature")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("MeasurementTime");

                    b.HasIndex("SensorId");

                    b.ToTable("Values");
                });

            modelBuilder.Entity("ObserverSensor", b =>
                {
                    b.HasOne("TemperatureViewer.Models.Entities.Observer", null)
                        .WithMany()
                        .HasForeignKey("ObserversId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TemperatureViewer.Models.Entities.Sensor", null)
                        .WithMany()
                        .HasForeignKey("SensorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SensorUser", b =>
                {
                    b.HasOne("TemperatureViewer.Models.Entities.Sensor", null)
                        .WithMany()
                        .HasForeignKey("SensorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TemperatureViewer.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Sensor", b =>
                {
                    b.HasOne("TemperatureViewer.Models.Entities.Location", "Location")
                        .WithMany("Sensors")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("TemperatureViewer.Models.Entities.Threshold", "Threshold")
                        .WithMany("Sensors")
                        .HasForeignKey("ThresholdId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Location");

                    b.Navigation("Threshold");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Value", b =>
                {
                    b.HasOne("TemperatureViewer.Models.Entities.Sensor", "Sensor")
                        .WithMany("Values")
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sensor");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Location", b =>
                {
                    b.Navigation("Sensors");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Sensor", b =>
                {
                    b.Navigation("Values");
                });

            modelBuilder.Entity("TemperatureViewer.Models.Entities.Threshold", b =>
                {
                    b.Navigation("Sensors");
                });
#pragma warning restore 612, 618
        }
    }
}
