using SQLite;

namespace ShoppingList002.Models.DbModels
{
    [Table("ShoppingListItem")]
    public class ShoppingListItemDbModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public int ItemId { get; set; }

        [NotNull]
        public string Name { get; set; } = string.Empty;

        public string? Detail { get; set; }

        [NotNull]
        public DateTimeOffset AddedDate { get; set; }      // 初回登録日時（INSERT時）

        [NotNull]
        public DateTimeOffset UpdatedDate { get; set; }    // 最終更新日時（状態変更時）

        public string? Status { get; set; }          // null=現役, "済"=買い物完了, "C"=キャンセル
    }
}
