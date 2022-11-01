using ServerProject.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerProject.Common
{
    // helper for implement in-memory DB functions
    public static class DatabaseHelper
    {
        private static AppDbContext _appDb;

        public static void Init(AppDbContext appDbContext)
        {
            _appDb = appDbContext;
        }

        public static async Task<KeyValuePair<string, bool>> SaveZipStructure(string fileName, string dataStructure)
        {
            try
            {
                ZipFile file = new ZipFile(fileName, dataStructure);
                await Task.Run(() => SaveToDb(file));

                return new KeyValuePair<string, bool>(file.Id.ToString(), true);
            }
            catch (Exception e)
            {
                return new KeyValuePair<string, bool>(e.Message, false);
            }
        }

        private static async void SaveToDb(ZipFile file)
        {
            _appDb.ZipFiles.Add(file);
            await Task.Run(() => _appDb.SaveChanges());
        }
    }
}
