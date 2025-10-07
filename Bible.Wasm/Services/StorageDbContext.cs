using Microsoft.EntityFrameworkCore;

namespace Bible.Wasm.Services
{
    public class StorageDbContext : DbContext
    {
        public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options) { }

        public DbSet<StorageItem> StorageItems { get; set; } = null!;
    }

    public class StorageItem
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}