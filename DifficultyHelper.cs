namespace Taberry;

internal static class DifficultyHelper {
    public static int difficulty => MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
    public static string GetDifficultyName(int difficulty) {
        switch (difficulty) {
            case 0:
                return "Harmless";
            case 1:
                return "Lenient";
            case 2:
                return "Standard";
            case 3:
                return "Violent";
            case 4:
                return "Brutal";
            case 5:
                return "ULTRAKILL Must Die";
            default:
                return "Unknown difficulty";
        }
    }

    public static string GetDifficultyName() {
        return GetDifficultyName(difficulty);
    }
}
