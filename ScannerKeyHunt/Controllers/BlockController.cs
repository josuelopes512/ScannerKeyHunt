using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Domain.Services;
using ScannerKeyHunt.Repository.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ScannerKeyHunt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BlockController> _logger;
        private readonly PartitionCalculator _calculator;
        private readonly IUnitOfWork _unitOfWork;

        public BlockController(IServiceProvider serviceProvider, ILogger<BlockController> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _calculator = new PartitionCalculator(serviceProvider, 67);
            _unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        }

        // GET: api/<BlockController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new List<string>();
        }

        // GET api/<BlockController>/5
        [HttpGet("{id}")]
        public Block Get(long id)
        {
            Block block = _calculator.GetOrCreateBlock();
            block.Area = null;
            return block;
        }

        // POST api/<BlockController>
        [HttpPost]
        public void Post()
        {
        }

        // PUT api/<BlockController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<BlockController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
