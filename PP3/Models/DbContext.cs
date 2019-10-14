using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PP3.Models
{
    public class PatentDbContext : DbContext
    {
        public PatentDbContext(DbContextOptions<PatentDbContext> options) : base(options) { }
        public DbSet<Patent> Patents { get; set; }
    }
}
