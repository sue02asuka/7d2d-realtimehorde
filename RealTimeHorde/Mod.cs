using HarmonyLib;
using RealTimeHorde.Managers;
using System.Reflection;

namespace RealTimeHorde
{
    public class RealTimeHordeMod : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Log.Out("[RealTimeHorde] MOD初期化開始");

            // スケジュール設定ファイルを読み込む（S-001）
            ScheduleManager.Load(modInstance.Path);

            // Harmony パッチを全適用
            var harmony = new Harmony("com.sue02.realtimehorde");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Out("[RealTimeHorde] MOD初期化完了 — リアルタイムホードMODが有効です");
        }
    }
}
