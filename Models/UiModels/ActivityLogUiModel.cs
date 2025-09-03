using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Models.UiModels
{
    public class ActivityLogUiModel
    {
        public string Timestamp { get; set; }    // "2025/06/18 14:22"
        public string ActionType { get; set; }       // "リスト追加"
        public string ItemName { get; set; }     // "ピーマン"
        public string CategoryName { get; set; }     // "野菜"
        public string Actor { get; set; }        // "じゅんちゃん"
    }
}
