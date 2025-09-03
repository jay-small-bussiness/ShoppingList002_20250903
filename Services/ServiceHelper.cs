using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Services
{
    public static class ServiceHelper
    {
        public static IServiceProvider Services { get; set; }

        public static T GetService<T>() where T : class
        {
            return Services.GetService(typeof(T)) as T;
        }

        public static T GetRequiredService<T>() where T : class
        {
            return Services.GetRequiredService(typeof(T)) as T;
        }
    }
}
