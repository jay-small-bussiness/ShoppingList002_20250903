using System.Globalization;

namespace ShoppingList002.Services.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = (bool)value;
            string actionType = parameter as string ?? "";

            if (!isSelected)
                return Colors.LightGray;

            return actionType switch
            {
                "リスト追加" => new Color(0.7f, 1.0f, 0.7f),
                "リスト削除" => new Color(1.0f, 0.7f, 0.7f),
                "購入" => new Color(0.7f, 0.7f, 1.0f),
                _ => Colors.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
