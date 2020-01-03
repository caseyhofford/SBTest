using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherEF
{
    public class WeatherContext : DbContext
    {
        public WeatherContext(DbContextOptions<WeatherContext> options)
            : base(options)
        { }

        public DbSet<Location> Location { get; set; }
        public DbSet<Reading> Reading { get; set; }
        public DbSet<Day> Day { get; set; }
        public DbSet<WeatherType> WeatherType { get; set; }
    }

    public class Location
    {
        [Key]
        public int Zip { get; set; }
        public string City { get; set; }
        public decimal  Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string TimeZone { get; set; }
        
        public ICollection<Reading> Reading { get; set; }
        public ICollection<Day> Day { get; set; }
    }
    public class Reading
    {
        [Key]
        public int ReadingID { get; set; }
        public int LocationZipID { get; set; }
        public DateTime ReadingDateTime { get; set; }
        public int WeatherTypeID { get; set; }
        public decimal WindSpeed { get; set; }
        public short WindDirection { get; set; }
        public decimal WindGust { get; set; }
        public decimal Temperature { get; set; }
        public byte Clouds { get; set; }
        public int DayID { get; set; }

        [ForeignKey("LocationZipID")]
        public Location Location { get; set; }
    }

    public class Day
    {
        [Key]
        public int DayID { get; set; }
        public int LocationZipID { get; set; }
        public DateTime Date { get; set; }
        public string TimeZone { get; set; }

        public ICollection<Reading> Readings { get; set; }
    }
    public class WeatherType
    {
        [Key]
        public int WeatherTypeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Reading> Readings { get; set; }
    }

    public class WeatherContextFactory : IDesignTimeDbContextFactory<WeatherContext>
    {
        public WeatherContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WeatherContext>();
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("sqldb_connection"));
            Console.WriteLine("**Weather Context Factory**");

            return new WeatherContext(optionsBuilder.Options);
        }
    }
}
