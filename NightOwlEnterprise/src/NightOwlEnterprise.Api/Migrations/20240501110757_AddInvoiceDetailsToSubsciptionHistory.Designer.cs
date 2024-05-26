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
    [Migration("20240501110757_AddInvoiceDetailsToSubsciptionHistory")]
    partial class AddInvoiceDetailsToSubsciptionHistory
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

            modelBuilder.Entity("CoachDetail", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("date");

                    b.Property<bool>("ChangedDepartmentType")
                        .HasColumnType("boolean");

                    b.Property<Guid>("DepartmentId")
                        .HasColumnType("uuid");

                    b.Property<int>("DepartmentType")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("FirstAytNet")
                        .HasColumnType("smallint");

                    b.Property<byte>("FirstTytNet")
                        .HasColumnType("smallint");

                    b.Property<byte>("FridayQuota")
                        .HasColumnType("smallint");

                    b.Property<int>("FromDepartment")
                        .HasColumnType("integer");

                    b.Property<bool>("GoneCramSchool")
                        .HasColumnType("boolean");

                    b.Property<string>("HighSchool")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float>("HighSchoolGPA")
                        .HasColumnType("real");

                    b.Property<bool>("IsGraduated")
                        .HasColumnType("boolean");

                    b.Property<byte>("LastAytNet")
                        .HasColumnType("smallint");

                    b.Property<byte>("LastTytNet")
                        .HasColumnType("smallint");

                    b.Property<bool>("Male")
                        .HasColumnType("boolean");

                    b.Property<string>("Mobile")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("MondayQuota")
                        .HasColumnType("smallint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("PrivateTutoring")
                        .HasColumnType("boolean");

                    b.Property<long>("Rank")
                        .HasColumnType("bigint");

                    b.Property<byte>("SaturdayQuota")
                        .HasColumnType("smallint");

                    b.Property<bool>("School")
                        .HasColumnType("boolean");

                    b.Property<byte>("StudentQuota")
                        .HasColumnType("smallint");

                    b.Property<byte>("SundayQuota")
                        .HasColumnType("smallint");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("ThursdayQuota")
                        .HasColumnType("smallint");

                    b.Property<int>("ToDepartment")
                        .HasColumnType("integer");

                    b.Property<byte>("TuesdayQuota")
                        .HasColumnType("smallint");

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

            modelBuilder.Entity("CoachDilNets", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<byte>("YDT")
                        .HasColumnType("smallint");

                    b.HasKey("CoachId");

                    b.ToTable("CoachDilNets");
                });

            modelBuilder.Entity("CoachMFNets", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<byte>("Biology")
                        .HasColumnType("smallint");

                    b.Property<byte>("Chemistry")
                        .HasColumnType("smallint");

                    b.Property<byte>("Geometry")
                        .HasColumnType("smallint");

                    b.Property<byte>("Mathematics")
                        .HasColumnType("smallint");

                    b.Property<byte>("Physics")
                        .HasColumnType("smallint");

                    b.HasKey("CoachId");

                    b.ToTable("CoachMFNets");
                });

            modelBuilder.Entity("CoachSozelNets", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<byte>("Geography1")
                        .HasColumnType("smallint");

                    b.Property<byte>("Geography2")
                        .HasColumnType("smallint");

                    b.Property<byte>("History1")
                        .HasColumnType("smallint");

                    b.Property<byte>("History2")
                        .HasColumnType("smallint");

                    b.Property<byte>("Literature1")
                        .HasColumnType("smallint");

                    b.Property<byte>("Philosophy")
                        .HasColumnType("smallint");

                    b.Property<byte>("Religion")
                        .HasColumnType("smallint");

                    b.HasKey("CoachId");

                    b.ToTable("CoachSozelNets");
                });

            modelBuilder.Entity("CoachTMNets", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<byte>("Geography")
                        .HasColumnType("smallint");

                    b.Property<byte>("Geometry")
                        .HasColumnType("smallint");

                    b.Property<byte>("History")
                        .HasColumnType("smallint");

                    b.Property<byte>("Literature")
                        .HasColumnType("smallint");

                    b.Property<byte>("Mathematics")
                        .HasColumnType("smallint");

                    b.HasKey("CoachId");

                    b.ToTable("CoachTMNets");
                });

            modelBuilder.Entity("CoachTYTNets", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<byte>("Biology")
                        .HasColumnType("smallint");

                    b.Property<byte>("Chemistry")
                        .HasColumnType("smallint");

                    b.Property<byte>("Geography")
                        .HasColumnType("smallint");

                    b.Property<byte>("Geometry")
                        .HasColumnType("smallint");

                    b.Property<byte>("Grammar")
                        .HasColumnType("smallint");

                    b.Property<byte>("History")
                        .HasColumnType("smallint");

                    b.Property<byte>("Mathematics")
                        .HasColumnType("smallint");

                    b.Property<byte>("Philosophy")
                        .HasColumnType("smallint");

                    b.Property<byte>("Physics")
                        .HasColumnType("smallint");

                    b.Property<byte>("Religion")
                        .HasColumnType("smallint");

                    b.Property<byte>("Semantics")
                        .HasColumnType("smallint");

                    b.HasKey("CoachId");

                    b.ToTable("CoachTYTNets");
                });

            modelBuilder.Entity("CoachYksRanking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Enter")
                        .HasColumnType("boolean");

                    b.Property<long>("Rank")
                        .HasColumnType("bigint");

                    b.Property<string>("Year")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CoachId");

                    b.ToTable("CoachYksRankings");
                });

            modelBuilder.Entity("Department", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("DepartmentType")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("Invitation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("interval");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("interval");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CoachId");

                    b.HasIndex("StudentId");

                    b.ToTable("Invitations");
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

            modelBuilder.Entity("PrivateTutoringDil", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<bool>("YTD")
                        .HasColumnType("boolean");

                    b.HasKey("CoachId");

                    b.ToTable("PrivateTutoringDil");
                });

            modelBuilder.Entity("PrivateTutoringMF", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Biology")
                        .HasColumnType("boolean");

                    b.Property<bool>("Chemistry")
                        .HasColumnType("boolean");

                    b.Property<bool>("Geometry")
                        .HasColumnType("boolean");

                    b.Property<bool>("Mathematics")
                        .HasColumnType("boolean");

                    b.Property<bool>("Physics")
                        .HasColumnType("boolean");

                    b.HasKey("CoachId");

                    b.ToTable("PrivateTutoringMF");
                });

            modelBuilder.Entity("PrivateTutoringSozel", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Geography1")
                        .HasColumnType("boolean");

                    b.Property<bool>("Geography2")
                        .HasColumnType("boolean");

                    b.Property<bool>("History1")
                        .HasColumnType("boolean");

                    b.Property<bool>("History2")
                        .HasColumnType("boolean");

                    b.Property<bool>("Literature1")
                        .HasColumnType("boolean");

                    b.Property<bool>("Philosophy")
                        .HasColumnType("boolean");

                    b.Property<bool>("Religion")
                        .HasColumnType("boolean");

                    b.HasKey("CoachId");

                    b.ToTable("PrivateTutoringSozel");
                });

            modelBuilder.Entity("PrivateTutoringTM", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Geography")
                        .HasColumnType("boolean");

                    b.Property<bool>("Geometry")
                        .HasColumnType("boolean");

                    b.Property<bool>("History")
                        .HasColumnType("boolean");

                    b.Property<bool>("Literature")
                        .HasColumnType("boolean");

                    b.Property<bool>("Mathematics")
                        .HasColumnType("boolean");

                    b.HasKey("CoachId");

                    b.ToTable("PrivateTutoringTM");
                });

            modelBuilder.Entity("PrivateTutoringTYT", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Biology")
                        .HasColumnType("boolean");

                    b.Property<bool>("Chemistry")
                        .HasColumnType("boolean");

                    b.Property<bool>("Geography")
                        .HasColumnType("boolean");

                    b.Property<bool>("Geometry")
                        .HasColumnType("boolean");

                    b.Property<bool>("History")
                        .HasColumnType("boolean");

                    b.Property<bool>("Mathematics")
                        .HasColumnType("boolean");

                    b.Property<bool>("Philosophy")
                        .HasColumnType("boolean");

                    b.Property<bool>("Physics")
                        .HasColumnType("boolean");

                    b.Property<bool>("Religion")
                        .HasColumnType("boolean");

                    b.Property<bool>("Turkish")
                        .HasColumnType("boolean");

                    b.HasKey("CoachId");

                    b.ToTable("PrivateTutoringTYT");
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

                    b.Property<string>("InvoiceId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("InvoiceState")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastError")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("SubscriptionEndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SubscriptionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("SubscriptionStartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SubscriptionState")
                        .IsRequired()
                        .HasColumnType("text");

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

            modelBuilder.Entity("CoachDilNets", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("CoachDilNets")
                        .HasForeignKey("CoachDilNets", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("CoachMFNets", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("CoachMfNets")
                        .HasForeignKey("CoachMFNets", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("CoachSozelNets", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("CoachSozelNets")
                        .HasForeignKey("CoachSozelNets", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("CoachTMNets", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("CoachTmNets")
                        .HasForeignKey("CoachTMNets", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("CoachTYTNets", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("CoachTytNets")
                        .HasForeignKey("CoachTYTNets", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("CoachYksRanking", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithMany("CoachYksRankings")
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("Invitation", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithMany("InvitationsAsCoach")
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ApplicationUser", "Student")
                        .WithMany("InvitationsAsStudent")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");

                    b.Navigation("Student");
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

            modelBuilder.Entity("PrivateTutoringDil", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("PrivateTutoringDil")
                        .HasForeignKey("PrivateTutoringDil", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("PrivateTutoringMF", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("PrivateTutoringMF")
                        .HasForeignKey("PrivateTutoringMF", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("PrivateTutoringSozel", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("PrivateTutoringSozel")
                        .HasForeignKey("PrivateTutoringSozel", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("PrivateTutoringTM", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("PrivateTutoringTM")
                        .HasForeignKey("PrivateTutoringTM", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("PrivateTutoringTYT", b =>
                {
                    b.HasOne("ApplicationUser", "Coach")
                        .WithOne("PrivateTutoringTYT")
                        .HasForeignKey("PrivateTutoringTYT", "CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
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

                    b.Navigation("CoachDilNets")
                        .IsRequired();

                    b.Navigation("CoachMfNets")
                        .IsRequired();

                    b.Navigation("CoachSozelNets")
                        .IsRequired();

                    b.Navigation("CoachTmNets")
                        .IsRequired();

                    b.Navigation("CoachTytNets")
                        .IsRequired();

                    b.Navigation("CoachYksRankings");

                    b.Navigation("InvitationsAsCoach");

                    b.Navigation("InvitationsAsStudent");

                    b.Navigation("PrivateTutoringDil")
                        .IsRequired();

                    b.Navigation("PrivateTutoringMF")
                        .IsRequired();

                    b.Navigation("PrivateTutoringSozel")
                        .IsRequired();

                    b.Navigation("PrivateTutoringTM")
                        .IsRequired();

                    b.Navigation("PrivateTutoringTYT")
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