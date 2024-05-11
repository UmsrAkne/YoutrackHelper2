namespace YoutrackHelper2.Models
{
    /// <summary>
    /// WorkType を文字列に、または文字列を WorkType に変換する拡張メソッドを定義するクラス
    /// </summary>
    public static class WorkTypeExtension
    {
        public static string ToWorkTypeName(this WorkType value)
        {
            return value switch
            {
                WorkType.Feature => "機能",
                WorkType.Appearance => "外観",
                WorkType.Test => "テスト",
                WorkType.Todo => "タスク",
                WorkType.Bug => "バグ",
                _ => string.Empty,
            };
        }

        public static WorkType FromString(string description)
        {
            return description switch
            {
                "機能" => WorkType.Feature,
                "外観" => WorkType.Appearance,
                "テスト" => WorkType.Test,
                "タスク" => WorkType.Todo,
                "バグ" => WorkType.Bug,
                _ => WorkType.Feature,
            };
        }
    }
}