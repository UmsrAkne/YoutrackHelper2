using System.Collections.Generic;
using System.Linq;
using YouTrackSharp.Issues;

namespace YoutrackHelper2.Models
{
    /// <summary>
    /// YouTrackSharp.Issues の Issue のフィールドから値を取り出すメソッドを提供します
    /// </summary>
    public static class ValueGetter
    {
        /// <summary>
        /// Issue から指定したキーのフィールドを検索して値を返します.
        /// </summary>
        /// <param name="issue">Issue を入力します</param>
        /// <param name="valueKey">取り出したいフィールドの名前を入力します</param>
        /// <returns>指定されたキーに格納されている値を返します。指定のキーが存在しない場合は string.Empty を返します</returns>
        public static string GetString(Issue issue, string valueKey)
        {
            var field = issue.Fields.FirstOrDefault(f => f.Name == valueKey);
            if (field == null)
            {
                return string.Empty;
            }

            return field.Value is List<string> values ? values.FirstOrDefault() : string.Empty;
        }
    }
}