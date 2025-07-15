using LaptopService.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LaptopWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BugController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BugController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("GetAllBugDataRaw")]
        public async Task<IActionResult> GetAllBugDataRaw(int? bguid = null, string? btversion = null)
        {
            var bugTasks = new List<Dictionary<string, object>>();
            var bugSummary = new List<Dictionary<string, object>>();
            var timeTracking = new List<Dictionary<string, object>>();

            using var conn = GetConnection();
            await conn.OpenAsync();

            // 1. GetBugTaskDetails
            try
            {
                using (var cmd = new SqlCommand("GetBugTaskDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@bguid", (object?)bguid ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@btversion", (object?)btversion ?? DBNull.Value);

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        bugTasks.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            // 2. GetBugTimeSummary
            try
            {
                using (var cmd = new SqlCommand("GetBugTimeSummary", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@bguid", (object?)bguid ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@btversion", (object?)btversion ?? DBNull.Value);

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        bugSummary.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            // 3. GetTimeTrackingByBugID
            try
            {
                using (var cmd = new SqlCommand("GetTimeTrackingByBugID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@bguid", (object?)bguid ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@btversion", (object?)btversion ?? DBNull.Value);

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        timeTracking.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            try
            {

                return Ok(new
                {
                    BugTaskDetails = bugTasks,
                    BugTimeSummary = bugSummary,
                    TimeTracking = timeTracking
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }


    }
}
