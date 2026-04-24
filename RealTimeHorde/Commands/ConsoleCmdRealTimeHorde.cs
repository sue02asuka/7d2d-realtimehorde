using RealTimeHorde.Managers;
using RealTimeHorde.Models;
using System;
using System.Collections.Generic;
using System.Xml;

namespace RealTimeHorde.Commands
{
    /// <summary>
    /// F1コンソールでスケジュールを管理するコマンド
    ///
    /// 使い方（F1を押してから入力）:
    ///   rth list                          → スケジュール一覧
    ///   rth add <曜日(英語)> <時> <分> [警告分]  → スケジュール追加
    ///   rth remove <番号>                  → スケジュール削除（番号はlistで確認）
    ///   rth clear                         → 全削除
    ///   rth save                          → XMLに保存（ゲーム再起動後も反映）
    ///   rth next                          → 次回ホード日時を表示
    ///
    /// 曜日: Monday / Tuesday / Wednesday / Thursday / Friday / Saturday / Sunday
    /// </summary>
    public class ConsoleCmdRealTimeHorde : ConsoleCmdAbstract
    {
        public override bool IsExecuteOnClient => true;
        public override bool AllowedInMainMenu => false;

        // コマンド名は "rth" のみ。rth + サブコマンドで操作する。
        public override string[] GetCommands() => new[] { "rth" };
        public override string[] getCommands() => GetCommands();

        public override string GetDescription() =>
            "RealTimeHorde: リアルタイムホードのスケジュールを管理します (rth help で詳細)";
        public override string getDescription() => GetDescription();

        public override string GetHelp() =>
            "=== RealTimeHorde コマンド ===\n" +
            "  rth list                              スケジュール一覧\n" +
            "  rth add <曜日> <時> <分> [警告分]     スケジュール追加\n" +
            "    例: rth add Wednesday 22 0 30\n" +
            "    例: rth add Friday 21 0 60\n" +
            "  rth remove <番号>                     スケジュール削除\n" +
            "  rth clear                             全スケジュール削除\n" +
            "  rth save                              XMLファイルに保存\n" +
            "  rth next                              次回ホード日時を表示\n" +
            "  rth help                              このヘルプを表示\n" +
            "曜日: Monday/Tuesday/Wednesday/Thursday/Friday/Saturday/Sunday";

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            // 7D2Dはコマンド名を除いた引数だけを _params に渡す
            // 例: "rth add Friday 22 0 30" → _params = ["add", "Friday", "22", "0", "30"]
            var sub = _params.Count >= 1 ? _params[0].ToLower() : "help";

            switch (sub)
            {
                case "list":    CmdList(); break;
                case "add":     CmdAdd(_params); break;
                case "remove":  CmdRemove(_params); break;
                case "clear":   CmdClear(); break;
                case "save":    CmdSave(); break;
                case "next":    CmdNext(); break;
                default:
                    Log.Out(GetHelp());
                    break;
            }
        }

        // rth list
        private void CmdList()
        {
            if (ScheduleManager.Schedules.Count == 0)
            {
                Log.Out("[RealTimeHorde] スケジュールが登録されていません");
                Log.Out("[RealTimeHorde] 例: rth add Wednesday 22 0 30");
                return;
            }
            Log.Out($"[RealTimeHorde] 現在のスケジュール（{ScheduleManager.Schedules.Count}件）:");
            for (int i = 0; i < ScheduleManager.Schedules.Count; i++)
            {
                var s = ScheduleManager.Schedules[i];
                Log.Out($"  [{i}] {s.DayOfWeek,-12} {s.Hour:D2}:{s.Minute:D2}  警告{s.WarningMinutes}分前");
            }
        }

        // rth add <曜日> <時> <分> [警告分]
        private void CmdAdd(List<string> _params)
        {
            // _params: ["add", 曜日, 時, 分, (警告分)]
            if (_params.Count < 4)
            {
                Log.Out("[RealTimeHorde] 使い方: rth add <曜日> <時> <分> [警告分]");
                Log.Out("  例: rth add Wednesday 22 0 30");
                Log.Out("  曜日: Monday/Tuesday/Wednesday/Thursday/Friday/Saturday/Sunday");
                return;
            }
            try
            {
                if (!Enum.TryParse(_params[1], true, out DayOfWeek day))
                    throw new Exception($"不正な曜日: '{_params[1]}'  例→ Wednesday / Friday");

                var hour    = int.Parse(_params[2]);
                var minute  = int.Parse(_params[3]);
                var warning = _params.Count >= 5 ? int.Parse(_params[4]) : 0;

                if (hour   < 0 || hour   > 23) throw new Exception($"時刻が不正: hour={hour}（0〜23）");
                if (minute < 0 || minute > 59) throw new Exception($"時刻が不正: minute={minute}（0〜59）");

                ScheduleManager.Schedules.Add(new HordeSchedule
                {
                    DayOfWeek      = day,
                    Hour           = hour,
                    Minute         = minute,
                    WarningMinutes = warning
                });
                Log.Out($"[RealTimeHorde] ✅ 追加: {day} {hour:D2}:{minute:D2}  警告{warning}分前");
                Log.Out($"[RealTimeHorde]    ※ rth save で保存してください");
            }
            catch (Exception ex)
            {
                Log.Out($"[RealTimeHorde] ❌ {ex.Message}");
            }
        }

        // rth remove <番号>
        private void CmdRemove(List<string> _params)
        {
            // _params: ["remove", 番号]
            if (_params.Count < 2 || !int.TryParse(_params[1], out int idx))
            {
                Log.Out("[RealTimeHorde] 使い方: rth remove <番号>");
                CmdList();
                return;
            }
            if (idx < 0 || idx >= ScheduleManager.Schedules.Count)
            {
                Log.Out($"[RealTimeHorde] ❌ 番号が不正です（0〜{ScheduleManager.Schedules.Count - 1}）");
                CmdList();
                return;
            }
            var removed = ScheduleManager.Schedules[idx];
            ScheduleManager.Schedules.RemoveAt(idx);
            Log.Out($"[RealTimeHorde] ✅ 削除: {removed.DayOfWeek} {removed.Hour:D2}:{removed.Minute:D2}");
        }

        // rth clear
        private void CmdClear()
        {
            var count = ScheduleManager.Schedules.Count;
            ScheduleManager.Schedules.Clear();
            Log.Out($"[RealTimeHorde] ✅ {count}件のスケジュールを全削除しました");
        }

        // rth save
        private void CmdSave()
        {
            try
            {
                var path = ScheduleManager.ConfigPath;
                if (string.IsNullOrEmpty(path))
                {
                    Log.Out("[RealTimeHorde] ❌ 保存先パスが不明です（MOD初期化前？）");
                    return;
                }

                var doc  = new XmlDocument();
                var decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(decl);

                var comment = doc.CreateComment(
                    "\n  RealTimeHorde スケジュール設定\n" +
                    "  dayOfWeek: Monday/Tuesday/Wednesday/Thursday/Friday/Saturday/Sunday\n" +
                    "  hour: 0-23  minute: 0-59  warningMinutes: 0で無効\n");
                doc.AppendChild(comment);

                var root = doc.CreateElement("HordeSchedule");
                doc.AppendChild(root);

                foreach (var s in ScheduleManager.Schedules)
                {
                    var node = doc.CreateElement("HordeEvent");
                    node.SetAttribute("dayOfWeek",      s.DayOfWeek.ToString());
                    node.SetAttribute("hour",           s.Hour.ToString());
                    node.SetAttribute("minute",         s.Minute.ToString());
                    node.SetAttribute("warningMinutes", s.WarningMinutes.ToString());
                    root.AppendChild(node);
                }

                doc.Save(path);
                Log.Out($"[RealTimeHorde] ✅ {ScheduleManager.Schedules.Count}件を保存しました → {path}");
            }
            catch (Exception ex)
            {
                Log.Out($"[RealTimeHorde] ❌ 保存失敗: {ex.Message}");
            }
        }

        // rth next
        private void CmdNext()
        {
            if (ScheduleManager.Schedules.Count == 0)
            {
                Log.Out("[RealTimeHorde] スケジュールが登録されていません");
                return;
            }
            var now     = DateTime.Now;
            var nearest = DateTime.MaxValue;
            HordeSchedule? nearestSchedule = null;

            foreach (var s in ScheduleManager.Schedules)
            {
                var next = s.GetNextOccurrence(now);
                if (next < nearest) { nearest = next; nearestSchedule = s; }
            }

            if (nearestSchedule != null)
            {
                var diff = nearest - now;
                Log.Out($"[RealTimeHorde] 次回ホード: {nearest:M/d(ddd) HH:mm}  " +
                        $"（あと {(int)diff.TotalHours}時間{diff.Minutes}分）");
            }
        }
    }
}
