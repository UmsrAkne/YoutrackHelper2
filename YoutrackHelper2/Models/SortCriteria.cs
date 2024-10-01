namespace YoutrackHelper2.Models
{
    /// <summary>
    /// ソートの基準を表す Enum です。
    /// </summary>
    public enum SortCriteria
    {
        /// <summary>
        /// 日時を基準にソートします。
        /// </summary>
        Date,

        /// <summary>
        /// アルファベット（五十音）でソートします。
        /// </summary>
        Alphabetical,

        /// <summary>
        /// 課題の ID を基準にソートします。
        /// </summary>
        Id,
    }
}