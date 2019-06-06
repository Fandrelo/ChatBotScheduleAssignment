using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Areas.Identity.Data;
using ChatBot.Models.ScheduleAssignment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Models
{
    public class ChatBotContext : IdentityDbContext<ChatBotUser>
    {
        public ChatBotContext(DbContextOptions<ChatBotContext> options)
            : base(options)
        {
        }

        public DbSet<MailList> MailLists { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<Assignment> Assignments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<MailList>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OwnerEmail)
                    .IsRequired()
                    .IsUnicode(false);
            });

            builder.Entity<Email>(entity =>
            {
                entity.HasKey(e => e.Address);

                entity.Property(e => e.Address)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.HasOne(d => d.MailListNavigation)
                    .WithMany(p => p.Emails)
                    .HasForeignKey(d => d.MailList)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.Property(e => e.OwnerEmail)
                    .IsRequired()
                    .IsUnicode(false);
            });

            builder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.DueDate)
                    .HasColumnType("date");

                entity.HasOne(d => d.MailListNavigation)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.MailList)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.Property(e => e.OwnerEmail)
                    .IsRequired()
                    .IsUnicode(false);
            });
        }
    }
}
