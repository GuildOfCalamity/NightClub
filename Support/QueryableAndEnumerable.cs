using System;
using System.Collections.Generic;
using System.Linq;

namespace NightClub;

public class Customer
{
    public int Revenue { get; set; }
    public Customer(int revenue)
    {
        Revenue = revenue;
    }
}

public class ORM // ObjectRelationalMapper is representing our DB model/entity
{
    readonly int _max = 500000;
    public int Max { get => _max; }

    /// <summary>
    /// This is our IList<T> data. 
    /// </summary>
    public List<Customer> Customers { get; set; } = new List<Customer>();

    public ORM()
    {
        System.Diagnostics.Debug.WriteLine($"> Creating {_max} {nameof(Customer)} records...");
        Enumerable.Range(1, _max).ForEach(idx => { Customers.Add(new Customer(Random.Shared.Next(1, _max + 1))); });
    }

    /// <summary>
    /// Test method #1
    /// </summary>
    /// <returns><see cref="IEnumerable{Customer}"/> for collection</returns>
    public IEnumerable<Customer> GetCustomersAsEnumerable()
    {
        return Customers.AsEnumerable();
    }

    /// <summary>
    /// Test method #2
    /// </summary>
    /// <returns><see cref="IQueryable{Customer}"/> for speed</returns>
    public IQueryable<Customer> GetCustomersAsQueryable()
    {
        return Customers.AsQueryable();
    }
}
