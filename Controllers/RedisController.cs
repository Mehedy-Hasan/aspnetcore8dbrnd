using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;
using TestProj.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private readonly IDatabase _redisDb;

        public RedisController(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        // POST: api/Redis
        [HttpPost]
        public async Task<ActionResult<DBEntity>> PostRedisEntitiy([FromBody] DBEntity dBEntity)
        {
            dBEntity.Id = Guid.NewGuid(); // Ensure unique IDs

            HashSet<string> keys = new HashSet<string>();
            HashSet<string> values = new HashSet<string>();
            var hashEntry =  new HashEntry[]
            {
                new HashEntry($"{dBEntity.Id}", dBEntity.Name)
            };

            await _redisDb.HashSetAsync($"entity", hashEntry);

            return CreatedAtAction("GetDBEntity", new { id = dBEntity.Id }, dBEntity);
        }

        // POST: api/Redis/100
        [HttpPost("{count}")]
        public async Task<ActionResult<DBEntity>> PostRedisBulkInsert(int count)
        {
            List<Task> redisTasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                var _temp = new DBEntity
                {
                    Id = Guid.NewGuid(),
                    Name = $"Name_{i}_{Guid.NewGuid()}"
                };
                
                // Cache entity using both ID and Name
                string jsonEntity = JsonSerializer.Serialize(_temp);
                redisTasks.Add(_redisDb.StringSetAsync($"entity:{_temp.Id}", jsonEntity, TimeSpan.FromMinutes(500)));
                redisTasks.Add(_redisDb.StringSetAsync($"entity:name:{_temp.Name}", jsonEntity, TimeSpan.FromMinutes(500)));
            }

            await Task.WhenAll(redisTasks);

            return Ok(new { Message = "Batch insert successful"});
        }

        // GET: api/Redis/count
        [HttpGet("count")]
        public async Task<IActionResult> GetRedisEntityCount()
        {
            var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "entity:*").Count();

            return Ok(new { RedisCount = keys });
        }
    }
}
