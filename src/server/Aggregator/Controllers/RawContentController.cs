using Aggregator.Models;
using AggregatorLib;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            try
            {
                return system.RawContentRepository.GetAllRawContent().ToList(); // need to convert to list before returning. Something doesn't play nice if the LiteDB deserialization happens within the framework...
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // GET api/<RawContentController>/5
        [HttpGet("{id}")]
        public RawContent Get(Guid id)
        {
            // TODO: error handling
            return system.RawContentRepository.GetRawContentById(id);
        }

        [HttpPost("Process")]
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

        [HttpPost("Download")]
        public AggregatorSystem.ProcessedContentAdditions Download(DownloadRawContentTransferObject downloadRawContentTransferObject)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadRawContentTransferObject.SourceUri);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var content = new RawContent(Id: Guid.NewGuid(),
                             RetrieveTime: NodaTime.SystemClock.Instance.GetCurrentInstant(),
                             Type: downloadRawContentTransferObject.Type,
                             Content: reader.ReadToEnd(),
                             Context: downloadRawContentTransferObject.Context,
                             SourceUri: downloadRawContentTransferObject.SourceUri);

                // TODO: error handling...
                return system.ProcessRawContent(content);
            }
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
