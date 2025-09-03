using SQLite;
using System;

namespace ShoppingList002.Models.DbModels
{
    [Table("ActivityLog")]
    public class ActivityLogDbModel
    {
        [PrimaryKey, AutoIncrement]
        public int LogId { get; set; }

        public string ActionType { get; set; }     // "追加", "削除", "購入", "カテゴリ作成" など
        public int? ItemId { get; set; }           // アイテムID（null許容）
        public string ItemName { get; set; }       // アイテム名（表示用）
        public string CategoryName { get; set; }   // カテゴリ名（表示用）
        public string Description { get; set; }    // 表示テキストそのもの（整形済でもOK）
        public string Actor { get; set; }          // 操作主（じゅんちゃん、妻、娘、など）
        public DateTimeOffset Timestamp { get; set; } // 操作時刻
    }

}
