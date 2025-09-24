public static class PublicTag
{
    // uid から 0000-9999 の4桁を作る（安定・軽量）
    public static string Make(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return "0000";
        unchecked
        {
            uint hash = 2166136261; // FNV-1a 32bit
            for (int i = 0; i < uid.Length; i++)
            {
                hash ^= uid[i];
                hash *= 16777619;
            }
            return (hash % 10000).ToString("D4");
        }
    }
}
