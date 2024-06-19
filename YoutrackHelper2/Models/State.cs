namespace YoutrackHelper2.Models
{
    public enum State
    {
        /// <summary>
        /// タスクが未完了の状態です。
        /// </summary>
        Incomplete,

        /// <summary>
        /// タスクが完了した状態です。
        /// </summary>
        Completed,

        /// <summary>
        /// タスクが作業中の状態です。
        /// </summary>
        Progressing,

        /// <summary>
        /// タスクが廃止された状態です。
        /// </summary>
        Obsolete,
    }
}