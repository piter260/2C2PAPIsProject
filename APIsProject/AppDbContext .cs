using APIsProject;
using System.Collections.Generic;
using APIsProject.Models;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace APIsProject
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public Microsoft.EntityFrameworkCore.DbSet<TestTable> TestTable { get; set; }
    }
}



