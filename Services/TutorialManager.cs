using ShoppingList002.Models.UiModels;

namespace ShoppingList002.Services
{
    public class TutorialManager
    {
        private readonly List<TutorialStep> _steps;
        private int _currentIndex = 0;

        public TutorialManager(List<TutorialStep> steps)
        {
            _steps = steps;
        }

        public void Start()
        {
            ShowCurrentStep();
        }

        private void ShowCurrentStep()
        {
            if (_currentIndex >= _steps.Count)
                return;

            var step = _steps[_currentIndex];

            if (Preferences.Get(step.PreferenceKey, false))
            {
                _currentIndex++;
                ShowCurrentStep();
                return;
            }
            //// ★ ここでフラグを付ける（表示した時点で一回限りにする）
            //Preferences.Set(step.PreferenceKey, true);

            if (step.OverlayGrid != null)
            {
                var label = step.OverlayGrid.FindByName<Label>("TutorialTextLabel");
                var frame = step.OverlayGrid.FindByName<Frame>("TutorialFrame");

                if (label != null)
                {
                    label.Text = step.TutorialString ?? "";
                    label.FontSize = step.FontSize;
                    label.TextColor = step.TextColor;

                    // アニメーション
                    ShowTextAnimation(frame, step.VerticalAlignment);
                }

                if (frame != null)
                {
                    frame.HorizontalOptions = step.HorizontalAlignment;
                    frame.VerticalOptions = step.VerticalAlignment;
                }

                step.OverlayGrid.IsVisible = true;
            }
        }

        public void CompleteCurrentStep()
        {
            if (_currentIndex >= _steps.Count)
                return;

            var step = _steps[_currentIndex];

            Preferences.Set(step.PreferenceKey, true);
            step.OverlayGrid.IsVisible = false;

            step.OnCompleted?.Invoke();

            _currentIndex++;
            ShowCurrentStep();
        }

        private async void ShowTextAnimation(View target, LayoutOptions verticalAlignment)
        {
            double startOffsetY = 0;

            if (verticalAlignment.Alignment == LayoutAlignment.End)
            {
                startOffsetY = -150; // 上から降りる
            }
            else if (verticalAlignment.Alignment == LayoutAlignment.Start)
            {
                startOffsetY = +150; // 下から上に出る
            }
            else
            {
                startOffsetY = 0; // 中央はフェード
            }

            target.TranslationY = startOffsetY;
            target.Opacity = 0;

            if (startOffsetY != 0)
            {
                await Task.WhenAll(
                    target.FadeTo(1, 500),
                    target.TranslateTo(0, 0, 800)
                );
            }
            else
            {
                await target.FadeTo(1, 500);
            }
        }
    }

}
