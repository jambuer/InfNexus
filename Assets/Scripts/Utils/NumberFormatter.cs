// Scripts/Utils/NumberFormatter.cs

using System;
using System.Globalization; // Bu satır EKLENMİŞ olmalı

public static class NumberFormatter
{
    // en-US kültürü (1,234,567.89 formatını garantiler)
    private static readonly CultureInfo formattingCulture = new CultureInfo("en-US");

    // İstenen son ekler (Milyar'dan başlıyor)
    private static readonly string[] Suffixes = {
        "B", "T", "Q", "QQ", "Qq", "s", "S", "O", "N", "d", 
        "U", "D", "Td", "Qd", "Qid", "Sd", "Spd", "Od", "Nd", "V"
        // Gerektiğinde burayı uzatabilirsiniz
    };

    /// <summary>
    /// Verilen double sayıyı istenen formata (virgüllü veya kısaltmalı) çevirir.
    /// </summary>
    public static string FormatNumber(double number)
    {
        // 1 Milyar (1e9) altındaki sayılar için
        if (Math.Abs(number) < 1e9)
        {
            // "N0" formatı grup ayraçları (virgül) ekler ve ondalık göstermez
            // Örn: 123,456,789
            return number.ToString("N0", formattingCulture);
        }

        // 1 Milyar (1e9) ve üzeri için kısaltmalı (Suffix) format
        string sign = number < 0 ? "-" : "";
        double absNum = Math.Abs(number);

        // Hangi son eki kullanacağımızı bulalım
        // 1e9 (Milyar) = 9
        // 1e12 (Trilyon) = 12
        int exponent = (int)Math.Floor(Math.Log10(absNum)); // Sayının üssü (kaç haneli olduğu)
        
        // (9-9)/3 = 0 -> Suffixes[0] = "B"
        // (12-9)/3 = 1 -> Suffixes[1] = "T"
        // (15-9)/3 = 2 -> Suffixes[2] = "Q"
        int suffixIndex = (exponent - 9) / 3;

        if (suffixIndex < 0) suffixIndex = 0; // 1e9'dan küçükse (ki yukarıda yakaladık ama garanti)
        if (suffixIndex >= Suffixes.Length)
        {
            suffixIndex = Suffixes.Length - 1; // Listemizdeki son eki kullan
        }

        string suffix = Suffixes[suffixIndex];
        
        // Sayıyı doğru üsse böl (1e9, 1e12, 1e15...)
        double divisor = Math.Pow(10, 9 + (suffixIndex * 3));
        double formattedValue = number / divisor;

        // "N2" formatı hem grup ayracı (virgül) hem de 2 ondalık (nokta) ekler
        // Örn: 1,234.56 T veya 564,123,123.43 QQQ (Senin örneğine en yakın format)
        return formattedValue.ToString("N2", formattingCulture) + " " + suffix;
    }
}