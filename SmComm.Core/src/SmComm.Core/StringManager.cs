namespace SmComm.Core
{
    internal static class StringManager
    {
        internal static string GetString(string key)
        {
            return SmSimple.Core.StringManager.GetString(key);
        }

        internal static string GetString(string key, string key2)
        {
            return SmSimple.Core.StringManager.GetString(key, key2);
        }

        internal static string DoNotTranslate(string key) { return key; }
    }
}
