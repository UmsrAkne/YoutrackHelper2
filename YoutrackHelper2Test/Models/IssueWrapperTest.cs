using System.Linq;
using NUnit.Framework;
using YoutrackHelper2.Models;

namespace YoutrackHelper2Test.Models
{
    public class IssueWrapperTest
    {
        [Test]

        // 通常の入力 （タイプあり）
        [TestCase("タイトル, 説明, バグ", "タイトル", "説明", WorkType.Bug)]

        // 通常の入力 (タイプなし)
        [TestCase("タイトル, 説明", "タイトル", "説明", WorkType.Feature)]

        // 説明文なし
        [TestCase("タイトル,", "タイトル", "", WorkType.Feature)]

        // タイトルの前方にスペース
        [TestCase("  タイトル,", "タイトル", "", WorkType.Feature)]

        // タイプを手前に配置したケースで説明文なし
        [TestCase("バグ, タイトル", "タイトル", "", WorkType.Bug)]

        // タイプを手前に配置したケースで説明文あり
        [TestCase("バグ, タイトル, 説明", "タイトル", "説明", WorkType.Bug)]

        // タイトルと説明文の間にタイプを配置したケース
        [TestCase("タイトル, バグ", "タイトル", "", WorkType.Bug)]

        // 説明文を２回配置。最初の説明文が入力されるはず
        [TestCase("バグ, タイトル, 説明, 二回目の説明", "タイトル", "説明", WorkType.Bug)]
        public void ToIssueWrapperTest(string text, string title, string description, WorkType type)
        {
            // ToIssueWrapper() が意図した通りに文字列を変換できるか確認します。
            // WorkType は未入力の場合は WorkType.Feature が割り当てられるため、今回は WorkType.Bug がセットして確認しています。

            var w1 = IssueWrapper.ToIssueWrapper(text);
            Assert.AreEqual(title, w1.Title);
            Assert.AreEqual(description, w1.Description);
            Assert.AreEqual(type, w1.WorkType);
        }

        [Test]
        public void ToIssueWrapperTest_タグの読み込み()
        {
            // ToIssueWrapper() が意図した通りに文字列を変換できるか確認します。
            // WorkType は未入力の場合は WorkType.Feature が割り当てられるため、今回は WorkType.Bug がセットして確認しています。

            var w1 = IssueWrapper.ToIssueWrapper( "タイトル, 説明, バグ, #tag #Test Tag");
            Assert.AreEqual("タイトル", w1.Title);
            Assert.AreEqual("説明", w1.Description);
            Assert.AreEqual(WorkType.Bug, w1.WorkType);

            Assert.AreEqual("tag", w1.Tags.ToList()[0].Text, "タグとして認識されているか？");
            Assert.AreEqual("Test Tag", w1.Tags.ToList()[1].Text, "間に半角スペースが含まれても問題ないか？");
        }
    }
}