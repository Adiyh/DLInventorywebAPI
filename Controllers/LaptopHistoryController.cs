//using LaptopService.Infrastructure.Repositories.Interface;
//using LaptopService.Models;
//using Microsoft.AspNetCore.Mvc;

//namespace LaptopWebApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class LaptopHistoryController : ControllerBase
//    {
//        private readonly ILaptopHistoryRepository _historyRepository;

//        public LaptopHistoryController(ILaptopHistoryRepository historyRepository)
//        {
//            _historyRepository = historyRepository;
//        }

//        [HttpGet("GetByLaptopId/{laptopId}")]
//        public ActionResult<IEnumerable<LaptopHistory>> GetByLaptopId(int laptopId)
//        {
//            var history = _historyRepository.GetHistoryByLaptopId(laptopId);
//            return Ok(history.Select(h => new
//            {
//                date = h.Date.ToString("yyyy-MM-dd"),
//                action = h.Action
//            }));
//        }

//        // Optional: Add POST endpoint to add history
//        //[HttpPost]
//        //public IActionResult AddHistory([FromBody] LaptopHistory history)
//        //{
//        //    _historyRepository.AddHistory(history);
//        //    return Ok();
//        //}
//    }
//}
