using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SqlApi
{
    internal class NotesDbController
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("NotesDb_ConnectionString");

        public HttpRequest Req { get; }
        public ILogger Log { get; }
        public Dictionary<string, object> BodyData { get; }

        public NotesDbController(HttpRequest req, ILogger log)
        {
            Req = req;
            Log = log;
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                BodyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
            }
        }
        public string Get()
        {
            string noteid = GetVariable("noteid", optional: true);

            Log.LogInformation($"Get: {noteid}");

            var sql = "dbo.spNote_Get";
            using (var connection = new SqlConnection(ConnectionString))
            {
                var results = connection.Query(sql, new { noteid }, commandType: CommandType.StoredProcedure);
                return JsonConvert.SerializeObject(results);
            }
        }

        public string Post()
        {

            string title = GetVariable("title");
            string note = GetVariable("note");
            string userId = GetVariable("userId", optional: true);

            Log.LogInformation($"Post: {title} - {userId}");

            var sql = "dbo.spNote_Add";
            using (var connection = new SqlConnection(ConnectionString))
            {
                var results = connection.Query(sql, new { title, note, userId }, commandType: CommandType.StoredProcedure);
                return JsonConvert.SerializeObject(results);
            }
        }

        public string Put()
        {
            string noteid = GetVariable("noteid");
            string title = GetVariable("title");
            string note = GetVariable("note");
            string userId = GetVariable("userId", optional: true);

            Log.LogInformation($"Put: {noteid} - {title} - {userId}");

            var sql = "[dbo].[spNote_Update]";
            using (var connection = new SqlConnection(ConnectionString))
            {
                var results = connection.Query(sql, new { noteid, title, note, userId }, commandType: CommandType.StoredProcedure);
                return JsonConvert.SerializeObject(results);
            }
        }
        public string Delete()
        {
            int.TryParse(GetVariable("NoteId"), out int noteId);

            Log.LogInformation($"Delete: {noteId}");

            var sql = "dbo.spNote_Delete";
            using (var connection = new SqlConnection(ConnectionString))
            {
                var results = connection.Query(sql, new { noteId }, commandType: CommandType.StoredProcedure);
                return JsonConvert.SerializeObject(results);
            }
        }
        protected internal string GetVariable(string variable, bool optional = false)
        {
            string result = Req.Query[variable];
            if (result is null && BodyData is not null)
            {
                var bodyResult = BodyData.Where(x => x.Key.ToLower() == variable.ToLower()).FirstOrDefault();
                if (bodyResult.Value is null)
                {
                    if (optional)
                    {
                        return null;
                    }
                    throw new MissingFieldException($"Failed to find required parameter '{variable}'");
                }
                else
                {
                    result = bodyResult.Value.ToString();
                }
            }
            if (result is null && !optional)
            {
                throw new Exception($"Failed to locate parameter '{variable}'");
            }
            return result;
        }
    }
}
