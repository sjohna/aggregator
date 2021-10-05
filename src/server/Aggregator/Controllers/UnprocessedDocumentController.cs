using AggregatorLib;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnprocessedDocumentController : ControllerBase
    {
        private AggregatorSystem system;

        public UnprocessedDocumentController(AggregatorSystem system)
        {
            this.system = system;
        }

        // TODO: unit test various cases
        // GET: api/<UnprocessedDocumentController>
        [HttpGet]
        public IEnumerable<UnprocessedDocument> Get(string? filter, string? sort, bool? sortDescending)
        {
            // TODO: pagination
            // TODO: error handling?
            if (!sortDescending.HasValue || !sortDescending.Value)
            {
                return system.UnprocessedDocumentRepository.Query(Where: filter, OrderByAsc: sort);
            }
            else
            {
                return system.UnprocessedDocumentRepository.Query(Where: filter, OrderByDesc: sort);
            }
        }

        // GET api/<UnprocessedDocumentController>/5
        [HttpGet("{id}")]
        public UnprocessedDocument Get(Guid id)
        {
            // TODO: error handling
            return system.UnprocessedDocumentRepository.GetById(id);
        }

        //// POST api/<UnprocessedDocumentController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<UnprocessedDocumentController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<UnprocessedDocumentController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
