using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestProj.Data;
using TestProj.Models;

namespace TestProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DBEntitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DBEntitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DBEntities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DBEntity>>> GetEntities()
        {
            return await _context.Entities.ToListAsync();
        }

        // GET: api/DBEntities/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCountEntities()
        {
            return await _context.Entities.CountAsync();
        }

        // GET: api/DBEntities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DBEntity>> GetDBEntity(Guid id)
        {
            var dBEntity = await _context.Entities.FindAsync(id);

            if (dBEntity == null)
            {
                return NotFound();
            }

            return dBEntity;
        }

        // PUT: api/DBEntities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDBEntity(Guid id, DBEntity dBEntity)
        {
            if (id != dBEntity.Id)
            {
                return BadRequest();
            }

            _context.Entry(dBEntity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DBEntityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DBEntities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DBEntity>> PostDBEntity(DBEntity dBEntity)
        {
            dBEntity.Id = Guid.NewGuid(); // Ensure unique IDs

            _context.Entities.Add(dBEntity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDBEntity", new { id = dBEntity.Id }, dBEntity);
        }

        // POST: api/DBEntities/100
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{count}")]
        public async Task<ActionResult<DBEntity>> PostDBEntity(int count)
        {
            List<DBEntity> entities = new List<DBEntity>();
            for (int i = 0; i < count; i++)
            {
                var _temp = new DBEntity();
                _temp.Id = Guid.NewGuid();
                _temp.Name = $"Name_{i}_{_temp.Id}";
                entities.Add(_temp);
            }

            await _context.Entities.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Batch insert successful", Count = entities.Count });
        }

        // DELETE: api/DBEntities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDBEntity(Guid id)
        {
            var dBEntity = await _context.Entities.FindAsync(id);
            if (dBEntity == null)
            {
                return NotFound();
            }

            _context.Entities.Remove(dBEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DBEntityExists(Guid id)
        {
            return _context.Entities.Any(e => e.Id == id);
        }
    }
}
