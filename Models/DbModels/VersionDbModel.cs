using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace ShoppingList002.Models.DbModels
{
    [Table("Version")]

    public class VersionDbModel
    {
        [SQLite.PrimaryKey]
        public int VersionId { get; set; }

        [SQLite.NotNull]
        public string DbVersion { get; set; }

        [SQLite.NotNull]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}