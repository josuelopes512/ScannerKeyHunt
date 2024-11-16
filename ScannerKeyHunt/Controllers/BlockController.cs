using Microsoft.AspNetCore.Mvc;
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
        private PartitionCalculator _calculator;
        private readonly IUnitOfWork _unitOfWork;

        public BlockController(IServiceProvider serviceProvider, ILogger<BlockController> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _unitOfWork = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        }

        // GET api/<BlockController>/5
        [HttpGet]
        public IActionResult Get(int puzzleId)
        {
            try
            {
                Block block = new PartitionCalculator(_serviceProvider, puzzleId).GetOrCreateBlock();

                return Ok(new { Id = block.Id, StartKey = block.StartKey, EndKey = block.EndKey });
            }
            catch (Exception ex)
            {
                return BadRequest(new {Message = ex.Message});
            }
        }

        // POST api/<BlockController>
        [HttpPost]
        public IActionResult Post(BlockRequest blockRequest)
        {
            try
            {
                _calculator = new PartitionCalculator(_serviceProvider, blockRequest.PuzzleId);

                Block block = _unitOfWork.BlockRepository.GetById(blockRequest.Id);

                if (block.Seed == blockRequest.Seed)
                {
                    if (!string.IsNullOrEmpty(blockRequest.PrivateKey))
                    {
                        Area area = _unitOfWork.AreaRepository.GetAll(x => x.Id == block.AreaId).FirstOrDefault();
                        Section section = _unitOfWork.SectionRepository.GetById(area.SectionId);
                        PuzzleWallet puzzleWallet = _unitOfWork.PuzzleWalletCache.GetById(section.PuzzleWalletId);

                        List<Section> sections = _unitOfWork.SectionRepository.GetAll(x => x.PuzzleWalletId == puzzleWallet.Id);
                        sections.ForEach(section =>
                        {
                            section.IsCompleted = true;
                            section.Disabled = true;
                        });
                        _unitOfWork.SectionRepository.BulkUpdate(sections);

                        List<Area> areas = _unitOfWork.AreaRepository.GetAll(x => sections.Select(x => x.Id).Contains(x.SectionId));
                        areas.ForEach(area =>
                        {
                            area.IsCompleted = true;
                            area.Disabled = true;
                        });
                        _unitOfWork.AreaRepository.BulkUpdate(areas);

                        List<Block> allBlocks = new List<Block>();

                        areas.ForEach(area =>
                        {
                            List<Block> blocks = _unitOfWork.BlockRepository.GetAll(x => x.AreaId == area.Id);

                            blocks.ForEach(block =>
                            {
                                block.IsCompleted = true;
                                block.Disabled = true;
                            });
                            allBlocks.AddRange(blocks);
                        });

                        _unitOfWork.BlockRepository.BulkUpdate(allBlocks);

                        puzzleWallet.IsCompleted = true;
                        puzzleWallet.PrivateKey = blockRequest.PrivateKey;
                        _unitOfWork.PuzzleWalletCache.Update(puzzleWallet);

                        section.IsCompleted = true;
                        section.Disabled = false;
                        _unitOfWork.SectionRepository.Update(section);

                        area.IsCompleted = true;
                        area.Disabled = false;
                        _unitOfWork.AreaRepository.Update(area);
                    }

                    block.IsCompleted = true;
                    block.Disabled = false;
                    _unitOfWork.BlockRepository.Update(block);

                    _unitOfWork.Save();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
