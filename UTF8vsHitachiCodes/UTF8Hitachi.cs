using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTF8vsHitachiCodes {
   static public class UTF8Hitachi {

      static UTF8Hitachi() {
         // Build language dictionaries
         HitachiToUTF8 = new Dictionary<int, string>();
         UTF8ToHitachi = new Dictionary<string, int>();

         for (int i = 0; i < Accent1Characters.Length; i++) {
            UTF8ToHitachi.Add(Accent1Characters[i], HitachiAccent1Characters[i]);
            HitachiToUTF8.Add(HitachiAccent1Characters[i], Accent1Characters[i]);
         }

         for (int i = 0; i < Accent2Characters.Length; i++) {
            UTF8ToHitachi.Add(Accent2Characters[i], HitachiAccent2Characters[i]);
            HitachiToUTF8.Add(HitachiAccent2Characters[i], Accent2Characters[i]);
         }

         for (int i = 0; i < GreekCharacters.Length; i++) {
            UTF8ToHitachi.Add(GreekCharacters[i], HitachiGreekCharacters[i]);
            HitachiToUTF8.Add(HitachiGreekCharacters[i], GreekCharacters[i]);
         }

         for (int i = 0; i < RussianCharacters.Length; i++) {
            UTF8ToHitachi.Add(RussianCharacters[i], HitachiRussianCharacters[i]);
            HitachiToUTF8.Add(HitachiRussianCharacters[i], RussianCharacters[i]);
         }

         for (int i = 0; i < DateTimeCharacters.Length; i++) {
            UTF8ToHitachi.Add(DateTimeCharacters[i], HitachiDateTimeCharacters[i]);
            HitachiToUTF8.Add(HitachiDateTimeCharacters[i], DateTimeCharacters[i]);
         }

         for (int i = 0; i < DateTimeCharactersSOP4.Length; i++) {
            //UTF8ToHitachi.Add(DateTimeCharacters[i], HitachiDateTimeCharacters[i]);
            HitachiToUTF8.Add(HitachiDateTimeCharactersSOP4[i], DateTimeCharactersSOP4[i]);
         }
      }

      // Multiple language translations for Legacy Printers
      static public Dictionary<int, string> HitachiToUTF8;
      static public Dictionary<string, int> UTF8ToHitachi;

      #region European Accent Characters

      static public string[] Accent1Characters = new string[] {
         "À",    "Á",    "Â",    "Ã",    "Ä",    "È",    "É",    "Ê",    "Ë",    "Ì",
         "Í",    "Î",    "Ï",    "Ò",    "Ó",    "Ô",    "Õ",    "Ö",    "Ù",    "Ú",
         "Û",    "Ü",    "Æ",    "Ç",    "Ñ",    "Œ",    "Å",    "Ø",    "£",    "€",
         "à",    "á",    "â",    "ã",    "ä",    "è",    "é",    "ê",    "ë",    "ì",
         "í",    "î",    "ï",    "ò",    "ó",    "ô",    "õ",    "ö",    "ù",    "ú",
         "û",    "ü",    "æ",    "ç",    "ñ",    "œ",    "ß",    "å",    "ø",
     };

      static int[] HitachiAccent1Characters = new int[] {
         0xF340, 0xF341, 0xF342, 0xF343, 0xF344, 0xF345, 0xF346, 0xF347, 0xF348, 0xF349,
         0xF34A, 0xF34B, 0xF34C, 0xF34D, 0xF34E, 0xF34F, 0xF350, 0xF351, 0xF352, 0xF353,
         0xF354, 0xF355, 0xF356, 0xF357, 0xF358, 0xF359, 0xF35B, 0xF35C, 0xF35E, 0xF35F,
         0xF360, 0xF361, 0xF362, 0xF363, 0xF364, 0xF365, 0xF366, 0xF367, 0xF368, 0xF369,
         0xF36A, 0xF36B, 0xF36C, 0xF36D, 0xF36E, 0xF36F, 0xF370, 0xF371, 0xF372, 0xF373,
         0xF374, 0xF375, 0xF376, 0xF377, 0xF378, 0xF379, 0xF37A, 0xF37B, 0xF37C,
      };

      static public string[] Accent2Characters = new string[] {
         "İ",    "Ğ",    "Ş",    "Ů",    "Ý",    "Č",    "Ď",    "Ě",    "Ň",    "Ř",
         "Š",    "Ť",    "Ž",    "Ą",    "Ć",    "Ę",    "Ł",    "Ń",    "Ś",    "Ź",
         "Ż",    "Ĺ",    "Ľ",    "Ŕ",    "Đ",    "Ő",    "Ű",    "ı",    "ǧ",    "ş",
         "ů",    "ý",    "č",    "ď",    "ě",    "ň",    "ř",    "š",    "ť",    "ž",
         "ą",    "ć",    "ę",    "ł",    "ń",    "ś",    "ź",    "ż",    "ĺ",    "ľ",
         "ŕ",    "đ",    "ő",    "ű",
      };

      static int[] HitachiAccent2Characters = new int[] {
         0xF380, 0xF382, 0xF383, 0xF384, 0xF385, 0xF386, 0xF387, 0xF388, 0xF389, 0xF38A,
         0xF38B, 0xF38C, 0xF38D, 0xF38E, 0xF38F, 0xF390, 0xF391, 0xF392, 0xF393, 0xF394,
         0xF395, 0xF396, 0xF397, 0xF398, 0xF399, 0xF39A, 0xF39B, 0xF3A1, 0xF3A2, 0xF3A3,
         0xF3A4, 0xF3A5, 0xF3A6, 0xF3A7, 0xF3A8, 0xF3A9, 0xF3AA, 0xF3AB, 0xF3AC, 0xF3AD,
         0xF3AE, 0xF3AF, 0xF3B0, 0xF3B1, 0xF3B2, 0xF3B3, 0xF3B4, 0xF3B5, 0xF3B6, 0xF3B7,
         0xF3B8, 0xF3B9, 0xF3BA, 0xF3BB,
      };

      #endregion

      #region Greek

      static public string[] GreekCharacters = new string[] {
         "Α",    "Β",    "Γ",    "Δ",    "Ε",    "Ζ",    "Η",    "Θ",    "Ι",    "Κ",    "Λ",    "Μ",
         "Ν",    "Ξ",    "Ο",    "Π",    "Ρ",    "Σ",    "Τ",    "Υ",    "Φ",    "Χ",    "Ψ",    "Ω",
         "α",    "β",    "γ",    "δ",    "ε",    "ζ",    "η",    "θ",    "ι",    "κ",    "λ",    "μ",
         "ν",    "ξ",    "ο",    "π",    "ρ",    "σ",    "τ",    "υ",    "φ",    "χ",    "ψ",    "ω",
      };

      static int[] HitachiGreekCharacters = new int[] {
         0xF2AC, 0xF3C0, 0xF3C1, 0xF3C2, 0xF3C3, 0xF3C4, 0xF3C5, 0xF3C6, 0xF3C7, 0xF3C8, 0xF3C9, 0xF3CA,
         0xF3CB, 0xF3CC, 0xF3CD, 0xF3CE, 0xF3CF, 0xF3D0, 0xF3D1, 0xF3D2, 0xF3D3, 0xF3D4, 0xF3D5, 0xF3D6,
         0xF3DF, 0xF3E0, 0xF3E1, 0xF3E2, 0xF3E3, 0xF3E4, 0xF3E5, 0xF3E6, 0xF3E7, 0xF3E8, 0xF3E9, 0xF3EA,
         0xF3EB, 0xF3EC, 0xF3ED, 0xF3EE, 0xF3EF, 0xF3F0, 0xF3F1, 0xF3F2, 0xF3F3, 0xF3F4, 0xF3F5, 0xF3F6,
      };

      #endregion

      #region Russian

      static public string[] RussianCharacters = new string[] {
         "А",    "Б",    "В",    "Г",    "Д",    "Е",    "Ё",    "Ж",    "З",    "И",
         "Й",    "К",    "Л",    "М",    "Н",    "О",    "П",    "Р",    "С",    "Т",
         "У",    "Ф",    "Х",    "Ц",    "Ч",    "Ш",    "Щ",    "Ъ",    "Ы",    "Ь",
         "Э",    "Ю",    "Я",    "Ђ",    "Ј",    "Љ",    "Њ",    "Ћ",    "Џ",
         "а",    "б",    "в",    "г",    "д",    "е",    "ё",    "ж",    "з",    "и",
         "й",    "к",    "л",    "м",    "н",    "о",    "п",    "р",    "с",    "т",
         "у",    "ф",    "х",    "ц",    "ч",    "ш",    "щ",    "ъ",    "ы",    "ь",
         "э",    "ю",    "я",    "ђ",    "ј",    "љ",    "њ",    "ћ",    "џ",
      };

      static int[] HitachiRussianCharacters = new int[] {
         0xF540, 0xF541, 0xF542, 0xF543, 0xF544, 0xF545, 0xF546, 0xF547, 0xF548, 0xF549,
         0xF54A, 0xF54B, 0xF54C, 0xF54D, 0xF54E, 0xF54F, 0xF550, 0xF551, 0xF552, 0xF553,
         0xF554, 0xF555, 0xF556, 0xF557, 0xF558, 0xF559, 0xF55A, 0xF55B, 0xF55C, 0xF55D,
         0xF55E, 0xF55F, 0xF560, 0xF561, 0xF562, 0xF563, 0xF564, 0xF565, 0xF566,
         0xF570, 0xF571, 0xF572, 0xF573, 0xF574, 0xF575, 0xF576, 0xF577, 0xF578, 0xF579,
         0xF57A, 0xF57B, 0xF57C, 0xF57D, 0xF57E, 0xF580, 0xF581, 0xF582, 0xF583, 0xF584,
         0xF585, 0xF586, 0xF587, 0xF588, 0xF589, 0xF58A, 0xF58B, 0xF58C, 0xF58D, 0xF58E,
         0xF58F, 0xF590, 0xF591, 0xF592, 0xF593, 0xF594, 0xF595, 0xF596, 0xF597,
      };

      #endregion

      #region Date/Time/Counter

      public static string[] DateTimeCharacters = new string[] {
         "{'}",  "{.}",  "{:}",  "{,}",  "{ }",  "{;}",  "{!}",
         "{Y}",  "{M}",  "{D}",  "{h}",  "{m}",  "{s}",  "{T}",  "{N}",  "{W}",  "{7}",  "{C}",  "{E}",  "{F}",
         "{{Y}", "{{M}", "{{D}", "{{h}", "{{m}", "{{s}", "{{T}", "{{N}", "{{W}", "{{7}", "{{C}", "{{E}", "{{F}",
         "{Y}}", "{M}}", "{D}}", "{h}}", "{m}}", "{s}}", "{T}}", "{N}}", "{W}}", "{7}}", "{C}}", "{E}}", "{F}}",
      };

      static int[] HitachiDateTimeCharacters = new int[] {
         0xF240, 0xF241, 0xF242, 0xF243, 0xF244, 0xF245, 0xF246,
         0xF250, 0xF251, 0xF252, 0xF253, 0xF254, 0xF255, 0xF256, 0xF257, 0xF258, 0xF259, 0xF25A, 0xF25B, 0xF25C,
         0xF260, 0xF261, 0xF262, 0xF263, 0xF264, 0xF265, 0xF266, 0xF267, 0xF268, 0xF269, 0xF26A, 0xF26B, 0xF26C,
         0xF270, 0xF271, 0xF272, 0xF273, 0xF274, 0xF275, 0xF276, 0xF277, 0xF278, 0xF279, 0xF27A, 0xF27B, 0xF27C,
      };

      static string[] DateTimeCharactersSOP4 = new string[] {
         "{Y}",  "{M}",  "{D}",  "{h}",  "{m}",  "{s}",  "{T}",  "{W}",  "{7}",  "{C}",  "{E}",  "{F}",
      };

      static int[] HitachiDateTimeCharactersSOP4 = new int[] {
         0x944E, 0x8C8E, 0x93FA, 0x8E9E, 0x95AA, 0x9562, 0x8273, 0x8F54, 0x976A, 0x8262, 0x8264, 0x8265,
      };



      #endregion

      #region Hebrew

      static public string[] HebrewCharacters = new string[] {
         "א", "ב", "ג", "ד", "ה", "ו", "ז", "ח", "ט", "י",
         "ך", "כ", "ל", "ם", "מ", "ן", "נ", "ס", "ע", "ף",
         "פ", "ץ", "צ", "ק", "ר", "ש", "ת", "־", "	׀", "׃",
         "׆", "װ", "ױ", "ײ", "׳", "״"
      };

      #endregion


   }
}
