using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;


namespace StreamingOperators.Test
{
    [TestFixture]
    public class GroupingTests
    {
        [Test, TestCaseSource(nameof(TestCases))]
        public void OrderedGroupBy_CompareToLinq(int[] input)
        {
            var groupedByLinq = input.GroupBy(i => i);
            var streamGrouped = input.OrderedGroupBy(i => i);

            CollectionAssert.AreEquivalent(groupedByLinq, streamGrouped);
        }

        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new[] { 1, 2, 2, 3 }).SetName("SimpleHappyPath");
                yield return new TestCaseData(new int[] { }).SetName("Empty");
                yield return new TestCaseData(new[] { 1 }).SetName("One Entry");
                yield return new TestCaseData(new[] { 1, 2 }).SetName("Two entries");
                yield return new TestCaseData(new[] { 1, 1, 2, 3 }).SetName("Group at the beginning");
                yield return new TestCaseData(new[] { 1, 2, 3, 3 }).SetName("Group at the end");
            }
        }

        [Test]
        public void OrderedGroupBy_ThrowsWhenUnordered()
        {
            var input = new[] { 1, 3, 2 };

            Assert.Throws<ArgumentException>(() => input.OrderedGroupBy(i => i).ToList());
        }
    }
}