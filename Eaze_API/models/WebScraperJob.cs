using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Quartz;

namespace Eaze_API.models
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class WebScraperJob : IJob
    {

        /// <summary>
        /// Uses HttpClient to scrape the contents of a website into a string, stored in the context of the job
        /// </summary>
        /// <param name="context"> Context of the batched off job</param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap values = context.JobDetail.JobDataMap;
            try
            {

                string url = values.GetString("URL");
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                string contents;
                if (response.IsSuccessStatusCode)
                {
                    contents = await response.Content.ReadAsStringAsync();

                }
                else
                {
                    contents = "Received Status code of " + response.StatusCode.ToString();
                }
                values.Put("SCRAPE", contents);
                values.Put("COMPLETION_STATUS", "Complete");
            }
            catch (Exception e)
            {
                values.Put("SCRAPE", e.Message);
                values.Put("COMPLETION_STATUS", "Complete with Errors");
            }
        }
    }
}
