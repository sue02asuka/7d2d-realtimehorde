using RealTimeHorde.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RealTimeHorde.Managers
{
    public static class ScheduleManager
    {
        public static List<HordeSchedule> Schedules { get; private set; } = new List<HordeSchedule>();

        // rth_save コマンドから参照できるよう保持
        public static string? ConfigPath { get; private set; }

        public static void Load(string modPath)
        {
            var configPath = Path.Combine(modPath, "Config", "horde-schedule.xml");
            ConfigPath = configPath;
            if (!File.Exists(configPath))
            {
                Log.Warning($"[RealTimeHorde] 設定ファイルが見つかりません: {configPath}");
                return;
            }

            Schedules.Clear();
            var doc = new XmlDocument();
            doc.Load(configPath);

            var nodes = doc.SelectNodes("//HordeEvent");
            if (nodes == null)
            {
                Log.Warning("[RealTimeHorde] HordeEvent要素が見つかりません");
                return;
            }

            foreach (XmlNode node in nodes)
            {
                try
                {
                    var dayStr = node.Attributes?["dayOfWeek"]?.Value
                        ?? throw new Exception("dayOfWeek 属性がありません");
                    var hourStr = node.Attributes?["hour"]?.Value
                        ?? throw new Exception("hour 属性がありません");
                    var minuteStr = node.Attributes?["minute"]?.Value
                        ?? throw new Exception("minute 属性がありません");
                    var warningStr = node.Attributes?["warningMinutes"]?.Value ?? "0";

                    if (!Enum.TryParse(dayStr, true, out DayOfWeek day))
                        throw new Exception($"不正な曜日名: '{dayStr}' (例: Monday, Tuesday, Wednesday...)");

                    var hour = int.Parse(hourStr);
                    var minute = int.Parse(minuteStr);
                    var warning = int.Parse(warningStr);

                    if (hour < 0 || hour > 23) throw new Exception($"時刻が不正: hour={hour}");
                    if (minute < 0 || minute > 59) throw new Exception($"時刻が不正: minute={minute}");

                    Schedules.Add(new HordeSchedule
                    {
                        DayOfWeek = day,
                        Hour = hour,
                        Minute = minute,
                        WarningMinutes = warning
                    });
                    Log.Out($"[RealTimeHorde] スケジュール登録: {day} {hour:D2}:{minute:D2}（{warning}分前警告）");
                }
                catch (Exception ex)
                {
                    Log.Warning($"[RealTimeHorde] スケジュールスキップ: {ex.Message}");
                }
            }
            Log.Out($"[RealTimeHorde] 設定読込完了: {Schedules.Count}件のスケジュール");
        }

        // 警告フラグを日付変わりでリセット
        public static void ResetDailyFlags()
        {
            foreach (var s in Schedules) s.WarningSent = false;
        }
    }
}
