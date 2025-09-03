using System.Globalization;

namespace ShoppingList002.Services.Converters
{
    public class ActionTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var actionType = value as string;
            return actionType switch
            {
                "リスト追加" => Colors.Green,
                "リスト削除" => Colors.Red,
                "購入" => Colors.Blue,
                _ => Colors.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
