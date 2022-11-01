using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace ServerProject.Database
{
    // Class DB object 
    public class AppDbContext : DbContext
    {
        protected AppDbContext(DbContextOptions options) : base(options) { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : this((DbContextOptions)options)
        {
        }

        public virtual DbSet<ZipFile> ZipFiles { get; set; }
    }

    public class ZipFile
    {
        [Key]
        public string Id { get; set; }
        public string FileName { get; set; }
        public string ContentStructure { get; set; }

        public ZipFile(string fileName, string contentStructure)
        {
            this.Id = Guid.NewGuid().ToString();
            this.FileName = fileName;
            this.ContentStructure = contentStructure;
        }
    }
}
