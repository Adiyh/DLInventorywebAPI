using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace LaptopWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GanttController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GanttController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("chart")]
        public async Task<IActionResult> GetGanttChart(
            [FromQuery] string projectName,
            [FromQuery] string targetVersion)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT * FROM Gantt1
                WHERE Project_Name = @Project_Name
                  AND Target_Version = @Target_Version";
                cmd.Parameters.AddWithValue("@Project_Name", projectName);
                cmd.Parameters.AddWithValue("@Target_Version", targetVersion);

                var tasks = new List<object>();
                var dependencies = new List<object>();
                var resources = new Dictionary<string, int>();
                var assignments = new List<object>();
                var taskIdSet = new HashSet<int>();
                int resourceIdCounter = 1;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int taskId = reader["Task_Id"] is DBNull ? 0 : Convert.ToInt32(reader["Task_Id"]);
                        int? dependentId = reader["Dependent_id"] is DBNull ? null : (int?)reader["Dependent_id"];
                        string? resourceName = reader["Resource_Name"] as string;

                        if (taskId != 0)
                            taskIdSet.Add(taskId);

                        tasks.Add(new
                        {
                            id = taskId,
                            parentId = (int?)null,
                            title = reader["Task_Summary"] as string,
                            start = reader["Actual_Start"] is DBNull ? null : ((DateTime)reader["Actual_Start"]).ToString("yyyy-MM-dd"),
                            end = reader["Current_Merge_Date"] is DBNull ? null : ((DateTime)reader["Current_Merge_Date"]).ToString("yyyy-MM-dd"),
                            progress = reader["Progress"] is DBNull ? 0 : Convert.ToInt32(reader["Progress"])
                        });

                        if (dependentId.HasValue && dependentId.Value != 0)
                        {
                            dependencies.Add(new
                            {
                                id = taskId,
                                predecessorId = dependentId.Value,
                                successorId = taskId,
                                type = 0
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(resourceName))
                        {
                            if (!resources.ContainsKey(resourceName))
                            {
                                resources[resourceName] = resourceIdCounter++;
                            }
                            assignments.Add(new
                            {
                                id = assignments.Count + 1,
                                taskId = taskId,
                                resourceId = resources[resourceName]
                            });
                        }
                    }
                }

                dependencies = dependencies
                    .Where(d =>
                        taskIdSet.Contains((int)d.GetType().GetProperty("predecessorId")!.GetValue(d)!) &&
                        taskIdSet.Contains((int)d.GetType().GetProperty("successorId")!.GetValue(d)!))
                    .ToList();

                var resourcesList = resources.Select(r => new { id = r.Value, text = r.Key }).ToList();

                return Ok(new
                {
                    tasks,
                    dependencies,
                    resources = resourcesList,
                    assignments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Error: {ex.Message}");
            }
        }

        [HttpPost("save-task")]
        public async Task<IActionResult> SaveTask([FromBody] GanttTaskDto task)
        {
            if (task == null || task.id == 0)
                return BadRequest("Invalid task data.");

            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                UPDATE Gantt1
                SET 
                    Current_Merge_Date = @End,
                    Actual_Start = @Start,
                    Progress = @Progress,
                    Resource_Name = @ResourceName
                WHERE Task_Id = @Id";

                cmd.Parameters.AddWithValue("@Id", task.id);
                cmd.Parameters.AddWithValue("@End", (object?)task.end ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Start", (object?)task.start ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Progress", task.progress ?? 0);
                cmd.Parameters.AddWithValue("@ResourceName", (object?)task.resource_Name ?? DBNull.Value);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows > 0)
                    return Ok(new { success = true });
                else
                    return NotFound("Task not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Error: {ex.Message}");
            }
        }

        public class GanttTaskDto
        {
            public int id { get; set; }
            public string? start { get; set; }
            public string? end { get; set; }
            public int? progress { get; set; }
            public string? resource_Name { get; set; }
        }
    }
}
