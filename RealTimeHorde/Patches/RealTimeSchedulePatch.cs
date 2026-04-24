using HarmonyLib;
using RealTimeHorde.Managers;
using System;

namespace RealTimeHorde.Patches
{
    /// <summary>
    /// ゲームの毎フレームUpdateにフックし、現実時刻でスケジュールを監視する（H-002）
    /// パフォーマンスのため、実際の処理は毎分1回のみ実行する。
    /// </summary>
    [HarmonyPatch(typeof(GameManager), "Update")]
    internal class RealTimeSchedulePatch
    {
        private static DateTime _lastCheck = DateTime.MinValue;
        private static DateTime _lastTriggerDateTime = DateTime.MinValue;
        private static bool _hordeTriggeredThisSlot = false;
        private static DateTime _lastDate = DateTime.MinValue;

        public static void Postfix()
        {
            // ゲームが完全に起動していない間はスキップ
            if (GameManager.Instance?.gameStateManager == null) return;
            if (!GameManager.Instance.gameStateManager.IsGameStarted()) return;
            if (GameManager.Instance.World == null) return;

            var now = DateTime.Now;

            // 日付変わりでフラグリセット
            if (now.Date != _lastDate)
            {
                _lastDate = now.Date;
                _hordeTriggeredThisSlot = false;
                ScheduleManager.ResetDailyFlags();
            }

            // 毎分1回だけチェック（FPS負荷を避ける）
            if ((now - _lastCheck).TotalSeconds < 60) return;
            _lastCheck = now;

            Log.Out($"[RealTimeHorde] 時刻チェック: {now:ddd HH:mm}");

            foreach (var schedule in ScheduleManager.Schedules)
            {
                // 警告アナウンス（S-004）
                if (!schedule.WarningSent && schedule.MatchesWarning(now))
                {
                    schedule.WarningSent = true;
                    var msg = $"⚠ ホード警告: {schedule.WarningMinutes}分後にホードが来ます！準備してください！";
                    SendGameMessage(msg);
                    Log.Out($"[RealTimeHorde] 警告送信: {msg}");
                }

                // スケジュール一致判定（H-003）
                if (schedule.Matches(now))
                {
                    // 同一スロットでの重複発動防止（H-004）
                    if (_hordeTriggeredThisSlot && now - _lastTriggerDateTime < TimeSpan.FromMinutes(5))
                    {
                        Log.Out("[RealTimeHorde] 重複発動防止: 同一スロット内で既に発動済み");
                        continue;
                    }

                    Log.Out($"[RealTimeHorde] スケジュール一致！BloodMoon発動: {schedule}");
                    TriggerBloodMoon();
                    _hordeTriggeredThisSlot = true;
                    _lastTriggerDateTime = now;
                    schedule.WarningSent = true; // 発動後は警告も不要
                }
            }
        }

        private static void TriggerBloodMoon()
        {
            try
            {
                var bloodMoon = GameManager.Instance.World.GetAIDirector().BloodMoonComponent;
                bloodMoon.StartBloodMoon();
                Log.Out("[RealTimeHorde] BloodMoon発動成功");
            }
            catch (Exception ex)
            {
                Log.Error($"[RealTimeHorde] BloodMoon発動失敗: {ex.Message}");
            }
        }

        private static void SendGameMessage(string message)
        {
            try
            {
                var player = GameManager.Instance.World.GetPrimaryPlayer();
                if (player != null)
                    GameManager.ShowTooltip(player, message, "ui_warning");
            }
            catch (Exception ex)
            {
                Log.Warning($"[RealTimeHorde] メッセージ送信失敗: {ex.Message}");
            }
        }
    }
}
