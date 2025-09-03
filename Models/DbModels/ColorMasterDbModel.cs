using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace ShoppingList002.Models.DbModels
{
    [Table("ColorMaster")]
    public class ColorMasterDbModel
    {
        [PrimaryKey]
        public int ColorId { get; set; }

        [SQLite.NotNull]
        public string UnSelectedHexCode { get; set; }

        [SQLite.NotNull]
        public string SelectedHexCode { get; set; }

        [SQLite.NotNull]
        public string PreSelectedHexCode { get; set; }

        public string Name { get; set; }
    }

}
