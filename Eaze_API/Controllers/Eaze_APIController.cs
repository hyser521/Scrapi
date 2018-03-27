using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eaze_API.models;
using Quartz;
using Newtonsoft.Json;
namespace Eaze_API.Controllers
{
    [Produces("application/json")]
    [Route("api/eazeapi")]
    public class Eaze_APIController : Controller
    {
        private readonly ScopedSchedulerContext __context;
        private const string defaultGroup = Quartz.Util.Key<JobKey>.DefaultGroup;
        public Eaze_APIController(ScopedSchedulerContext context)
        {
            __context = context;
        }

        /// <summary>
        /// Returns the job status from the scheduler.
        /// </summary>
        /// <param name="id">job ID</param>
        /// <returns>JSON object containing the status</returns>
        /// <response code="200">A response message wrapped in JSON</response>
        /// <response code="400">ID not found</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpGet("{id}", Name ="GetJobStatus")]
        public async Task<IActionResult> GetJobStatus(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || ! await __context.scheduler.CheckExists(JobKey.Create(id, defaultGroup)))
                return new StatusCodeResult(400);
            TriggerState status = await __context.scheduler.GetTriggerState(new TriggerKey(id,defaultGroup));
            IJobDetail detail = await __context.scheduler.GetJobDetail(JobKey.Create(id, defaultGroup));
            string message = "";
            
             switch((int) status)
            {
                case 0: message = "There are no errors related to this job"; break;
                case 1: message = "The job has been paused.  Check back later."; break;
                case 2: message = "The job has completed"; break;
                case 3:
                    return new StatusCodeResult(500);
                case 4: message = "The job is waiting to execute."; break;
                case 5: message = detail.JobDataMap.GetString("COMPLETION_STATUS"); break;
            }
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("status", message);
            return new OkObjectResult(response); 
        }

        /// <summary>
        /// Returns the scraping result or error from attempting to scrape
        /// </summary>
        /// <param name="id">job ID</param>
        /// <response code="200">A response message wrapped in JSON</response>
        /// <response code="400">ID not found</response>
        [ProducesResponseType(typeof(Dictionary<string,string>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("{id}/results", Name = "GetJobResults")]
        public async Task<IActionResult> GetJobResults(string id)
        {
            if (string.IsNullOrWhiteSpace(id) ||!await __context.scheduler.CheckExists(JobKey.Create(id, defaultGroup)))
                return new StatusCodeResult(400);
            IJobDetail detail = await __context.scheduler.GetJobDetail(JobKey.Create(id,defaultGroup));
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("result", detail.JobDataMap.GetString("SCRAPE"));
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Adds a job for the URL specified in the message body
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/eazeapi
        ///     {
        ///        "url": "https://wwww.google.com"
        ///     }
        ///
        /// </remarks>
        /// <param name="json">JSON-wrapped URL string</param>
        /// <response code="201">JSON-wrapped ID string</response>
        /// <response code="400">No URL in message</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(typeof(Dictionary<string,string>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost]
        public async Task<IActionResult> PostJob([FromBody]Dictionary<string,string> json)
        {
            try
            {
                string url = "";
                if (!json.TryGetValue("url", out url))
                    return new StatusCodeResult(400);
                string id = Guid.NewGuid().ToString();
                ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(id, defaultGroup)
                        .StartNow()
                        .Build();

                IJobDetail job = JobBuilder.Create<WebScraperJob>()
                        .WithIdentity(id, defaultGroup)
                        .UsingJobData("URL", url)
                        .StoreDurably(true)
                        .Build();
                await __context.scheduler.ScheduleJob(job, trigger);
                Dictionary<string, string> response = new Dictionary<string, string>();
                response.Add("id", id);
                return CreatedAtRoute("GetJobStatus", new { id },response);
                


            }
            catch (SchedulerException se)
            {
                return StatusCode(500,se);
            }
        }
        
    }
}
