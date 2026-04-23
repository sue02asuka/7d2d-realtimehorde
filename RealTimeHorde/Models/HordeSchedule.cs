using System;

namespace RealTimeHorde.Models
{
    public class HordeSchedule
    {
        public DayOfWeek DayOfWeek { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int WarningMinutes { get; set; }

        // 警告済みフラグ（1スケジュールにつき1回のみ警告）
        public bool WarningSent { get; set; }

        public bool Matches(DateTime now)
        {
            return now.DayOfWeek == DayOfWeek
                && now.Hour == Hour
                && now.Minute == Minute;
        }

        // warning時刻に一致するか（WarningMinutes分前）
        public bool MatchesWarning(DateTime now)
        {
            if (WarningMinutes <= 0) return false;
            var warnTime = GetNextOccurrence(now).AddMinutes(-WarningMinutes);
            return now.Hour == warnTime.Hour && now.Minute == warnTime.Minute;
        }

        // 次回この曜日・時刻が来るDateTimeを返す
        public DateTime GetNextOccurrence(DateTime from)
        {
            var daysUntil = ((int)DayOfWeek - (int)from.DayOfWeek + 7) % 7;
            var candidate = from.Date.AddDays(daysUntil).AddHours(Hour).AddMinutes(Minute);
            if (candidate <= from) candidate = candidate.AddDays(7);
            return candidate;
        }

        public override string ToString()
            => $"{DayOfWeek} {Hour:D2}:{Minute:D2}（{WarningMinutes}分前警告）";
    }
}
