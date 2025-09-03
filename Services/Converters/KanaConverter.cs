using System.Text;

namespace ShoppingList002.Services.Converter;

public static class KanaConverter
{
    // 1) ひらがな化（空白/記号除去・小書き統一・濁点は残す）
    public static string ToHiraganaLoose(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Normalize(NormalizationForm.FormKC);
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            char c = ch;
            if (c >= 'ァ' && c <= 'ヶ') c = (char)(c - 'ァ' + 'ぁ'); // カタ→ひら
            if (c == 'ヴ') c = 'ゔ';
            if ((c >= 'ぁ' && c <= 'ゟ') || c == 'ー') sb.Append(c); // ひら or 長音のみ
        }
        var res = sb.ToString();
        // 小書きを並字寄せ
        res = res.Replace("ぁ", "あ").Replace("ぃ", "い").Replace("ぅ", "う").Replace("ぇ", "え").Replace("ぉ", "お")
                 .Replace("ゃ", "や").Replace("ゅ", "ゆ").Replace("ょ", "よ").Replace("ゎ", "わ");
        // 空白除去（全角含む）
        res = res.Replace(" ", "").Replace("　", "");
        return res;
    }

    // 2) 読みの“ゆらぎ”候補（最小セット）
    private static readonly Dictionary<string, string[]> Variants = new()
    {
        { "豚", new[]{ "ぶた", "とん" } },
        { "鶏", new[]{ "にわとり", "とり" } },
        { "鳥", new[]{ "にわとり", "とり" } },
        // 必要に応じて少しずつ追加
    };

    private static readonly Dictionary<string, string[]> PhraseVariants = new()
    {
        { "大葉", new[]{ "おおば", "しそ" } },
        { "胡椒", new[]{ "こしょう" } },
        { "絹ごし", new[]{ "きぬごし" } },
    };

    // 3) テキスト→キー群（登録/検索の両方で利用）
    //    ※ 順序維持：フレーズ展開 → 漢字ゆらぎ → （最後に）素のひらがな
    public static List<string> GenerateKanaKeysSimple(string text)
    {
        var list = new List<string>();
        var seen = new HashSet<string>();
        void add(string s) { if (!string.IsNullOrEmpty(s) && seen.Add(s)) list.Add(s); }

        // 先にフレーズ（大葉/しそ 等）
        foreach (var (phrase, vars) in PhraseVariants)
        {
            if (text.Contains(phrase))
            {
                foreach (var v in vars)
                    add(ToHiraganaLoose(text.Replace(phrase, v)));
            }
        }

        // 漢字のゆらぎ（豚→ぶた/とん 等）
        foreach (var (kanji, vars) in Variants)
        {
            if (text.Contains(kanji))
            {
                foreach (var v in vars)
                    add(ToHiraganaLoose(text.Replace(kanji, v)));
            }
        }

        // 参考として“素のひらがな”（漢字は落ちやすい）を最後に
        add(ToHiraganaLoose(text)); // 例: 「豚こま」→「こま」

        return list;
    }
}
