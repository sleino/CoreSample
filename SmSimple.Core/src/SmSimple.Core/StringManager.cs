namespace SmSimple.Core
{
    public class StringManager
    {
        public static string GetString(string key)
        {
            return key; 
        }

        public static string GetString(string key, string key2)
        {
            return key + key2;
        }


        internal static string DoNotTranslate(string key) { return key; }
    }
}
