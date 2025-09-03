using SQLite;
using System.Diagnostics.CodeAnalysis;
// CandidateCategoryDbModel.cs（旧：Page）
namespace ShoppingList002.Models.DbModels
{
    [Table("CandidateCategory")]

    public class CandidateCategoryDbModel
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int CategoryId { get; set; }

        [SQLite.NotNull]
        public string Title { get; set; }

        [SQLite.NotNull]
        public int DisplayOrder { get; set; }

        [SQLite.NotNull]
        public int ColorId { get; set; }

        [SQLite.NotNull]
        public string IconName { get; set; }

        [SQLite.NotNull]
        public int DeleteFlg { get; set; }

        [SQLite.NotNull]
        public DateTimeOffset UpdatedAt { get; set; }

        [SQLite.NotNull]
        public int IsSynced { get; set; }
    }
}
