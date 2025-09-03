using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Services.Converters
{
    public class ActionTypeToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var actionType = value as string;

            return actionType switch
            {
                "リスト追加" => new Color(0.9f, 1.0f, 0.9f),    // 薄緑
                "リスト削除" => new Color(1.0f, 0.9f, 0.9f),    // 薄赤
                "購入" => new Color(0.9f, 0.9f, 1.0f),    // 薄青
                _ => Colors.LightGray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
