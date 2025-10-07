using Bible.Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Bible.Wasm.Services
{
    public class DatabaseStorageService : StorageService
    {
        private readonly IDbContextFactory<StorageDbContext> _dbFactory;

        public DatabaseStorageService(IDbContextFactory<StorageDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public override async Task<string> GetItemAsync(string key)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var item = await db.StorageItems.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key);
            return item?.Value ?? string.Empty;
        }

        public override async Task SetItemAsync(string key, string value)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var item = await db.StorageItems.FirstOrDefaultAsync(x => x.Key == key);
            if (item == null)
            {
                db.StorageItems.Add(new StorageItem { Key = key, Value = value });
            }
            else
            {
                item.Value = value;
                db.StorageItems.Update(item);
            }

            await db.SaveChangesAsync();
        }
    }
}
