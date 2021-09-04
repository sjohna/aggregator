using Aggregator.Models;
using AggregatorLib;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawContentController : ControllerBase
    {
        private AggregatorSystem system;

        public RawContentController(AggregatorSystem system)
        {
            this.system = system;
        }

        // GET: api/<RawContentController>
        [HttpGet]
        public IEnumerable<RawContent> Get()
        {
            // TODO: pagination
            // TODO: error handling?
            return system.RawContentRepository.GetAllRawContent();
        }

        // GET api/<RawContentController>/5
        [HttpGet("{id}")]
        public RawContent Get(Guid id)
        {
            // TODO: error handling
            return system.RawContentRepository.GetRawContentById(id);
        }

        [HttpPost("process")]
        public AggregatorSystem.ProcessedContentAdditions Process(RawContentTransferObject contentTransferObject)
        {
            var content = new RawContent(Id: Guid.NewGuid(),
                                         RetrieveTime: Instant.FromUnixTimeMilliseconds(contentTransferObject.RetrieveTime),
                                         Type: contentTransferObject.Type,
                                         Content: contentTransferObject.Content,
                                         Context: contentTransferObject.Context,
                                         SourceUri: contentTransferObject.SourceUri);

            // TODO: error handling...
            return system.ProcessRawContent(content);
        }

        //// POST api/<RawContentController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<RawContentController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<RawContentController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
