using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
namespace Eaze_API_Tests
{
    [TestClass]
    public class Eaze_API_Tests
    {
        private Eaze_API.Controllers.Eaze_APIController _eazeAPI;
        public Eaze_API_Tests ()
        {
            _eazeAPI = new Eaze_API.Controllers.Eaze_APIController(new Eaze_API.models.ScopedSchedulerContext());
        }

        [TestMethod]
        public void SubmitURL()
        {
            Dictionary<string, string> jsonMock = new Dictionary<string, string>();
            jsonMock.Add("url", "https://wwww.google.com");
            CreatedAtRouteResult result = (CreatedAtRouteResult)  _eazeAPI.PostJob(jsonMock).Result;
            Assert.AreEqual(result.StatusCode,201);

        }

        [TestMethod]
        public void SubmitNothing()
        {
            Dictionary<string, string> jsonMock = new Dictionary<string, string>();
            StatusCodeResult result = (StatusCodeResult)_eazeAPI.PostJob(jsonMock).Result;
            Assert.AreEqual(400, result.StatusCode);

        }

        [TestMethod]
        public void TestSuccessfulStatusCall()
        {
            Dictionary<string, string> jsonMock = new Dictionary<string, string>();
            jsonMock.Add("url", "https://github.com/");
            CreatedAtRouteResult guid = (CreatedAtRouteResult) _eazeAPI.PostJob(jsonMock).Result;
            Dictionary<string,string> result = (Dictionary<string, string>)guid.Value;
            OkObjectResult status = (OkObjectResult) _eazeAPI.GetJobStatus(result["id"]).Result;
            Assert.AreEqual(200,status.StatusCode);
        }

        [DataTestMethod]
        [DataRow("Thisisn'taGUID")]
        [DataRow("")]
        public void TestFailedStatusCall(string id)
        {
            StatusCodeResult status = (StatusCodeResult) _eazeAPI.GetJobStatus(id).Result;
            Assert.AreEqual(400,status.StatusCode);
        }

        [TestMethod]
        public void TestSuccessfulResult()
        {
            Dictionary<string, string> jsonMock = new Dictionary<string, string>();
            jsonMock.Add("url", "https://github.com/");
            CreatedAtRouteResult guid = (CreatedAtRouteResult)_eazeAPI.PostJob(jsonMock).Result;
            Dictionary<string, string> id = (Dictionary<string, string>)guid.Value;
            Thread.Sleep(500);
            OkObjectResult status = (OkObjectResult)_eazeAPI.GetJobStatus(id["id"]).Result;
            Dictionary<string,string> status_value = (Dictionary<string, string>)status.Value;
            Assert.AreNotEqual("Complete with Errors", status_value["status"]);
            OkObjectResult result = (OkObjectResult)_eazeAPI.GetJobResults(id["id"]).Result;
            Assert.AreNotEqual(string.Empty, result.Value);
        }
        [TestMethod]
        public void TestFailedResult()
        {
            Dictionary<string, string> jsonMock = new Dictionary<string, string>();
            jsonMock.Add("url", "incorrectURLFormat");
            CreatedAtRouteResult guid = (CreatedAtRouteResult)_eazeAPI.PostJob(jsonMock).Result;
            Dictionary<string, string> id = (Dictionary<string, string>)guid.Value;
            Thread.Sleep(500);
            OkObjectResult status = (OkObjectResult)_eazeAPI.GetJobStatus(id["id"]).Result;
            Dictionary<string, string> status_value = (Dictionary<string, string>)status.Value;
            Assert.AreEqual("Complete with Errors", status_value["status"]);
            OkObjectResult result = (OkObjectResult)_eazeAPI.GetJobResults(id["id"]).Result;
            Assert.AreNotEqual(string.Empty, result.Value);
        }
        [DataTestMethod]
        [DataRow("Thisisn'taGUID")]
        [DataRow("")]
        public void TestNonExistentResult(string id)
        {
            StatusCodeResult status = (StatusCodeResult)_eazeAPI.GetJobResults(id).Result;
            Assert.AreEqual(400, status.StatusCode);
        }
    }
}
