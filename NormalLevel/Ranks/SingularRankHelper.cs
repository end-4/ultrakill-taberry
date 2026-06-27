using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Taberry.NormalLevel.Ranks;

/// <summary>
/// By "singular" I mean it counts for a single statistic like kills.
/// (RankHelper in the base game turns out to be for the overall rank)
/// </summary>
public static class SingularRankHelper {
    public static int GetRank(int[] ranksToCheck, float value, bool reverse) {
        int index = 0;
        while (index < ranksToCheck.Length) {
            if ((reverse && value <= ranksToCheck[index]) || (!reverse && value >= ranksToCheck[index])) {
                index++;
                continue;
            }

            break;
        }

        if (ranksToCheck != null && ranksToCheck.Length > 0) {
            int targetIndex = Mathf.Clamp(index, 0, ranksToCheck.Length);
            return targetIndex;
        } else {
            return 0;
        }
    }

    public static int ClampRank(int rank, int[] ranksToCheck) {
        return Mathf.Clamp(rank, 0, ranksToCheck.Length);
    }

    public static string GetRankForegroundColor(int rank) {
        switch (rank) {
            case 0: return "#0094FF"; // D
            case 1: return "#4CFF00"; // C
            case 2: return "#FFD800"; // B
            case 3: return "#FF6A00"; // A
            case 4: return "#FF0000"; // S
            default:
                return "#FFFFFF";
        }
    }
}
