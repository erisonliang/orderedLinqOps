# orderedLinqOps
Alternatives to some hashing-based LINQ operators (GroupBy, Join), based on ordered inputs. Available as a nuget.

The advantage of these operators is they don't buffer and are faster.
The limitation is that they only operate on pre-sorted input.

## Installing
[It's a nuget!](https://www.nuget.org/packages/OrderedLinqOps/)

## Usage
The example is inspired by the one at LINQs GroupBy, and it is almost equivalent. The one important difference is that the input collection has to be pre-sorted by age. If it's not, an exception is thrown during the iteration.

```C#
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
```

## Motivation
LINQ operators, based on hashing and "true-false equality", are good enough for most purposes. 

Sometimes however, you are working with big data that you can't or won't buffer all in memory, but you can source them ordered. For example, when they come from a SQL database. You put the "ORDER BY" clause in the SQL query, and then in C# you group/join the data as they come and go, without overflowing memory. All you need to do is define an "ordered equality" comparer, equivalent to the SQL ordering rules.

Also, generally, in cases you have pre-ordered data, it makes sense to use that fact to speed up processing. Without the need to pre-build a hashtable, and without the hash-lookups, order-based processing will be much faster than normal LINQs hash-based one.

## See also
Stephen Cleary's [Comparers](https://github.com/StephenCleary/Comparers) library for easy declarative creation of both hashing and sorting comparers.

## License
This project is licensed under LGPL 3.0 - see the [LICENSE.md](LICENSE.md) file for details

