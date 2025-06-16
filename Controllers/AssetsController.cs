using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaptopService.Models;

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

        // POST: api/assets
        [HttpPost]
        public async Task<IActionResult> AddAsset([FromBody] Asset asset)
        {
            asset.CreatedAt = DateTime.UtcNow;
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            return Ok(asset);
        }

        // GET: api/assets/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetAssetSummary()
        {
            var summary = await _context.Assets
                .GroupBy(a => a.Type)
                .Select(g => new { type = g.Key, count = g.Count() })
                .ToListAsync();
            return Ok(summary);
        }

        // GET: api/assets?type=Laptop
        [HttpGet]
        public async Task<IActionResult> GetAssets([FromQuery] string? type)
        {
            var query = _context.Assets.AsQueryable();
            if (!string.IsNullOrEmpty(type))
                query = query.Where(a => a.Type == type);
            var assets = await query.ToListAsync();
            return Ok(assets);
        }

        // DELETE: api/assets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/assets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] Asset updated)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            asset.Type = updated.Type;
            asset.Data = updated.Data;
            await _context.SaveChangesAsync();
            return Ok(asset);
        }

        // GET: api/assets/export?type=Laptop
        [HttpGet("export")]
        public async Task<IActionResult> ExportAssets([FromQuery] string? type)
        {
            var query = _context.Assets.AsQueryable();
            if (!string.IsNullOrEmpty(type))
                query = query.Where(a => a.Type == type);
            var assets = await query.ToListAsync();

            // Convert to CSV (simple implementation)
            var csv = "Id,Type,Data,CreatedAt\n" +
                string.Join("\n", assets.Select(a =>
                    $"{a.Id},{a.Type},\"{a.Data.Replace("\"", "\"\"")}\",{a.CreatedAt:O}"));

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "assets.csv");
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetTypes() =>
    Ok(await _context.AssetTypes.Select(t => t.Name).ToListAsync());

        // POST: api/assets/types
        [HttpPost("types")]
        public async Task<IActionResult> AddType([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest();
            if (await _context.AssetTypes.AnyAsync(t => t.Name == name)) return Conflict();
            _context.AssetTypes.Add(new AssetType { Name = name });
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
