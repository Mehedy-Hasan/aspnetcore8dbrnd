using Elastic.Apm;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;
using TestProj.Data;
using TestProj.Models;

namespace TestProj.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;
        private readonly IDatabase _redisDb;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _logger = logger;
            _redisDb = redis.GetDatabase();
        }

        private void InsertEntitiesIntoDatabase()
        {
            List<DBEntity> entities = new List<DBEntity>();
            for (int i = 0; i < 100; i++)
            {
                var _temp = new DBEntity
                {
                    Id = Guid.NewGuid(),
                    Name = $"Name_{i}_{Guid.NewGuid()}"
                };
                entities.Add(_temp);
            }

            _context.Entities.AddRange(entities);
            _context.SaveChanges();
            var count = _context.Entities.Count();
        }

        private async Task InsertEntitiesIntoRedis()
        {
            List<Task> redisTasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var _temp = new DBEntity
                {
                    Id = Guid.NewGuid(),
                    Name = $"Name_{i}_{Guid.NewGuid()}"
                };

                string jsonEntity = JsonSerializer.Serialize(_temp);
                redisTasks.Add(_redisDb.StringSetAsync($"entity:{_temp.Id}", jsonEntity, TimeSpan.FromMinutes(500)));
                redisTasks.Add(_redisDb.StringSetAsync($"entity:name:{_temp.Name}", jsonEntity, TimeSpan.FromMinutes(500)));
            }

            await Task.WhenAll(redisTasks);
            var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "entity:*").Count();
        }

        public IActionResult Index()
        {

            // Insert entities into the database
            InsertEntitiesIntoDatabase();

            // Insert entities into Redis cache
            InsertEntitiesIntoRedis();

            return View();
        }

        public IActionResult Privacy()
        {
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
