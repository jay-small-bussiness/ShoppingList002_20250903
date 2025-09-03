using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Services
{
    public interface ISpeechToTextService
    {
        Task<string> RecognizeAsync();
        //Task<string> CheckPermissions();
    }

}
