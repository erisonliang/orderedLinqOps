using System;
using System.Linq;
using NUnit.Framework;


namespace StreamingOperators.Test
{
    [TestFixture]
    public class GroupingTests
    {
        [Test]
        public void OrderedGroupBy_SimpleHappyPath()
        {

            var input = new[] { 1, 2, 2, 3 };

            var groupedByLinq = input.GroupBy(i => i);

            var streamGrouped = input.OrderedGroupBy(i => i);

            CollectionAssert.AreEquivalent(groupedByLinq, streamGrouped);
        }

        [Test]
        public void OrderedGroupBy_ThrowsWhenUnordered()
        {
            var input = new[] { 1, 3, 2 };

            Assert.Throws<ArgumentException>(() => input.OrderedGroupBy(i => i).ToList());
        }
    }
}