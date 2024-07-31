using System;
using NUnit.Framework;
using YoutrackHelper2.Models;
using YoutrackHelper2.Views;
using YoutrackHelper2.Views.Converters;

namespace YoutrackHelper2Test.Views
{
    [TestFixture]
    public class IssueStatusConverterTest
    {
        [Test]
        public void Nullの場合()
        {
            var converter = new IssueStatusConverter();
            Assert.That(
                converter.Convert(null, null, null, null),
                Is.EqualTo(string.Empty));
        }

        [Test]
        public void 作業中の場合()
        {
            var converter = new IssueStatusConverter();
            var iw = new IssueWrapper
            {
                Progressing = true,
                WorkingDuration = TimeSpan.FromMinutes(1),
            };

            Assert.That(
                converter.Convert(iw, null, null, null),
                Is.EqualTo("00:01:00"));
        }
    }
}