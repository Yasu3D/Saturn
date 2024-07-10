using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame
{
public static class SaturnSorting
{
    private enum CharType
    {
        // Values provide the overall sort order.
        Number = 0,
        Letter = 1,
        Kana = 2,
        Ideograph = 3,
        Other = 4,
    }

    /// <summary>
    /// Group a list by the first character of some key. (For instance, a song name or artist.)
    /// A "rubi" name can also be provided, a la RUBI_TITLE, which provides additional sorting info for Japanese names
    /// by providing the hirigana reading of the name.
    /// </summary>
    /// <param name="source">The source list to be grouped.</param>
    /// <param name="nameAndRubiSelector">Given a list entry, provides a tuple of the name and rubi name for the entry.
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [NotNull]
    public static IEnumerable<IGrouping<string, T>> GroupByJpEnName<T>([NotNull] this IEnumerable<T> source,
        Func<T, (string, string)> nameAndRubiSelector)
    {
        return source
            .GroupBy(item =>
            {
                (string name, string rubi) = nameAndRubiSelector(item);
                string usableName = NormalizeFullwidthHalfwidth(rubi ?? name);
                return FirstCharInfo(usableName);
            })
            // Order by the tuple
            .OrderBy(group => group.Key.Item1)
            .ThenBy(group => group.Key.Item2, StringComparer.InvariantCulture)
            // Trick to get the group key as just the string.
            .SelectMany(group => group.GroupBy(_ => group.Key.Item2));
    }

    /// <summary>
    /// Sort a list by the some key. (For instance, a song name or artist.)
    /// A "rubi" name can also be provided, a la RUBI_TITLE, which provides additional sorting info for Japanese names
    /// by providing the hirigana reading of the name.
    /// </summary>
    /// <param name="source">The source list to be sorted.</param>
    /// <param name="nameAndRubiSelector">Given a list entry, provides a tuple of the name and rubi name for the entry.
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [NotNull]
    public static IEnumerable<T> OrderByJpEnName<T>([NotNull] this IEnumerable<T> source,
        Func<T, (string, string)> nameAndRubiSelector)
    {
        return source
            .Select(value =>
            {
                (string name, string rubi) = nameAndRubiSelector(value);
                // Could consider using a library like Kawazu to the reading for kanji names without rubi, but it might
                // not be very accruate.
                string sortKey = NormalizeFullwidthHalfwidth(rubi ?? name);
                (CharType startCharType, string startCharName) = FirstCharInfo(sortKey);
                return new { value, sortKey, startCharType, startCharName };
            })
            .OrderBy(item => item.startCharType)
            .ThenBy(item => item.startCharName, StringComparer.InvariantCulture)
            // Note: StringComparer.InvariantCulture seems to gracefully sort katakana and hiragana interchangeably.
            .ThenBy(item => item.sortKey, StringComparer.InvariantCulture)
            .Select(item => item.value);
    }

    private static (CharType, string) FirstCharInfo([NotNull] string key)
    {
        if (key.Length == 0)
            return (CharType.Other, "[empty]");

        // Note: it's safe to just take the first char since C# internally uses UTF-16.
        // So all CJK ideographs (kanji) and all hiragana and katakana are only 1 char.
        return key[0] switch
        {
            // ReSharper disable PatternIsRedundant
            // Don't know why Rider is being stupid here.

            >= '0' and <= '9' => (CharType.Number, "0-9"),

            >= 'a' and <= 'z' => (CharType.Letter, char.ToUpper(key[0]).ToString()),
            >= 'A' and <= 'Z' => (CharType.Letter, key[0].ToString()),

            // https://www.unicode.org/charts/PDF/U3040.pdf
            >= 'ぁ' and <= 'お' => (CharType.Kana, "あ～"),
            >= 'か' and <= 'ご' or >= 'ゕ' and <= 'ゖ' => (CharType.Kana, "か～"),
            >= 'さ' and <= 'ぞ' => (CharType.Kana, "さ～"),
            >= 'た' and <= 'ど' => (CharType.Kana, "た～"),
            >= 'な' and <= 'の' => (CharType.Kana, "な～"),
            >= 'は' and <= 'ぽ' => (CharType.Kana, "は～"),
            >= 'ま' and <= 'も' => (CharType.Kana, "ま～"),
            >= 'ゃ' and <= 'よ' => (CharType.Kana, "や～"),
            >= 'ら' and <= 'ろ' => (CharType.Kana, "ら～"),
            // Includes ん which doesn't really make sense but idk what else to do.
            >= 'ゎ' and <= 'ゔ' => (CharType.Kana, "わ～"),

            // https://www.unicode.org/charts/PDF/U30A0.pdf
            >= 'ァ' and <= 'オ' => (CharType.Kana, "あ～"),
            >= 'カ' and <= 'ゴ' or >= 'ヵ' and <= 'ヶ' => (CharType.Kana, "か～"),
            >= 'サ' and <= 'ゾ' => (CharType.Kana, "さ～"),
            >= 'タ' and <= 'ド' => (CharType.Kana, "た～"),
            >= 'ナ' and <= 'ノ' => (CharType.Kana, "な～"),
            >= 'ハ' and <= 'ポ' => (CharType.Kana, "は～"),
            >= 'マ' and <= 'モ' => (CharType.Kana, "ま～"),
            >= 'ャ' and <= 'ヨ' => (CharType.Kana, "や～"),
            >= 'ラ' and <= 'ロ' => (CharType.Kana, "ら～"),
            // Includes ン which doesn't really make sense but idk what else to do.
            // Also includes ヷ, ヸ, ヹ, ヺ which are probably rare.
            >= 'ヮ' and <= 'ヴ' or >= 'ヷ' and <= 'ヺ' => (CharType.Kana, "わ～"),

            // Capture the whole ideographic CJK block (plus extension A)
            // https://www.unicode.org/charts/PDF/U4E00.pdf
            // https://www.unicode.org/charts/PDF/U3400.pdf
            >= '\u4E00' and <= '\u9FFF' or >= '\u3400' and <= '\u4DBF' =>
                (CharType.Ideograph, "漢字"),

            _ => (CharType.Other, "@*&+"),

            // ReSharper restore PatternIsRedundant
        };
    }

    private const string FullwidthKatakana =
        "。「」、・ヲァィゥェォャュョッーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン\u3099\u309A";

    // Convert fullwidth latin letters to (halfwidth) ascii, and convert halfwidth katakana to standard fullwidth.
    [NotNull]
    public static string NormalizeFullwidthHalfwidth([NotNull] string input)
    {
        return new(input.Select(c => c switch
        {
            // Fullwidth latin chars are in the range FF01 to FF5E.
            // It is a copy of 0021 to 007E but offset by FEE0. (That is, 0021 -> FF01.)
            // See https://www.unicode.org/charts/PDF/U0000.pdf and https://www.unicode.org/charts/PDF/UFF00.pdf
            >= '\uFF01' /*！*/ and <= '\uFF5E' /*～*/ => (char)(c - 0xFEE0),

            // The fullwidth katakana codepoints are laid out differently than the halfwidth ones.
            // FullwidthKatakana has the fullwidth codepoints in the order they are found in the halfwidth block.
            // See https://www.unicode.org/charts/PDF/U30A0.pdf and https://www.unicode.org/charts/PDF/UFF00.pdf
            >= '｡' and <= '\uFF9F' => FullwidthKatakana[c - '｡'],

            // Note that Unicode does not have halfwidth hiragana.

            // Do not modify any other character.
            _ => c,
        }).ToArray());
    }
}
}
