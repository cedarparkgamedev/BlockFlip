public static class GameSessionSettings
{
    public static GameMode Mode { get; set; } = GameMode.Classic;

    public static bool UsesDangerGauge => Mode == GameMode.Classic;
}
