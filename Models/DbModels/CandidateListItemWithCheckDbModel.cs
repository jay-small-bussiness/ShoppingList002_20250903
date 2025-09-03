namespace ShoppingList002.Models.DbModels
{
    public class CandidateListItemWithCheckDbModel
    {
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string? Detail { get; set; }
        public int DisplaySeq { get; set; }
        public bool IsInShoppingList { get; set; }
    }

}
