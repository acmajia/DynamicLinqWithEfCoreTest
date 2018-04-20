﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace DynamicLinqWithEfCoreTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<Person>();

            list.Add(new Person
            {
                Age = 1,
                EnglishName = "sadfasdf",
                Id = Guid.NewGuid(),
                Name = "tom"
            });

            list.Add(new Person
            {
                Age = 2,
                EnglishName = "ddddd",
                Id = Guid.NewGuid(),
                Name = "jerry"
            });

            var sw = Stopwatch.StartNew();

            using (var context = new TestContext())
            {
                context.Set<Person>().AddRange(list);
                context.SaveChanges();
            }

            for (var i = 0; i <= 10_000; i++)
            {
                using (var context = new TestContext())
                {
                    var set = context.Set<Person>().Where(m => m.Age == 1 && m.Name == "tom").ToList();
                }
            }

            Console.WriteLine(sw.Elapsed);
            sw.Restart();

            for (var i = 0; i <= 10_000; i++)
            {
                using (var context = new TestContext())
                {
                    var set = context.Set<Person>().Where($"$.Age==@0 and $.Name==@1", 1, "tom").ToList();
                }
            }

            Console.WriteLine(sw.Elapsed);
            sw.Restart();

            var expression = DynamicExpressionParser.ParseLambda(typeof(Person), null, $"$.Age==@0 and $.Name==@1", 1, "tom");
            for (var i = 0; i <= 10_000; i++)
            {
                using (var context = new TestContext())
                {
                    var set = context.Set<Person>().Where(expression).ToDynamicList<Person>();
                }
            }
            Console.WriteLine(sw.Elapsed);

            Console.ReadLine();
        }
    }

    public class Person
    {
        public Guid Id { get; set; }
        public int Age { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
    }

    public class TestContext : DbContext
    {
        public TestContext()
        {

        }

        public TestContext(DbContextOptions<TestContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("test");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasKey(m => m.Id);
        }
    }
}
