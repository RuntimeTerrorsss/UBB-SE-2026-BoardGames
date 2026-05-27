// <copyright file="Address.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Owned]
public class Address
{
    public Address(string country, string city, string street, string streetNumber)
    {
        this.Country = country;
        this.City = city;
        this.Street = street;
        this.StreetNumber = streetNumber;
    }

    public Address()
    {
    }

    [Column("country")]
    public string Country { get; set; } = string.Empty;

    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Column("street")]
    public string Street { get; set; } = string.Empty;

    [Column("street_number")]
    public string StreetNumber { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{this.Street} {this.StreetNumber}, {this.City}, {this.Country}";
    }
}
