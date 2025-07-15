using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaptopService.Models;
using System.Text;
using LaptopService.Dtos;

namespace LaptopWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssetsController(AppDbContext context)
        {
            _context = context;
        }



        [HttpPost]
        public async Task<IActionResult> AddAsset([FromBody] Asset asset)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(asset.AssetId))
                    return BadRequest("AssetId is required.");


                if (_context.Assets.Any(a => a.AssetId == asset.AssetId))
                    return Conflict("AssetId must be unique.");

                asset.CreatedAt = DateTime.UtcNow;
                _context.Assets.Add(asset);
                await _context.SaveChangesAsync();
                return Ok(asset);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        
        [HttpGet("summary")]
        public async Task<IActionResult> GetAssetSummary()
        {
            try
            {

                var summary = await _context.Assets
                    .GroupBy(a => a.Type)
                    .Select(g => new { type = g.Key, count = g.Count() })
                    .ToListAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAssets([FromQuery] string? type, [FromQuery] string? employeeId)
        {
            try
            {

                var query = _context.Assets.AsQueryable();
                if (!string.IsNullOrEmpty(type))
                    query = query.Where(a => a.Type == type);
                if (!string.IsNullOrEmpty(employeeId))
                    query = query.Where(a => a.EmployeeId == employeeId);
                var assets = await query.ToListAsync();
                return Ok(assets);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    

     
        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            try
            {
                var asset = await _context.Assets.FindAsync(id);
                if (asset == null) return NotFound();
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

       
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] Asset updated)
        {
            try
            {

                var asset = await _context.Assets.FindAsync(id);
                if (asset == null) return NotFound();

                if (string.IsNullOrWhiteSpace(updated.AssetId))
                    return BadRequest("AssetId is required.");

                if (_context.Assets.Any(a => a.AssetId == updated.AssetId && a.Id != id))
                    return Conflict("AssetId must be unique.");

                asset.Type = updated.Type;
                asset.Data = updated.Data;
                asset.EmployeeId = updated.EmployeeId;
                asset.AssetId = updated.AssetId;

                await _context.SaveChangesAsync();
                return Ok(asset);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
       
        [HttpGet("export")]
        public async Task<IActionResult> ExportAssets([FromQuery] string? type)
        {
            try
            {
                var query = _context.Assets.AsQueryable();
                if (!string.IsNullOrEmpty(type))
                    query = query.Where(a => a.Type == type);
                var assets = await query.ToListAsync();


                var csv = "Id,Type,Data,CreatedAt\n" +
                    string.Join("\n", assets.Select(a =>
                        $"{a.Id},{a.Type},\"{a.Data.Replace("\"", "\"\"")}\",{a.CreatedAt:O}"));

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "assets.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetTypes() =>
    Ok(await _context.AssetTypes.Select(t => t.Name).ToListAsync());

        [HttpPost("types")]
        public async Task<IActionResult> AddType([FromBody] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) return BadRequest();
                if (await _context.AssetTypes.AnyAsync(t => t.Name == name)) return Conflict();
                _context.AssetTypes.Add(new AssetType { Name = name });
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsset(int id)
        {
            try
            {

                var asset = await _context.Assets.FindAsync(id);
                if (asset == null) return NotFound();
                return Ok(asset);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        [HttpGet("export-all")]
        public async Task<IActionResult> ExportAllAssets()
        {
            try
            {
                var assets = await _context.Assets
                       .OrderBy(a => a.Type)
                       .ToListAsync();

                var grouped = assets.GroupBy(a => a.Type);
                var csv = new StringBuilder();

                foreach (var group in grouped)
                {

                    var assetDicts = group.Select(a => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(a.Data)).ToList();
                    var allKeys = assetDicts.SelectMany(d => d.Keys).Distinct().ToList();


                    csv.AppendLine($"Type: {group.Key}");
                    csv.AppendLine(string.Join(",", allKeys));


                    foreach (var dict in assetDicts)
                    {
                        csv.AppendLine(string.Join(",", allKeys.Select(k => dict.ContainsKey(k) ? dict[k] : "")));
                    }

                    csv.AppendLine();
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", "all-assets.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
     
        [HttpGet("vendors")]
        public async Task<IActionResult> GetVendors()
        {
            try
            {

                var vendors = await _context.Vendors.OrderByDescending(v => v.CreatedAt).ToListAsync();
                return Ok(vendors);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

       
        [HttpPost("vendors")]
        public async Task<IActionResult> AddVendor([FromBody] Vendor vendor)
        {
            try
            {
                vendor.CreatedAt = DateTime.UtcNow;
                _context.Vendors.Add(vendor);
                await _context.SaveChangesAsync();
                return Ok(vendor);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
   
        [HttpPost("vendors/{id}")]
        public async Task<IActionResult> EditVendor(int id, [FromBody] Vendor vendor)
        {
            try
            {
                var existing = await _context.Vendors.FindAsync(id);
                if (existing == null) return NotFound();
                existing.Data = vendor.Data;
                await _context.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
     
        [HttpPost("vendors/delete/{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            try
            {
                var vendor = await _context.Vendors.FindAsync(id);
                if (vendor == null) return NotFound();
                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
      
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _context.Employees.OrderByDescending(e => e.CreatedAt).ToListAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

    
        [HttpPost("employees")]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            try
            {
                employee.CreatedAt = DateTime.UtcNow;
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return Ok(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

 
        [HttpPost("employees/{id}")]
        public async Task<IActionResult> EditEmployee(string id, [FromBody] Employee employee)
        {
            try
            {
                var existing = await _context.Employees.FindAsync(id);
                if (existing == null) return NotFound();
                existing.Data = employee.Data;
                await _context.SaveChangesAsync();
                return Ok(existing);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

      
        [HttpPost("employees/delete/{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null) return NotFound();
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

   
        [HttpGet("by-employee/{employeeId}")]
        public IActionResult GetAssetsByEmployee(string employeeId)
        {
            try
            {
                var assets = _context.Assets
                       .Where(a => a.Data.Contains($"\"EmployeeId\":\"{employeeId}\""))
                       .ToList();
                return Ok(assets);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }




    }
}
