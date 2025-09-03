using ShoppingList002.Models.DbModels;
using ShoppingList002.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;

namespace ShoppingList002.Services
{
    public interface IDatabaseService
    {
        Task<SQLiteAsyncConnection> GetConnectionAsync();
        Task<int> ExecuteAsync(string sql, params object[] args);
        Task InitializeDatabaseAsync();
        Task<bool> CreateAllTablesAsync();
        Task DropAllTablesAsync();
        Task InsertInitialDataAsync();
        Task DatabaseTest();
        Task<string?> GetSettingAsync(string key);
        Task SetSettingAsync(string key, string value);
        Task<List<CandidateCategoryDbModel>> GetCandidateCategoryDbModels();
        Task<List<CandidateListItemDbModel>> GetCandidateListItemDbModelsByCategoryId(int categoryId);
        Task<List<CandidateListItemDbModel>> GetCandidateItemsByCategoryIdAsync(int categoryId);
        Task DeleteCandidateCategoryAsync(int categoryId);
        Task<List<string>> GetActiveShoppingItemNamesByCategoryIdAsync(int categoryId);
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, params object[] args) where T : new();
        Task<int> InsertAsync<T>(T item) where T : new();
        Task<int> UpdateAsync<T>(T item) where T : new();
        Task<T?> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : new();

        //Task<int> DeleteAsync<T>(T item) where T : new(); // 必要なら
        Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new();
        AsyncTableQuery<T> GetTable<T>() where T : new();
        Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : new();
        Task<T?> GetAsync<T>(int id) where T : new();
        Task<T?> GetCandidateCategoryAsync<T>(int id) where T : new();
        Task<Dictionary<int, ColorSet>> GetColorSetMapAsync();
        Task<List<T>> GetAllAsync<T>() where T : new();
        Task DeleteExpiredRecordsAsync();
        Task<string> GetCurrentDbVersionAsync();
        Task SetVersionAsync(string version);
        Task CreateTableAsync<T>() where T : new();
        Task InsertOrReplaceAsync<T>(T item) where T : new();
    }

}
