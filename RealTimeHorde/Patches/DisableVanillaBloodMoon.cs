using HarmonyLib;

namespace RealTimeHorde.Patches
{
    /// <summary>
    /// バニラの7日周期BloodMoonを無効化するパッチ（H-001）
    /// IsBloodMoonTime() を常にfalseにすることで、ゲーム内時間によるBloodMoon発動を抑制する。
    /// RealTimeHorde が StartBloodMoon() を直接呼ぶため、このパッチとは競合しない。
    /// </summary>
    [HarmonyPatch(typeof(AIDirectorBloodMoonComponent), "IsBloodMoonTime")]
    internal class DisableVanillaBloodMoonPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false; // 元メソッドをスキップ
        }
    }

    /// <summary>
    /// SetForToday() も無効化（バニラがBloodMoonデーに設定するのを防ぐ）
    /// </summary>
    [HarmonyPatch(typeof(AIDirectorBloodMoonComponent), "SetForToday")]
    internal class DisableSetForTodayPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
