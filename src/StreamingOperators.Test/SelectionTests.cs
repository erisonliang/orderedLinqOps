using System;
using System.Linq;
using NUnit.Framework;


namespace StreamingOperators.Test
{
    [TestFixture]
    public class SelectionTests
    {
        [Test]
        public void OrderedSelect_SimpleHappyPath()
        {

            var input = new[] { 1, 2, 3 };

            var actual = input.OrderedSelect(i => (decimal)i, (i, j) => j * i);
            var expected = new[] { 1m, 4m, 9m };

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void OrderedSelect_ThrowsWhenUnordered()
        {
            var input = new[] { 1, 3, 2 };
            
            Assert.Throws<ArgumentException>(() => input.OrderedSelect(i => (decimal)i, (i, j) => j * i).ToList());
        }
    }
}