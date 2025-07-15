
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using System.Data;
//using LaptopService.Dtos;
//using Microsoft.EntityFrameworkCore;

//namespace LaptopWebApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class GanttChartController : ControllerBase
//    {
//        private readonly IConfiguration _config;

//        public GanttChartController(IConfiguration config)
//        {
//            _config = config;
//        }

//        private SqlConnection GetConnection()
//        {
//            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetGanttTasks(
//            string? projectName = null,
//            DateTime? startDate = null,
//            DateTime? endDate = null)
//        {
//            try
//            {
//                using var conn = GetConnection();

//                try
//                {
//                    await conn.OpenAsync();
//                }
//                catch (Exception dbEx)
//                {
//                    return StatusCode(500, $"❌ Failed to connect to database: {dbEx.Message}");
//                }

//                using var cmd = conn.CreateCommand();
//                cmd.CommandText = "Ganttchart";
//                cmd.CommandType = CommandType.StoredProcedure;

//                cmd.Parameters.Add(new SqlParameter("@projectName", projectName ?? (object)DBNull.Value));
//                cmd.Parameters.Add(new SqlParameter("@startDate", startDate ?? (object)DBNull.Value));
//                cmd.Parameters.Add(new SqlParameter("@endDate", endDate ?? (object)DBNull.Value));

//                var results = new List<GanttTaskDto>();
//                using var reader = await cmd.ExecuteReaderAsync();

//                while (await reader.ReadAsync())
//                {
//                    results.Add(new GanttTaskDto
//                    {
//                        TaskId = reader.GetDecimal(0),
//                        DependentId = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
//                        TaskTitle = reader.GetString(2),
//                        StartDate = reader.GetString(3),
//                        EndDate = reader.GetString(4),
//                        ProjectName = reader.GetString(5),
//                        RelationshipType = reader.IsDBNull(6) ? null : reader.GetInt16(6),
//                        ResourceName = reader.IsDBNull(7) ? null : reader.GetString(7),
//                        TaskResourceName = reader.IsDBNull(8) ? null : reader.GetString(8),
//                        Progress = reader.GetDecimal(9),
//                    });
//                }

//                // Step 1: Tasks
//                var tasks = results.Select(r => new TaskDto
//                {
//                    Id = r.TaskId,
//                    ParentId = null,
//                    Title = r.TaskTitle,
//                    Start = r.StartDate,
//                    End = r.EndDate,
//                    Progress = r.Progress
//                }).ToList();

//                // Step 2: Dependencies
//                var dependencies = results
//                    .Where(r => r.DependentId.HasValue)
//                    .Select((r, index) => new DependencyDto
//                    {
//                        Id = index + 1,
//                        PredecessorId = r.DependentId.Value,
//                        SuccessorId = r.TaskId,
//                        Type = r.RelationshipType ?? 0
//                    }).ToList();

//                // Step 3: Resources
//                var resourceMap = results
//                    .Where(r => !string.IsNullOrEmpty(r.ResourceName))
//                    .Select(r => r.ResourceName)
//                    .Distinct()
//                    .Select((name, index) => new ResourceDto
//                    {
//                        Id = index + 1,
//                        Text = name
//                    }).ToList();

//                // Step 4: Assignments
//                var assignments = new List<AssignmentDto>();
//                int assignId = 1;

//                foreach (var r in results)
//                {
//                    var resource = resourceMap.FirstOrDefault(res => res.Text == r.ResourceName);
//                    if (resource != null)
//                    {
//                        assignments.Add(new AssignmentDto
//                        {
//                            Id = assignId++,
//                            TaskId = r.TaskId,
//                            ResourceId = resource.Id
//                        });
//                    }
//                }

//                var structured = new GanttStructuredResponse
//                {
//                    Tasks = tasks,
//                    Dependencies = dependencies,
//                    Resources = resourceMap,
//                    Assignments = assignments
//                };

//                return Ok(structured);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"❌ General error: {ex.Message}");
//            }
//        }

//        // Raw result from stored procedure
//        public class GanttTaskDto
//        {
//            public decimal TaskId { get; set; }
//            public decimal? DependentId { get; set; }
//            public string TaskTitle { get; set; }
//            public string StartDate { get; set; }
//            public string EndDate { get; set; }
//            public string ProjectName { get; set; }
//            public int? RelationshipType { get; set; }
//            public string ResourceName { get; set; }
//            public string TaskResourceName { get; set; }
//            public decimal Progress { get; set; }
//        }

//        // Final structured response
//        public class GanttStructuredResponse
//        {
//            public List<TaskDto> Tasks { get; set; }
//            public List<DependencyDto> Dependencies { get; set; }
//            public List<ResourceDto> Resources { get; set; }
//            public List<AssignmentDto> Assignments { get; set; }
//        }

//        public class TaskDto
//        {
//            public decimal Id { get; set; }
//            public decimal? ParentId { get; set; }
//            public string Title { get; set; }
//            public string Start { get; set; }
//            public string End { get; set; }
//            public decimal Progress { get; set; }
//        }

//        public class DependencyDto
//        {
//            public int Id { get; set; }
//            public decimal PredecessorId { get; set; }
//            public decimal SuccessorId { get; set; }
//            public int Type { get; set; }
//        }

//        public class ResourceDto
//        {
//            public int Id { get; set; }
//            public string Text { get; set; }
//        }

//        public class AssignmentDto
//        {
//            public int Id { get; set; }
//            public decimal TaskId { get; set; }
//            public int ResourceId { get; set; }
//        }
//    }
////}
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using System.Data;
//using LaptopService.Dtos;

//namespace LaptopWebApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class GanttChartController : ControllerBase
//    {
//        private readonly IConfiguration _config;

//        public GanttChartController(IConfiguration config)
//        {
//            _config = config;
//        }

//        private SqlConnection GetConnection()
//        {
//            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetGanttTasks(
//            string? projectName = null,
//            DateTime? startDate = null,
//            DateTime? endDate = null)
//        {
//            try
//            {
//                using var conn = GetConnection();

//                try
//                {
//                    await conn.OpenAsync();
//                }
//                catch (Exception dbEx)
//                {
//                    return StatusCode(500, $"❌ Failed to connect to database: {dbEx.Message}");
//                }

//                using var cmd = conn.CreateCommand();
//                cmd.CommandText = "Ganttchart";
//                cmd.CommandType = CommandType.StoredProcedure;

//                cmd.Parameters.Add(new SqlParameter("@projectName", projectName ?? (object)DBNull.Value));
//                cmd.Parameters.Add(new SqlParameter("@startDate", startDate ?? (object)DBNull.Value));
//                cmd.Parameters.Add(new SqlParameter("@endDate", endDate ?? (object)DBNull.Value));

//                var rawResults = new List<GanttTaskDto>();
//                using var reader = await cmd.ExecuteReaderAsync();

//                while (await reader.ReadAsync())
//                {
//                    rawResults.Add(new GanttTaskDto
//                    {
//                        TaskId = reader.GetDecimal(0),
//                        DependentId = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
//                        TaskTitle = reader.GetString(2),
//                        StartDate = reader.GetString(3),
//                        EndDate = reader.GetString(4),
//                        ProjectName = reader.GetString(5),
//                        RelationshipType = reader.IsDBNull(6) ? null : reader.GetInt16(6),
//                        ResourceName = reader.IsDBNull(7) ? null : reader.GetString(7),
//                        TaskResourceName = reader.IsDBNull(8) ? null : reader.GetString(8),
//                        Progress = reader.GetDecimal(9),
//                    });
//                }

//                // Filter: Keep only the first entry per TaskId
//                var results = rawResults
//                    .GroupBy(r => r.TaskId)
//                    .Select(g => g.First())
//                    .ToList();

//                // Optional: Log duplicate TaskIds
//                var duplicateIds = rawResults
//                    .GroupBy(r => r.TaskId)
//                    .Where(g => g.Count() > 1)
//                    .Select(g => g.Key)
//                    .ToList();

//                if (duplicateIds.Any())
//                {
//                    Console.WriteLine("⚠️ Duplicate TaskIds found: " + string.Join(", ", duplicateIds));
//                }

//                // Step 1: Tasks
//                var tasks = results.Select(r => new TaskDto
//                {
//                    Id = r.TaskId,
//                    ParentId = null,
//                    Title = r.TaskTitle,
//                    Start = r.StartDate,
//                    End = r.EndDate,
//                    Progress = r.Progress
//                }).ToList();

//                // Step 2: Dependencies
//                var dependencies = results
//                    .Where(r => r.DependentId.HasValue)
//                    .Select((r, index) => new DependencyDto
//                    {
//                        Id = index + 1,
//                        PredecessorId = r.DependentId.Value,
//                        SuccessorId = r.TaskId,
//                        Type = r.RelationshipType ?? 0
//                    }).ToList();

//                // Step 3: Resources
//                var resourceMap = results
//                    .Where(r => !string.IsNullOrEmpty(r.ResourceName))
//                    .Select(r => r.ResourceName)
//                    .Distinct()
//                    .Select((name, index) => new ResourceDto
//                    {
//                        Id = index + 1,
//                        Text = name
//                    }).ToList();

//                // Step 4: Assignments
//                var assignments = new List<AssignmentDto>();
//                int assignId = 1;

//                foreach (var r in results)
//                {
//                    var resource = resourceMap.FirstOrDefault(res => res.Text == r.ResourceName);
//                    if (resource != null)
//                    {
//                        assignments.Add(new AssignmentDto
//                        {
//                            Id = assignId++,
//                            TaskId = r.TaskId,
//                            ResourceId = resource.Id
//                        });
//                    }
//                }

//                var structured = new GanttStructuredResponse
//                {
//                    Tasks = tasks,
//                    Dependencies = dependencies,
//                    Resources = resourceMap,
//                    Assignments = assignments
//                };

//                return Ok(structured);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"❌ General error: {ex.Message}");
//            }
//        }

//        // Raw result from stored procedure
//        public class GanttTaskDto
//        {
//            public decimal TaskId { get; set; }
//            public decimal? DependentId { get; set; }
//            public string TaskTitle { get; set; }
//            public string StartDate { get; set; }
//            public string EndDate { get; set; }
//            public string ProjectName { get; set; }
//            public int? RelationshipType { get; set; }
//            public string ResourceName { get; set; }
//            public string TaskResourceName { get; set; }
//            public decimal Progress { get; set; }
//        }

//        // Final structured response
//        public class GanttStructuredResponse
//        {
//            public List<TaskDto> Tasks { get; set; }
//            public List<DependencyDto> Dependencies { get; set; }
//            public List<ResourceDto> Resources { get; set; }
//            public List<AssignmentDto> Assignments { get; set; }
//        }

//        public class TaskDto
//        {
//            public decimal Id { get; set; }
//            public decimal? ParentId { get; set; }
//            public string Title { get; set; }
//            public string Start { get; set; }
//            public string End { get; set; }
//            public decimal Progress { get; set; }
//        }

//        public class DependencyDto
//        {
//            public int Id { get; set; }
//            public decimal PredecessorId { get; set; }
//            public decimal SuccessorId { get; set; }
//            public int Type { get; set; }
//        }

//        public class ResourceDto
//        {
//            public int Id { get; set; }
//            public string Text { get; set; }
//        }

//        public class AssignmentDto
//        {
//            public int Id { get; set; }
//            public decimal TaskId { get; set; }
//            public int ResourceId { get; set; }
//        }
//    }
//}
//working above

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using LaptopService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LaptopWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GanttChartController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GanttChartController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
        public async Task<IActionResult> GetGanttTasks(
            string? projectName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            
            string? resourceName = null,
            string? targetVersion = null)
        {
            try
            {
                using var conn = GetConnection();

                try
                {
                    await conn.OpenAsync();
                }
                catch (Exception dbEx)
                {
                    return StatusCode(500, $"❌ Failed to connect to database: {dbEx.Message}");
                }

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "Ganttcharts";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@projectName", projectName ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@startDate", startDate ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@endDate", endDate ?? (object)DBNull.Value));
               
                cmd.Parameters.Add(new SqlParameter("@resourceName", resourceName ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@targetVersion", targetVersion ?? (object)DBNull.Value));

                var results = new List<GanttTaskDto>();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new GanttTaskDto
                    {
                        TaskId = reader.GetDecimal(0),
                        DependentId = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                        TaskTitle = reader.GetString(2),
                        StartDate = reader.GetString(3),
                        EndDate = reader.GetString(4),
                        ProjectName = reader.GetString(5),
                        RelationshipType = reader.IsDBNull(6) ? null : reader.GetInt16(6),
                        ResourceName = reader.IsDBNull(7) ? null : reader.GetString(7),
                        TaskResourceName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        Progress = reader.GetDecimal(9),
                    });
                }

                // Remove duplicate tasks (based on TaskId)
                var uniqueTaskIds = new HashSet<decimal>();
                var uniqueResults = results
                    .Where(r => uniqueTaskIds.Add(r.TaskId))
                    .ToList();

                // Step 1: Tasks
                var tasks = uniqueResults.Select(r => new TaskDto
                {
                    Id = r.TaskId,
                    ParentId = null,
                    Title = r.TaskTitle,
                    Start = r.StartDate,
                    End = r.EndDate,
                    Progress = r.Progress
                }).ToList();

                // Step 2: Dependencies
                var dependencies = uniqueResults
                    .Where(r => r.DependentId.HasValue)
                    .Select((r, index) => new DependencyDto
                    {
                        Id = index + 1,
                        PredecessorId = r.DependentId.Value,
                        SuccessorId = r.TaskId,
                        Type = r.RelationshipType ?? 0
                    }).ToList();

                // Step 3: Resources
                var resourceMap = uniqueResults
                    .Where(r => !string.IsNullOrEmpty(r.ResourceName))
                    .Select(r => r.ResourceName)
                    .Distinct()
                    .Select((name, index) => new ResourceDto
                    {
                        Id = index + 1,
                        Text = name
                    }).ToList();

                // Step 4: Assignments
                var assignments = new List<AssignmentDto>();
                int assignId = 1;

                foreach (var r in uniqueResults)
                {
                    var resource = resourceMap.FirstOrDefault(res => res.Text == r.ResourceName);
                    if (resource != null)
                    {
                        assignments.Add(new AssignmentDto
                        {
                            Id = assignId++,
                            TaskId = r.TaskId,
                            ResourceId = resource.Id
                        });
                    }
                }

                var structured = new GanttStructuredResponse
                {
                    Tasks = tasks,
                    Dependencies = dependencies,
                    Resources = resourceMap,
                    Assignments = assignments
                };

                return Ok(structured);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ General error: {ex.Message}");
            }
        }

        // DTO classes
        public class GanttTaskDto
        {
            public decimal TaskId { get; set; }
            public decimal? DependentId { get; set; }
            public string TaskTitle { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string ProjectName { get; set; }
            public int? RelationshipType { get; set; }
            public string ResourceName { get; set; }
            public string TaskResourceName { get; set; }
            public decimal Progress { get; set; }
        }

        public class GanttStructuredResponse
        {
            public List<TaskDto> Tasks { get; set; }
            public List<DependencyDto> Dependencies { get; set; }
            public List<ResourceDto> Resources { get; set; }
            public List<AssignmentDto> Assignments { get; set; }
        }

        public class TaskDto
        {
            public decimal Id { get; set; }
            public decimal? ParentId { get; set; }
            public string Title { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal Progress { get; set; }
        }

        public class DependencyDto
        {
            public int Id { get; set; }
            public decimal PredecessorId { get; set; }
            public decimal SuccessorId { get; set; }
            public int Type { get; set; }
        }

        public class ResourceDto
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        public class AssignmentDto
        {
            public int Id { get; set; }
            public decimal TaskId { get; set; }
            public int ResourceId { get; set; }
        }
    }
}
