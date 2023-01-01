using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Web.Http;

namespace SqlApi
{
    public static class NotesApi
    {
        public const string _FunctionName = "Notes";
        [FunctionName(_FunctionName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestStatus;
            try
            {
                NotesDbController db = new NotesDbController(req, log);

                switch (req.Method.ToUpper())
                {
                    case "GET":
                        requestStatus = db.Get();
                        break;
                    case "POST":
                        requestStatus = db.Post();
                        break;
                    case "PUT":
                        requestStatus = db.Put();
                        break;
                    case "DELETE":
                        requestStatus = db.Delete();
                        break;
                    default:
                        return new BadRequestObjectResult($"Invalid request type {req.Method}");
                }
            }
            catch (MissingFieldException ex)
            {
                log.LogError($"Bad request: {ex.Message}");
                return new BadRequestErrorMessageResult(ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError($"Received an error: {ex.Message}");
                return new InternalServerErrorResult();
            }

            return new OkObjectResult(requestStatus);
        }
    }
}
