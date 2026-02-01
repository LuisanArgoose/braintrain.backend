using Asp.Versioning;
using Braintrain.Backend.Training.Words.V1.Interfaces;
using Braintrain.Backend.Training.Words.V1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Braintrain.Backend.Training.Words.V1.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/training/words")]
    [Produces("application/json")]

    public class WordsController : Controller
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
        private readonly IWordsTrainingService _wordsTrainingService;
        public WordsController(IWordsTrainingService wordsTrainingService)
        {
            _wordsTrainingService = wordsTrainingService;
        }
        
        [HttpGet("GetWords")]

        public async Task<IActionResult> GetWordsAsync(int wordsCount, int colorsCount, long? userId)
        {
            if (colorsCount < 1 || colorsCount > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(colorsCount));
            }
            if (wordsCount < 1 || wordsCount > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(wordsCount));
            }
            var trainingWords = await _wordsTrainingService.GetWordsAsync(wordsCount, colorsCount, userId);

            return Json(trainingWords, _options);
        }

        [HttpPost("ImportWords")]
        public async Task<IActionResult> ImportWordsAsync([FromBody] List<string> words)
        {

            var addedWordsCount = await _wordsTrainingService.ImportWordsAsync(words);

            return Json(addedWordsCount, _options);
        }
    }
}
