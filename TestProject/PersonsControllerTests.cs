using Microsoft.EntityFrameworkCore;
using System;
using TestApp.Model;
using Xunit;
using Microsoft.EntityFrameworkCore.InMemory;
using TestApp.Controllers;
using Microsoft.Extensions.Caching.Memory;
using TestApp;
using System.Collections.Generic;

namespace TestProject
{
    public class PersonsControllerTests : IDisposable
    {

        private static readonly DbContextOptions<PersonDbContext> dbContextOptions = new DbContextOptionsBuilder<PersonDbContext>()
            .UseInMemoryDatabase(databaseName: "MyDb")
            .Options;

        PersonDbContext context;
        public PersonsControllerTests()
        {
            context = new PersonDbContext(dbContextOptions);
            context.Database.EnsureCreated();
            SeedDb();
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
        }

        private void SeedDb()
        {
            Person p1 = new Person
            {
                Id = 1,
                FirstName = "Priti",
                LastName = "Valesha",
                MiddleName = "Batra",
                Age = 20,
                Married = "True"
            };
            Person p2 = new Person
            {
                Id = 2,
                FirstName = "Manoj",
                LastName = "Valesha",
                MiddleName = "G",
                Age = 20,
                Married = "False"
            };
            context.Persons.Add(p1);
            context.Persons.Add(p2);
            context.SaveChanges();

        }

        [Fact]
        public void TestDeleteAll()
        {
            var controller = new PersonsController(context);
            controller.DeleteAll();
            Assert.Null(controller.GetPersons());
        }

        [Fact]
        public void TestDelete()
        {
            int id = 1;
            var controller = new PersonsController(context);
            controller.Delete(id);
            var person = controller.GetPersonById(id);
            Assert.Null(person);
        }

        [Fact]
        public void TestGetPersonById()
        {
            int id = 2;
            var controller = new PersonsController(context);
            var person = controller.GetPersonById(id);
            Assert.NotNull(person);
        }
    }
}
