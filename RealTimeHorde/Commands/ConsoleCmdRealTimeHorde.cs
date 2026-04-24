using RealTimeHorde.Managers;
using RealTimeHorde.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RealTimeHorde.Commands
{
    /// <summary>
    /// F1コンソールからスケジュールを管理するコマンド群
    ///
    /// 使い方:
    ///   rth_list                            → 現在のスケジュール一覧
    ///   rth_add <曜日> <時> <分> [警告分]   → スケジュール追加
    ///   rth_remove <番号>                   → スケジュール削除
    ///   rth_clear                           → 全スケジュール削除
    ///   rth_save                            → XMLファイルに保存
    ///   rth_next                            → 次回ホード発生日時を表示
    /// </summary>
    public class ConsoleCmdRealTimeHorde : ConsoleCmdAbstract
    {
        public override bool IsExecuteOnClient => true;
        public override bool AllowedInMainMenu => false;

        public override string[] GetCommands() => new[]
        {
            "rth_list", "rth_add", "rth_remove", "rth_clear", "rth_save", "rth_next"
        };
        public override string[] getCommands() => GetCommands();

        public override string GetDescription() =>
            "RealTimeHorde: リアルタイムホードのスケジュールを管理します";
        public override string getDescription() => GetDescription();

        public override string GetHelp() =>
            "  rth_list                          現在のスケジュール一覧を表示\n" +
            "  rth_add <曜日(英語)> <時> <分> [警告分]  スケジュールを追加\n" +
            "    例: rth_add Wednesday 22 0 30\n" +
            "    曜日: Monday/Tuesday/Wednesday/Thursday/Friday/Saturday/Sunday\n" +
            "  rth_remove <番号>                 番号のスケジュールを削除\n" +
            "  rth_clear                         全スケジュールを削除\n" +
            "  rth_save                          スケジュールをXMLファイルに保存\n" +
            "  rth_next                          次回ホード発生日時を表示";

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count == 0)
            {
                Log.Out(GetHelp());
                return;
            }

            switch (_params[0].ToLower())
            {
                case "rth_list":
                    CmdList();
                    break;
                case "rth_add":
                    CmdAdd(_params);
                    break;
                case "rth_remove":
                    CmdRemove(_params);
                    break;
                case "rth_clear":
                    CmdClear();
                    break;
                case "rth_save":
                    CmdSave();
                    break;
                case "rth_next":
                    CmdNext();
                    break;
                default:
                    Log.Out($"[RealTimeHorde] 不明なコマンド: {_params[0]}");
                    Log.Out(GetHelp());
                    break;
            }
        }

        // rth_list
        private void CmdList()
        {
            if (ScheduleManager.Schedules.Count == 0)
            {
                Log.Out("[RealTimeHorde] スケジュールが登録されていません");
                return;
            }
            Log.Out($"[RealTimeHorde] 現在のスケジュール（{ScheduleManager.Schedules.Count}件）:");
            for (int i = 0; i < ScheduleManager.Schedules.Count; i++)
            {
                var s = ScheduleManager.Schedules[i];
                Log.Out($"  [{i}] {s.DayOfWeek} {s.Hour:D2}:{s.Minute:D2}  警告{s.WarningMinutes}分前");
            }
        }

        // rth_add <曜日> <時> <分> [警告分]
        private void CmdAdd(List<string> _params)
        {
            if (_params.Count < 4)
            {
                Log.Out("[RealTimeHorde] 使い方: rth_add <曜日> <時> <分> [警告分]");
                Log.Out("  例: rth_add Wednesday 22 0 30");
                return;
            }
            try
            {
                if (!Enum.TryParse(_params[1], true, out DayOfWeek day))
                    throw new Exception($"不正な曜日: '{_params[1]}'  (例: Wednesday)");

                var hour = int.Parse(_params[2]);
                var minute = int.Parse(_params[3]);
                var warning = _params.Count >= 5 ? int.Parse(_params[4]) : 0;

                if (hour < 0 || hour > 23) throw new Exception($"時刻が不正: hour={hour}");
                if (minute < 0 || minute > 59) throw new Exception($"時刻が不正: minute={minute}");

                ScheduleManager.Schedules.Add(new HordeSchedule
                {
                    DayOfWeek = day,
                    Hour = hour,
                    Minute = minute,
                    WarningMinutes = warning
                });
                Log.Out($"[RealTimeHorde] ✅ 追加: {day} {hour:D2}:{minute:D2}  警告{warning}分前");
                Log.Out($"[RealTimeHorde] ※ rth_save で設定を保存してください");
            }
            catch (Exception ex)
            {
                Log.Out($"[RealTimeHorde] ❌ エラー: {ex.Message}");
            }
        }

        // rth_remove <番号>
        private void CmdRemove(List<string> _params)
        {
            if (_params.Count < 2 || !int.TryParse(_params[1], out int idx))
            {
                Log.Out("[RealTimeHorde] 使い方: rth_remove <番号>");
                CmdList();
                return;
            }
            if (idx < 0 || idx >= ScheduleManager.Schedules.Count)
            {
                Log.Out($"[RealTimeHorde] ❌ 番号が不正です（0〜{ScheduleManager.Schedules.Count - 1}）");
                return;
            }
            var removed = ScheduleManager.Schedules[idx];
            ScheduleManager.Schedules.RemoveAt(idx);
            Log.Out($"[RealTimeHorde] ✅ 削除: {removed.DayOfWeek} {removed.Hour:D2}:{removed.Minute:D2}");
        }

        // rth_clear
        private void CmdClear()
        {
            var count = ScheduleManager.Schedules.Count;
            ScheduleManager.Schedules.Clear();
            Log.Out($"[RealTimeHorde] ✅ {count}件のスケジュールを全削除しました");
        }

        // rth_save
        private void CmdSave()
        {
            try
            {
                var path = ScheduleManager.ConfigPath;
                if (path == null)
                {
                    Log.Out("[RealTimeHorde] ❌ 設定ファイルのパスが不明です（MOD初期化前？）");
                    return;
                }

                var doc = new XmlDocument();
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
                    node.SetAttribute("dayOfWeek", s.DayOfWeek.ToString());
                    node.SetAttribute("hour", s.Hour.ToString());
                    node.SetAttribute("minute", s.Minute.ToString());
                    node.SetAttribute("warningMinutes", s.WarningMinutes.ToString());
                    root.AppendChild(node);
                }

                doc.Save(path);
                Log.Out($"[RealTimeHorde] ✅ {ScheduleManager.Schedules.Count}件のスケジュールを保存しました");
                Log.Out($"[RealTimeHorde]   保存先: {path}");
            }
            catch (Exception ex)
            {
                Log.Out($"[RealTimeHorde] ❌ 保存失敗: {ex.Message}");
            }
        }

        // rth_next
        private void CmdNext()
        {
            if (ScheduleManager.Schedules.Count == 0)
            {
                Log.Out("[RealTimeHorde] スケジュールが登録されていません");
                return;
            }
            var now = DateTime.Now;
            var nearest = DateTime.MaxValue;
            HordeSchedule? nearestSchedule = null;

            foreach (var s in ScheduleManager.Schedules)
            {
                var next = s.GetNextOccurrence(now);
                if (next < nearest)
                {
                    nearest = next;
                    nearestSchedule = s;
                }
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
