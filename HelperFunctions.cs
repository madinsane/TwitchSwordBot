using TwitchSwordBot.Imports;
using HomoglyphConverter;

namespace TwitchSwordBot {
    /// <summary>
    /// Contains special helper functions
    /// </summary>
    public static class HelperFunctions {

        /// <summary>
        /// Gets remainder of str after the first instance of substring search is removed
        /// </summary>
        /// <param name="str">Full string</param>
        /// <param name="search">Substring to remove</param>
        /// <returns>Remainder of str without search</returns>
        public static string GetStringRemainder(string str, string search) {
            int searchEnd = str.IndexOf(search) + search.Length;
            if (searchEnd <= str.Length) {
                return str.Substring(searchEnd);
            }
            return "";
        }

        /// <summary>
        /// Converts a string into lower case base form for comparison
        /// </summary>
        /// Uses StringUtils and HomoglyphConverter
        /// <param name="str">Converts string to plain</param>
        /// <returns>Lower case plain version of str</returns>
        public static string ConvertStringToPlain(string str) {
            string stripped = str.StripInvisibleAndDiacritics();
            return stripped.ToCanonicalForm().ToLower();
        }

        /// <summary>
        /// Flips a string to upside down and reversed form after stripping and converts to lower case
        /// </summary>
        /// <param name="str">String to flip</param>
        /// <returns>Flipped string</returns>
        public static string FlipStringAndStrip(string str) {
            string stripped = str.StripInvisibleAndDiacritics();
            return stripped.FlipString().ToLower();
        }

        /// <summary>
        /// Takes 2 strings and compares the different forms to test if any result in true
        /// </summary>
        /// <param name="str1">String to test</param>
        /// <param name="str2">String in known form</param>
        /// <returns><c>true</c> if any form is equal</returns>
        public static bool CompareStrings(string str1, string str2) {
            string plain1 = ConvertStringToPlain(str1);
            string plain2 = ConvertStringToPlain(str2);
            string flip1 = FlipStringAndStrip(str1);
            return plain1.Equals(plain2) || plain1.Equals(str2.ToLower()) || flip1.Equals(str2.ToLower());
        }

        /// <summary>
        /// Takes 2 strings and checks if str1 is a substring of container in any of the different forms
        /// </summary>
        /// <param name="str1">String to test</param>
        /// <param name="container">String in known form</param>
        /// <returns><c>true</c> if any form of str1 is substring of container</returns>
        public static bool DoesStringContain(string str1, string container) {
            string plain1 = ConvertStringToPlain(str1);
            string plain2 = ConvertStringToPlain(container);
            string flip1 = FlipStringAndStrip(str1);
            return plain2.Contains(plain1) || container.ToLower().Contains(plain1) || container.ToLower().Contains(flip1);
        }

        /// <summary>
        /// Attempts to parse tags sent by IRC
        /// </summary>
        /// <param name="tags">Full string of tags</param>
        /// <returns>New <seealso cref="ChatTags"/> object with parsed info</returns>
        public static ChatTags ParseTags(string tags)
        {
            string tagsTrim = tags.TrimStart('@');
            string[] tagsSplit = tagsTrim.Split(";");
            ChatTags chatTags = new();
            foreach (string tag in tagsSplit)
            {
                if (tag.StartsWith("badges"))
                {
                    //Parse badges
                    if (tag.IndexOf("broadcaster") >= 0)
                    {
                        chatTags.IsBroadcaster = true;
                    }
                    if (tag.IndexOf("subscriber") >= 0)
                    {
                        chatTags.IsSub = true;
                    }
                    if (tag.IndexOf("admin") >= 0 ||
                        tag.IndexOf("staff") >= 0 ||
                        tag.IndexOf("global_mod") >= 0 ||
                        tag.IndexOf("moderator") >= 0)
                    {
                        chatTags.IsMod = true;
                    }
                }
                else if (tag.StartsWith("id"))
                {
                    chatTags.MsgId = HelperFunctions.GetStringRemainder(tag, "id=");
                }
                else if (tag.StartsWith("mod"))
                {
                    chatTags.IsMod = HelperFunctions.GetStringRemainder(tag, "mod=") == "1";
                }
                else if (tag.StartsWith("subscriber"))
                {
                    chatTags.IsSub = HelperFunctions.GetStringRemainder(tag, "subscriber=") == "1";
                }
                else if (tag.StartsWith("user-id"))
                {
                    chatTags.UserId = HelperFunctions.GetStringRemainder(tag, "user-id=");
                }
            }

            return chatTags;
        }
    }
}