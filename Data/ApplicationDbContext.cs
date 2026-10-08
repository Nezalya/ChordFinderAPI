using ChordFinderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChordFinderAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Acorde> Acordes { get; set; }
    public DbSet<Escala> Escalas { get; set; }
}
