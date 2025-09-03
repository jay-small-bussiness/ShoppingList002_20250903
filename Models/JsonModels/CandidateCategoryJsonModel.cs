using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Models.JsonModels
{
    public class CandidateCategoryJsonModel
    {
        public int CandidateListId { get; set; }
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
        public int ColorId { get; set; }
        public string IconName { get; set; }
        public List<CandidateListItemJsonModel> CandidateListItems { get; set; }
    }

}
