namespace YoutrackHelper2.Models
{
    public enum WorkType
    {
        /// <summary>
        /// 機能に関する作業です。
        /// </summary>
        Feature,

        /// <summary>
        /// 軽微な作業につけるタイプです。
        /// </summary>
        Todo,

        /// <summary>
        /// 外観に関する変更につけるタイプです。
        /// </summary>
        Appearance,

        /// <summary>
        /// バグ修正タスクです。
        /// </summary>
        Bug,

        /// <summary>
        /// テストに関するタスクです。
        /// </summary>
        Test,
    }
}