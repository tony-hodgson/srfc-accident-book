using Microsoft.EntityFrameworkCore;
using AccidentBook.API.Models;

namespace AccidentBook.API.Data;

public class AccidentDbContext : DbContext
{
    public AccidentDbContext(DbContextOptions<AccidentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Accident> Accidents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Accident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DateOfAccident).IsRequired();
            entity.Property(e => e.TimeOfAccident).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Opposition).HasMaxLength(200);
            entity.Property(e => e.PersonInvolved).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Age);
            entity.Property(e => e.PersonReporting).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.NatureOfInjury).HasMaxLength(500);
            entity.Property(e => e.TreatmentGiven).HasMaxLength(1000);
            entity.Property(e => e.ActionTaken).HasMaxLength(1000);
            entity.Property(e => e.Witnesses).HasMaxLength(500);
        });
    }
}

