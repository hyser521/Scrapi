using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace Eaze_API.models
{
    /// <summary>
    /// Dependency injected class that handles scheduling and acts as the database.
    /// Created using Quartz .NET package.
    /// </summary>
    public class ScopedSchedulerContext
    {
        public IScheduler scheduler;
        public ScopedSchedulerContext()
        {
            NameValueCollection props = new NameValueCollection
                    {
                        { "quartz.serializer.type", "binary" }
                    };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);

            scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

            scheduler.Start();

        }
    }
}
