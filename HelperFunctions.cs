using TwitchSwordBot.Imports;
using HomoglyphConverter;

namespace TwitchSwordBot {
    public static class HelperFunctions {


        public static string GetStringRemainder(string str, string search) {
            int searchEnd = str.IndexOf(search) + search.Length;
            if (searchEnd <= str.Length) {
                return str.Substring(searchEnd);
            }
            return "";
        }

        public static string ConvertStringToPlain(string str) {
            string stripped = str.StripInvisibleAndDiacritics();
            return stripped.ToCanonicalForm().ToLower();
        }

        public static string FlipStringAndStrip(string str) {
            string stripped = str.StripInvisibleAndDiacritics();
            return stripped.FlipString().ToLower();
        }

        public static bool CompareStrings(string str1, string str2) {
            string plain1 = ConvertStringToPlain(str1);
            string plain2 = ConvertStringToPlain(str2);
            string flip1 = FlipStringAndStrip(str1);
            return plain1.Equals(plain2) || plain1.Equals(str2.ToLower()) || flip1.Equals(str2.ToLower());
        }

        public static bool DoesStringContain(string str1, string container) {
            string plain1 = ConvertStringToPlain(str1);
            string plain2 = ConvertStringToPlain(container);
            string flip1 = FlipStringAndStrip(str1);
            return plain2.Contains(plain1) || container.ToLower().Contains(plain1) || container.ToLower().Contains(flip1);
        }
    }
}