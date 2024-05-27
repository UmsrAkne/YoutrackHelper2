using System;
using System.Collections.Generic;

namespace YoutrackHelper2.Models
{
    public static class StateExtension
    {
        public static string ToStateName(this State value)
        {
            return value switch
            {
                State.Incomplete => "未完了",
                State.Completed => "完了",
                State.Progressing => "作業中",
                State.Obsolete => "廃止",
                _ => string.Empty,
            };
        }

        public static State FromString(string str)
        {
            return str switch
            {
                "完了" => State.Completed,
                "未完了" => State.Incomplete,
                "作業中" => State.Progressing,
                "廃止" => State.Obsolete,
                _ => throw new ArgumentException($@"{str} は不正な引数です"),
            };
        }

        public static bool CanConvert(string text)
        {
            return new HashSet<string> { "完了", "未完了", "作業中", "廃止", }.Contains(text);
        }
    }
}