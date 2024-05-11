using NUnit.Framework;
using YoutrackHelper2.Models;

namespace YoutrackHelper2Test.Models
{
    public class IssueWrapperTest
    {
        [Test]
        [TestCase("タイトル, 説明, バグ", "タイトル", "説明", WorkType.Bug)]
        [TestCase("タイトル, 説明", "タイトル", "説明", WorkType.Feature)]
        [TestCase("タイトル,", "タイトル", "", WorkType.Feature)]
        [TestCase("  タイトル,", "タイトル", "", WorkType.Feature)]
        [TestCase("バグ, タイトル", "タイトル", "", WorkType.Bug)]
        [TestCase("バグ, タイトル, 説明", "タイトル", "説明", WorkType.Bug)]
        [TestCase("タイトル, バグ", "タイトル", "", WorkType.Bug)]
        public void ToIssueWrapperTest(string text, string title, string description, WorkType type)
        {
            // ToIssueWrapper() が意図した通りに文字列を変換できるか確認します。
            // WorkType は未入力の場合は WorkType.Feature が割り当てられるため、今回は WorkType.Bug がセットして確認しています。

            var w1 = IssueWrapper.ToIssueWrapper(text);
            Assert.AreEqual(title, w1.Title);
            Assert.AreEqual(description, w1.Description);
            Assert.AreEqual(type, w1.WorkType);
        }
    }
}