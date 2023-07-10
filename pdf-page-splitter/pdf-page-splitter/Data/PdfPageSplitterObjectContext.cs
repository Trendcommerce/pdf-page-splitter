using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using pdf_page_splitter.Data.Domain;
using System;
using System.IO;

namespace pdf_page_splitter.Data
{
    public class PdfPageSplitterObjectContext : DbContext
    {
        public PdfPageSplitterObjectContext(DbContextOptions<PdfPageSplitterObjectContext> options) : base(options)
        {
        }

        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<SplittedFile> SplittedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UploadedFile>().ToTable("UploadedFile");
            modelBuilder.Entity<SplittedFile>().ToTable("SplittedFile");
        }

        #region For Add-Migration

        public class PdfPageSplitterObjectContextFactory : IDesignTimeDbContextFactory<PdfPageSplitterObjectContext>
        {
            public PdfPageSplitterObjectContext CreateDbContext(string[] args)
            {
                var databaseFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
                if (!Directory.Exists(databaseFolderPath))
                {
                    Directory.CreateDirectory(databaseFolderPath);
                }
                var optionsBuilder = new DbContextOptionsBuilder<PdfPageSplitterObjectContext>();
                optionsBuilder.UseSqlite($"DataSource={Path.Combine(databaseFolderPath, "PdfPageSplitter.db")}");

                return new PdfPageSplitterObjectContext(optionsBuilder.Options);
            }
        }

        #endregion
    }
}
