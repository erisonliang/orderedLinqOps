using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;


namespace OrderedLinqOps.Test
{
    [TestFixture]
    public class OrderedGroupJoinTests
    {
        [Test, TestCaseSource(nameof(TestCases))]
        public void OrderedGroupJoin_CompareToLinq(int[] outer, int[] inner)
        {
            var innerValues = inner.Select((i, o) => (key: i, val: i + "i" + o)).ToList();
            var outerValues = outer.Select((i, o) => (key: i, val: i + "o" + o)).ToList();

            var actual = outerValues.OrderedGroupJoin(innerValues, i => i.key, j => j.key, (i, j) => (i.val, string.Join(",", j.Select(o => o.val))));
            var expected = outerValues.GroupJoin(innerValues, i => i.key, j => j.key, (i, j) => (i.val, string.Join(",", j.Select(o => o.val))));

            CollectionAssert.AreEquivalent(expected, actual);
        }

        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new int[] { }, new[] { 1, 2 }).SetName("Outer Empty");
                yield return new TestCaseData(new[] { 1, 2 }, new int[] { }).SetName("Inner Empty");
                yield return new TestCaseData(new[] { 1 }, new[] { 1 }).SetName("One Entry");
                yield return new TestCaseData(new[] { 1, 1, 1, 2, 3, 4 }, new[] { 1, 4 }).SetName("Outer Group at the beginning");
                yield return new TestCaseData(new[] { 1, 2, 3, 4, 4, 4 }, new[] { 1, 4 }).SetName("Outer Group at the end");
                yield return new TestCaseData(new[] { 1, 2, 3, 3, 3, 4 }, new[] { 1, 3, 4 }).SetName("Outer Group inbetween");
                yield return new TestCaseData(new[] { 1, 4 }, new[] { 1, 1, 1, 2, 3, 4 }).SetName("Inner Group at the beginning");
                yield return new TestCaseData(new[] { 1, 4 }, new[] { 1, 2, 3, 4, 4, 4 }).SetName("Inner Group at the end");
                yield return new TestCaseData(new[] { 1, 3, 4 }, new[] { 1, 2, 3, 3, 3, 4 }).SetName("Inner Group inbetween");
            }
        }

        [Test]
        public void OrderedGroupJoin_ThrowsWhenInnerUnordered()
        {
            var inner = new[] { 1, 3, 2 };
            var outer = new[] { 1, 2 };

            Assert.Throws<ArgumentException>(() => outer.OrderedGroupJoin(inner, i => i, i => i, (i, j) => i).ToList());
        }

        [Test]
        public void OrderedGroupJoin_ThrowsWhenOuterUnordered()
        {
            var inner = new[] { 1, 2, 3 };
            var outer = new[] { 1, 3, 2 };

            Assert.Throws<ArgumentException>(() => outer.OrderedGroupJoin(inner, i => i, i => i, (i, j) => i).ToList());
        }
    }
}