using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApp.Model;
using Z.EntityFramework.Plus;

namespace TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        private readonly PersonDbContext _context;
        CacheService _cacheService = new CacheService();
        public PersonsController(PersonDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public List<Person> GetPersons()
        {
            var cacheData = _cacheService.GetData<List<Person>>("persons");
            if (cacheData != null)
            {
                return cacheData;
            }
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            cacheData = _context.Persons.ToList();
            _cacheService.SetData<IEnumerable<Person>>("persons", cacheData, expirationTime);
            return cacheData;
        }

        [HttpGet("{id}")]
        public Person GetPersonById(int id)
        {
            Person filteredData;
            var cacheData = _cacheService.GetData<List<Person>>("persons");
            if (cacheData != null)
            {
                filteredData = cacheData.Where(x => x.Id == id).FirstOrDefault();
                return filteredData;
            }
            filteredData = _context.Persons.Where(x => x.Id == id).FirstOrDefault();
            return filteredData;
        }

        [HttpDelete]
        public IActionResult DeleteAll()
        {
            var cacheData = _cacheService.GetData<List<Person>>("persons");
            _context.Persons.RemoveRange(cacheData);
            _context.SaveChanges();
            _cacheService.RemoveData("persons");
            return Ok("person deleted");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _context.Persons.SingleOrDefault(p => p.Id == id);
            if (result == null)
            {
                return NotFound("Person doesn't exist");
            }
            else
            {
                _context.Persons.Remove(result);
                _context.SaveChanges();
                _cacheService.RemoveData("persons");
                return Ok("person deleted");
            }
        }

        [HttpPost]
        public  IActionResult AddPerson(Person person)
        {

            _context.Persons.Add(person);
            _context.SaveChanges();
            _cacheService.RemoveData("persons");
            return Created("api/persons/" + person.Id, person);
        }

        #region Update All
        [HttpPut]
        public IActionResult UpdateAll([FromBody]List<Person> persons, [FromQuery]int[] ids)
        {
            _context.Persons.Where(p => ids.Contains(p.Id))
                .Update(p => new Person() { Age = p.Age, FirstName = p.FirstName, LastName=p.LastName,Married = p.Married });
            _context.SaveChanges();
            _cacheService.RemoveData("persons");
            return Ok("Persons update successfully");

        }
        #endregion

        [HttpPut("{id}")]
        public IActionResult Update(int id, Person person)
        {
            var result = _context.Persons.SingleOrDefault(p => p.Id == id);
            if (result == null)
            {
                return NotFound("Person doesn't exist");
            }
            else
            {
                result.FirstName = person.FirstName != null ? person.FirstName : result.FirstName;
                result.LastName = person.LastName != null ? person.LastName : result.LastName;
                result.MiddleName = person.MiddleName != null ? person.MiddleName : result.MiddleName;
                result.Age = person.Age != 0 ? person.Age : result.Age;
                result.Married = person.Married != null ? person.Married : result.Married;

                _context.Update(result);
                _context.SaveChanges();
                _cacheService.RemoveData("persons");
                return Ok("Person update successfully");
            }
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] JsonPatchDocument<Person> person)
        {
            if (person != null)
            {
                var result = _context.Persons.SingleOrDefault(p => p.Id == id);
                if (result == null)
                {
                    return NotFound("Person doesn't exist");
                }
                else
                {
                    person.ApplyTo(result);
                    _cacheService.RemoveData("persons");
                    return NoContent();
                }
            }
            else
            {
                return NotFound("input is null");
            }
        }

    }
}
