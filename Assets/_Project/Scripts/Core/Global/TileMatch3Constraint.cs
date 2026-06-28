namespace TileMatch3.Core.Global
{
    public static class TileMatch3Constraint
    {
        public const string GAME_MAIN_KEY = "TileMatch3";
        public static string GetKey(string raw) => $"{GAME_MAIN_KEY}_{raw}";
    }
}