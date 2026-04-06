using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppListController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public AppListController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AppLists
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ GET BY ID
        [HttpGet("{appId}")]
        public async Task<IActionResult> Get(string appId)
        {
            var data = await _context.AppLists.FindAsync(appId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(AppListDto dto)
        {
            var entity = new AppList
            {
                AppId = dto.AppId,
                Name = dto.Name,
                AppType = dto.AppType,
                Title = dto.Title,
                ModifiedBy = dto.ModifiedBy,
                TimeStamp = DateTime.UtcNow,
                Rowversion = dto.Rowversion,
                OrgSecFl = dto.OrgSecFl
            };

            _context.AppLists.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ UPDATE
        [HttpPut("{appId}")]
        public async Task<IActionResult> Update(string appId, AppListDto dto)
        {
            var entity = await _context.AppLists.FindAsync(appId);

            if (entity == null)
                return NotFound();

            entity.Name = dto.Name;
            entity.AppType = dto.AppType;
            entity.Title = dto.Title;
            entity.ModifiedBy = dto.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;
            entity.Rowversion = dto.Rowversion;
            entity.OrgSecFl = dto.OrgSecFl;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{appId}")]
        public async Task<IActionResult> Delete(string appId)
        {
            var entity = await _context.AppLists.FindAsync(appId);

            if (entity == null)
                return NotFound();

            _context.AppLists.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // 🚀 BULK UPSERT (PostgreSQL optimized)
        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsert(List<AppListDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No data provided");

            // Convert to DataTable-like structure using raw SQL
            var sql = @"
            INSERT INTO app_list (
                app_id, name, app_type, title, modified_by, time_stamp, rowversion, org_sec_fl
            )
            SELECT 
                x.app_id, x.name, x.app_type, x.title, x.modified_by, NOW(), x.rowversion, x.org_sec_fl
            FROM jsonb_to_recordset(@data) AS x(
                app_id VARCHAR(30),
                name VARCHAR(60),
                app_type VARCHAR(10),
                title VARCHAR(60),
                modified_by VARCHAR(20),
                rowversion NUMERIC(10,0),
                org_sec_fl VARCHAR(1)
            )
            ON CONFLICT (app_id)
            DO UPDATE SET
                name = EXCLUDED.name,
                app_type = EXCLUDED.app_type,
                title = EXCLUDED.title,
                modified_by = EXCLUDED.modified_by,
                time_stamp = NOW(),
                rowversion = EXCLUDED.rowversion,
                org_sec_fl = EXCLUDED.org_sec_fl;
        ";

            var jsonData = System.Text.Json.JsonSerializer.Serialize(dtos);

            await _context.Database.ExecuteSqlRawAsync(
                sql,
                new Npgsql.NpgsqlParameter("@data", jsonData)
            );

            return Ok("Bulk upsert completed");
        }
    }
}
