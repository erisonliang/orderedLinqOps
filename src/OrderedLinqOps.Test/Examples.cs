using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


namespace OrderedLinqOps.Test
{
    /// <summary>
    /// For use on the project documentation
    /// </summary>
    [TestFixture]
    public class Examples
    {
        class Pet
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Test]
        public static void OrderedGroupByExam1()
        {
            var pets = new[] {
                new Pet { Name="Whiskers", Age=1 },
                new Pet { Name="Boots", Age=4 },
                new Pet { Name="Daisy", Age=4 },
                new Pet { Name="Barley", Age=8 } };

            // Group the pets using Age as the key value and selecting only the pet's Name for each value.
            var query = pets.OrderedGroupBy(pet => pet.Age, pet => pet.Name);

            foreach (var cohort in query)
            {
                Console.WriteLine(cohort.Key);
                foreach (var name in cohort)
                    Console.WriteLine("  {0}", name);
            }
        }

        /*
         This code produces the following output:
            1
              Whiskers
            4
              Boots
              Daisy
            8
              Barley
        */
    }
}