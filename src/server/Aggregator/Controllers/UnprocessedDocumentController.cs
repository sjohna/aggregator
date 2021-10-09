using Aggregator.Models;
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

        // TODO: test various cases
        // TODO: test pagination
        // GET: api/<UnprocessedDocumentController>
        [HttpGet]
        public Page<UnprocessedDocument> Get(string? filter = null, string? sort = null, bool? sortDescending = null, int pageSize = 10, int offset = 0)
        {
            IEnumerable<UnprocessedDocument> queryResult;
            int total = system.UnprocessedDocumentRepository.Count(filter);

            // TODO: error handling?
            if (!sortDescending.HasValue || !sortDescending.Value)
            {
                queryResult =  system.UnprocessedDocumentRepository.Query(Where: filter, OrderByAsc: sort, Limit: pageSize, Offset: offset);
            }
            else
            {
                queryResult = system.UnprocessedDocumentRepository.Query(Where: filter, OrderByDesc: sort, Limit: pageSize, Offset: offset);
            }

            return new Page<UnprocessedDocument>(
                    PageSize: pageSize,
                    Offset: offset,
                    Total: total,
                    Items: queryResult.ToList()
                );
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
