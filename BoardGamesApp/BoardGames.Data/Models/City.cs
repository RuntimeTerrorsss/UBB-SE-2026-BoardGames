using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

[Table("cities")]
public class City
{
    [SetsRequiredMembers]
    public City(string mainName, List<string> names, double latitude, double longitude)
    {
        MainName = mainName;
        Names = names;
        Latitude = latitude;
        Longitude = longitude;
    }

    public City()
    {
    }

    [Key]
    [Column("id")]
    public int CityId { get; set; }

    [Column("main_name")]
    required public string MainName { get; set; }

    [Column("names")]
    required public List<string> Names { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }
}