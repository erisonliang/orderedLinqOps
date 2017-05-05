using System;
using System.Linq;
using NUnit.Framework;


namespace StreamingOperators.Test
{
    [TestFixture]
    public class StreamingOperatorsTests
    {
        #region MergeJoin
        [Test]
        public void OrderedJoin_SimpleHappyPath()
        {
            var inner = new[] { 1, 2, 2, 3 };
            var outer = new[] { 1, 2, 4 };

            var expected = new[] { 1, 2, 2 };

            var actual = outer.MergeJoin(inner, i => i, i => i, (i, j) => i);

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void OrderedJoin_ThrowsWhenInnerUnordered()
        {
            var inner = new[] { 1, 3, 2 };
            var outer = new[] { 1, 2 };

            Assert.Throws<ArgumentException>(() => outer.MergeJoin(inner, i => i, i => i, (i, j) => i).ToList());
        }

        [Test]
        public void OrderedJoin_ThrowsWhenOuterUnordered()
        {
            var inner = new[] { 1, 2, 3 };
            var outer = new[] { 1, 3, 2 };

            Assert.Throws<ArgumentException>(() => outer.MergeJoin(inner, i => i, i => i, (i, j) => i).ToList());
        }
        #endregion MergeJoin

        #region GroupMergeJoin
        [Test]
        public void OrderedGroupJoin_SimpleHappyPath()
        {
            var inner = new[] { 1, 2, 3, 3, 3 };
            var outer = new[] { 1, 2, 2, 3 };

            var expected = new[]
            {
                new { i = 1, j = 1 },
                new { i = 2, j = 1 },
                new { i = 2, j = 1 },
                new { i = 3, j = 3 }
            };

            var actual = outer.GroupMergeJoin(inner, i => i, i => i, (i, j) => new { i, j = j.Count() });

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void OrderedGroupJoin_ThrowsWhenInnerUnordered()
        {
            var inner = new[] { 1, 3, 2 };
            var outer = new[] { 1, 2 };

            Assert.Throws<ArgumentException>(() => outer.GroupMergeJoin(inner, i => i, i => i, (i, j) => i).ToList());
        }

        [Test]
        public void OrderedGroupJoin_ThrowsWhenOuterUnordered()
        {
            var inner = new[] { 1, 2, 3 };
            var outer = new[] { 1, 3, 2 };

            Assert.Throws<ArgumentException>(() => outer.GroupMergeJoin(inner, i => i, i => i, (i, j) => i).ToList());
        }
        #endregion GroupMergeJoin
    }
}