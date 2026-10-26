using LearnJP.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserDetail> UserDetails => Set<UserDetail>();
    public DbSet<UserLearn> UserLearns => Set<UserLearn>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Hiragana> Hiraganas => Set<Hiragana>();
    public DbSet<Katakana> Katakanas => Set<Katakana>();
    public DbSet<MinnanoVocaN5> MinnanoVocaN5s => Set<MinnanoVocaN5>();
    public DbSet<MinnanoVocaN4> MinnanoVocaN4s => Set<MinnanoVocaN4>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<HistoryVocab> HistoryVocabs => Set<HistoryVocab>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<WrongVocab> WrongVocabs => Set<WrongVocab>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- users ----------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();

            // Lưu enum Role dưới dạng chuỗi khớp enum MySQL 'Student'/'Admin'.
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(20);
        });

        // ---------- userdetails (1-1 với users, UserId vừa PK vừa FK) ----------
        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.HasOne(d => d.User)
                  .WithOne(u => u.UserDetail)
                  .HasForeignKey<UserDetail>(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(d => d.Gender).HasConversion<string>().HasMaxLength(10);
        });

        // ---------- user_learn (1 dòng duy nhất/user theo xác nhận) ----------
        modelBuilder.Entity<UserLearn>(entity =>
        {
            entity.HasIndex(l => l.UserId).IsUnique();

            entity.HasOne(l => l.User)
                  .WithOne(u => u.UserLearn)
                  .HasForeignKey<UserLearn>(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- otp_verifications ----------
        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.HasIndex(o => new { o.Email, o.Purpose });
            entity.Property(o => o.Purpose).HasConversion<string>().HasMaxLength(20);
        });

        // ---------- password_reset_tokens ----------
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- lesson_progress (bảng mới) ----------
        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasIndex(l => new { l.UserId, l.VocaLevel, l.LessonNumber }).IsUnique();
            entity.Property(l => l.VocaLevel).HasConversion<string>().HasMaxLength(10);

            entity.HasOne(l => l.User)
                  .WithMany()
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- history_vocab (bảng mới) ----------
        modelBuilder.Entity<HistoryVocab>(entity =>
        {
            entity.HasIndex(h => new { h.UserId, h.VocaLevel, h.WordId }).IsUnique();
            entity.Property(h => h.VocaLevel).HasConversion<string>().HasMaxLength(10);

            entity.HasOne(h => h.User)
                  .WithMany()
                  .HasForeignKey(h => h.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- todo_items (bảng mới) ----------
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasIndex(t => new { t.UserId, t.DueDate });
            entity.Property(t => t.Priority);

            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- wrong_vocab (bảng mới — sổ tay lỗi sai) ----------
        modelBuilder.Entity<WrongVocab>(entity =>
        {
            entity.HasIndex(w => new { w.UserId, w.VocaLevel, w.WordId }).IsUnique();
            entity.Property(w => w.VocaLevel);

            entity.HasOne(w => w.User)
                  .WithMany()
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

    }
}