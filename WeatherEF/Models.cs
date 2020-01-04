using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>().HasData(
                new Location
                {
                    Zip = 98177,
                    City = "Seattle",
                    Latitude = new decimal(47.75),
                    Longitude = new decimal(-122.37),
                    TimeZone = "Pacific Standard Time"
                },
                new Location
                {
                    Zip = 90004,
                    City = "Los Angeles",
                    Latitude = new decimal(34.08),
                    Longitude = new decimal(-118.3),
                    TimeZone = "Pacific Standard Time"
                }, new Location
                {
                    Zip = 80904,
                    City = "Colorado Springs",
                    Latitude = new decimal(38.85),
                    Longitude = new decimal(-104.86),
                    TimeZone = "Mountain Standard Time"
                }
            );
        }
    }

    public class Location
    {
        [Key]
        public int Zip { get; set; }
        public string City { get; set; }
        public decimal Latitude { get; set; }
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
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]
        public int WeatherTypeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Reading> Readings { get; set; }
    }

    public class WeatherContextFactory : IDesignTimeDbContextFactory<WeatherContext>
    {
        public IConfigurationRoot Configuration { get; set; }

        public WeatherContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true)
                .AddJsonFile("secret.settings.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            var optionsBuilder = new DbContextOptionsBuilder<WeatherContext>();
            optionsBuilder.UseSqlServer(Configuration["sqldb_connection"]);
            Console.WriteLine("**Weather Context Factory**");

            return new WeatherContext(optionsBuilder.Options);
        }
    }
}
