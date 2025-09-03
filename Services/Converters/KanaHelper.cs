using System.Text;
using System.Text.RegularExpressions;

namespace ShoppingList002.Services.Converters
{
    public static class KanaHelper
    {
        public static string ToHiragana(string input)
        {
            string katakana;
#if ANDROID
            // Android専用ブリッジ（null来たらフォールバック）
            katakana = ShoppingList002.Platforms.Android.AndroidKanaBridge.ToKatakana(input) ?? input;
#else
            katakana = input; // 他プラットフォームはそのまま
#endif
            return KatakanaToHiragana(katakana);
        }

        // Strict用：長音/小書きだけ正規化（濁音は保持）
        public static string ToSearchKana(string input)
        {
            // 必ず s を作って最後に return する
            var s = ToHiragana(input).Normalize(NormalizationForm.FormKC);

            // 小書き → 通常
            s = s.Replace("ぁ", "あ").Replace("ぃ", "い").Replace("ぅ", "う").Replace("ぇ", "え").Replace("ぉ", "お")
                 .Replace("ゃ", "や").Replace("ゅ", "ゆ").Replace("ょ", "よ").Replace("ゎ", "わ");

            // 長音削除
            s = s.Replace("ー", "");

            // 合成濁点除去（保険）
            s = RemoveCombiningMarks(s);

            // ひらがな以外削除
            s = Regex.Replace(s, @"[^ぁ-ん]", "");
            // 最後に FormC で正規化
            return s.Normalize(NormalizationForm.FormC);
            //return s;
        }

        // Loosen用：Strictの結果に濁音/半濁音→清音を追加
        public static string ToSearchKanaLoosen(string input)
        {
            var s = ToSearchKana(input);

            s = s.Replace("が", "か").Replace("ぎ", "き").Replace("ぐ", "く").Replace("げ", "け").Replace("ご", "こ")
                 .Replace("ざ", "さ").Replace("じ", "し").Replace("ず", "す").Replace("ぜ", "せ").Replace("ぞ", "そ")
                 .Replace("だ", "た").Replace("ぢ", "ち").Replace("づ", "つ").Replace("で", "て").Replace("ど", "と")
                 .Replace("ば", "は").Replace("び", "ひ").Replace("ぶ", "ふ").Replace("べ", "へ").Replace("ぼ", "ほ")
                 .Replace("ぱ", "は").Replace("ぴ", "ひ").Replace("ぷ", "ふ").Replace("ぺ", "へ").Replace("ぽ", "ほ")
                 .Replace("ゔ", "う");

            return s;
        }
        // KanaHelper.cs に追加
        public static string ToKana(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // ToHiraganaでカタカナ→ひらがなに統一
            var s = ToHiragana(input).Normalize(NormalizationForm.FormKC);

            // 小書きゃゅょ等を通常形に
            s = s.Replace("ぁ", "あ").Replace("ぃ", "い").Replace("ぅ", "う")
                 .Replace("ぇ", "え").Replace("ぉ", "お")
                 .Replace("ゃ", "や").Replace("ゅ", "ゆ").Replace("ょ", "よ")
                 .Replace("ゎ", "わ");

            // 空白や全角スペースを削除
            s = s.Replace(" ", "").Replace("　", "");

            return s;
        }

        private static string RemoveCombiningMarks(string s)
        {
            var nfd = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(nfd.Length);
            foreach (var ch in nfd)
            {
                if (ch == '\u3099' || ch == '\u309A') continue; // 濁点・半濁点
                sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string KatakanaToHiragana(string katakana)
        {
            var sb = new StringBuilder(katakana.Length);
            foreach (var c in katakana)
            {
                if (c >= 'ァ' && c <= 'ン') sb.Append((char)(c - 'ァ' + 'ぁ'));
                else if (c == 'ヵ') sb.Append('か');
                else if (c == 'ヶ') sb.Append('け');
                else sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
