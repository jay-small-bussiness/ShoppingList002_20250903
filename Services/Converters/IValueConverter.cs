using System.Globalization;
using ShoppingList002.ViewModels;

namespace ShoppingList002.Services.Converters
{
    public class SelectedItemEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var collectionView = parameter as CollectionView;
            //return collectionView?.SelectedItem == value;
            if (parameter is EditCategoryPopupViewModel vm && value is ColorUiModel color)
            {
                return ReferenceEquals(vm.SelectedColor, color);
            }
            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
