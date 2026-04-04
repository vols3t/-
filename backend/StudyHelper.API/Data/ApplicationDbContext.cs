using Microsoft.EntityFrameworkCore;
using StudyHelper.API.Models;

namespace StudyHelper.API.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Question> Questions { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}