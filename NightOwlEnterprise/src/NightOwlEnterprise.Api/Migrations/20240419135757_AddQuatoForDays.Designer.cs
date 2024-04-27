﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NightOwlEnterprise.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240419135757_AddQuatoForDays")]
    partial class AddQuatoForDays
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ApplicationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("CustomerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PasswordResetCode")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PasswordResetCodeExpiration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("RefreshTokenExpiration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<string>("SubscriptionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<int>("UserType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("CoachCalendar", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("interval");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("interval");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CoachId");

                    b.HasIndex("StudentId");

                    b.ToTable("CoachCalendars");
                });

            modelBuilder.Entity("CoachDetail", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<bool>("ChangedSection")
                        .HasColumnType("boolean");

                    b.Property<Guid>("DepartmentId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Dil")
                        .HasColumnType("boolean");

                    b.Property<byte>("FirstTytNet")
                        .HasColumnType("smallint");

                    b.Property<byte>("FridayQuota")
                        .HasColumnType("smallint");

                    b.Property<string>("FromSection")
                        .HasColumnType("text");

                    b.Property<bool>("GoneCramSchool")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsGraduated")
                        .HasColumnType("boolean");

                    b.Property<bool>("Male")
                        .HasColumnType("boolean");

                    b.Property<bool>("Mf")
                        .HasColumnType("boolean");

                    b.Property<byte>("MondayQuota")
                        .HasColumnType("smallint");

                    b.Property<long>("Rank")
                        .HasColumnType("bigint");

                    b.Property<byte>("SaturdayQuota")
                        .HasColumnType("smallint");

                    b.Property<bool>("Sozel")
                        .HasColumnType("boolean");

                    b.Property<byte>("StudentQuota")
                        .HasColumnType("smallint");

                    b.Property<byte>("SundayQuota")
                        .HasColumnType("smallint");

                    b.Property<byte>("ThursdayQuota")
                        .HasColumnType("smallint");

                    b.Property<bool>("Tm")
                        .HasColumnType("boolean");

                    b.Property<string>("ToSection")
                        .HasColumnType("text");

                    b.Property<byte>("TuesdayQuota")
                        .HasColumnType("smallint");

                    b.Property<bool>("Tyt")
                        .HasColumnType("boolean");

                    b.Property<Guid>("UniversityId")
                        .HasColumnType("uuid");

                    b.Property<bool>("UsedYoutube")
                        .HasColumnType("boolean");

                    b.Property<byte>("WednesdayQuota")
                        .HasColumnType("smallint");

                    b.HasKey("CoachId");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("UniversityId");

                    b.ToTable("CoachDetail");
                });

            modelBuilder.Entity("Department", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("StudentDetail", b =>
                {
                    b.Property<Guid>("StudentId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<bool>("TermsAndConditionsAccepted")
                        .HasColumnType("boolean");

                    b.HasKey("StudentId");

                    b.ToTable("StudentDetail");
                });

            modelBuilder.Entity("SubscriptionHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("SubscriptionEndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("SubscriptionStartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("SubscriptionHistories");
                });

            modelBuilder.Entity("University", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Universities");
                });

            modelBuilder.Entity("UniversityDepartment", b =>
                {
                    b.Property<Guid>("UniversityId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("DepartmentId")
                        .HasColumnType("uuid");

                    b.HasKey("UniversityId", "DepartmentId");

                    b.HasIndex("DepartmentId");

                    b.ToTable("UniversityDepartments");
                });

            modelBuilder.Entity("CoachCalendar", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithMany()
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApplicationUser", "Student")
                        .WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("CoachDetail", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("CoachDetail")
                        .HasForeignKey("CoachDetail", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("University", "University")
                        .WithMany()
                        .HasForeignKey("UniversityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");

                    b.Navigation("Department");

                    b.Navigation("University");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("ApplicationRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("StudentDetail", b =>
                {
                    b.HasOne("ApplicationUser", "Student")
                        .WithOne("StudentDetail")
                        .HasForeignKey("StudentDetail", "StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");
                });

            modelBuilder.Entity("SubscriptionHistory", b =>
                {
                    b.HasOne("ApplicationUser", "User")
                        .WithMany("SubscriptionHistories")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("UniversityDepartment", b =>
                {
                    b.HasOne("Department", "Department")
                        .WithMany("UniversityDepartments")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("University", "University")
                        .WithMany("UniversityDepartments")
                        .HasForeignKey("UniversityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Department");

                    b.Navigation("University");
                });

            modelBuilder.Entity("ApplicationUser", b =>
                {
                    b.Navigation("CoachDetail")
                        .IsRequired();

                    b.Navigation("StudentDetail")
                        .IsRequired();

                    b.Navigation("SubscriptionHistories");
                });

            modelBuilder.Entity("Department", b =>
                {
                    b.Navigation("UniversityDepartments");
                });

            modelBuilder.Entity("University", b =>
                {
                    b.Navigation("UniversityDepartments");
                });
#pragma warning restore 612, 618
        }
    }
}
