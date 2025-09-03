using SQLite;

namespace ShoppingList002.Models.DbModels
{
    [Table("AppSetting")]
    public class AppSettingDbModel
    {
        [PrimaryKey]
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
