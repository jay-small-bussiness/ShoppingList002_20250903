using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace ShoppingList002.Models.DbModels
{
    [Table("CandidateListItem")]

    public class CandidateListItemDbModel
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int ItemId { get; set; }

        [SQLite.NotNull]
        public int CategoryId { get; set; }

        [SQLite.NotNull]
        public string Name { get; set; }

        public string Detail { get; set; }

        [SQLite.NotNull]
        public int DisplaySeq { get; set; }

        // 1.1.0 追加カラム
        public string? Kana { get; set; }
        public string? SearchKana { get; set; }

        [SQLite.NotNull]
        public int DeleteFlg { get; set; }

        [SQLite.NotNull]
        public DateTimeOffset UpdatedAt { get; set; }

        [SQLite.NotNull]
        public int IsSynced { get; set; }
    }
}
