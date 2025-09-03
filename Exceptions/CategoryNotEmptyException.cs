using System;
using System.Collections.Generic;

namespace ShoppingList002.Exceptions
{
    public class CategoryNotEmptyException : Exception
    {
        public List<string> AllItems { get; }
        public List<string> ActiveShoppingItems { get; }

        public CategoryNotEmptyException(List<string> allItems, List<string> activeItems)
            : base("このカテゴリにはまだアイテムが残っています。")
        {
            AllItems = allItems;
            ActiveShoppingItems = activeItems;
        }
    }
}
