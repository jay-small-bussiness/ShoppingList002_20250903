using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Models.UiModels
{
    public class TutorialStep
    {
        public string PreferenceKey { get; set; }
        public string TutorialString { get; set; }
        public double FontSize { get; set; } = 16;
        public Color TextColor { get; set; } = Colors.Black;
        public LayoutOptions HorizontalAlignment { get; set; } = LayoutOptions.Start;
        public LayoutOptions VerticalAlignment { get; set; } = LayoutOptions.End;
        public Grid OverlayGrid { get; set; }
        public Action OnCompleted { get; set; }
    }

}
