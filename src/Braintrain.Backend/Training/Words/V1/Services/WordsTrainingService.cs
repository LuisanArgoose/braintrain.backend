using Braintrain.Backend.Training.Words.V1.Entities;
using Braintrain.Backend.Training.Words.V1.Interfaces;
using Braintrain.Backend.Training.Words.V1.Models;
using Microsoft.EntityFrameworkCore;


namespace Braintrain.Backend.Training.Words.V1.Services;

public class WordsTrainingService : IWordsTrainingService
{
    private readonly WordsTrainingDbContext _db;
    public WordsTrainingService(WordsTrainingDbContext db)
    {
        _db = db;
    }
    async Task<List<TrainingWord>> IWordsTrainingService.GetWordsAsync(int wordsCount, int colorsCount, long? userId)
    {
        if (userId == null)
        {
            return await GetWordsAsync(wordsCount, colorsCount);
        }
        else
        {
            return await GetWordsAsync(wordsCount, colorsCount, (long)userId);
        }
    }
    private Task<List<TrainingWord>> GetWordsAsync(int wordsCount, int colorsCount, long userId)
    {
        throw new NotImplementedException();
    }
    private async Task<List<TrainingWord>> GetWordsAsync(int wordsCount, int colorsCount)
    {
        var random = new Random();

        // 1. Получаем диапазоны ID (минимальный и максимальный)
        // Это гораздо быстрее, чем Count(), так как используются индексы
        var wordStats = await _db.Words
            .Select(w => w.Id)
            .GroupBy(x => 1)
            .Select(g => new { Min = g.Min(), Max = g.Max() })
            .FirstOrDefaultAsync();

        var colorStats = await _db.Colors
            .Select(c => c.Id)
            .GroupBy(x => 1)
            .Select(g => new { Min = g.Min(), Max = g.Max() })
            .FirstOrDefaultAsync();

        if (wordStats == null || colorStats == null) return new List<TrainingWord>();

        // 2. Генерируем набор случайных ID
        // Мы берем чуть больше ID (на 20-30%), на случай если в базе есть "дыры" (удаленные ID)
        var targetWordIds = Enumerable.Range(0, (int)(wordsCount * 1.3))
            .Select(_ => (long)random.Next((int)wordStats.Min, (int)wordStats.Max + 1))
            .Distinct()
            .Take(wordsCount)
            .ToList();

        var targetColorIds = Enumerable.Range(0, (int)(colorsCount * 1.3))
            .Select(_ => (long)random.Next((int)colorStats.Min, (int)colorStats.Max + 1))
            .Distinct()
            .Take(colorsCount)
            .ToList();

        // 3. Выполняем запрос по конкретным ID (Index Scan - максимально быстро)
        var words = await _db.Words
            .Where(w => targetWordIds.Contains(w.Id))
            .ToListAsync();

        var colors = await _db.Colors
            .Where(c => targetColorIds.Contains(c.Id))
            .ToListAsync();

        // 1. Создаем копию списка для перемешивания
        var shuffledPool = new List<Color>();
        int poolIndex = 0;

        // Вспомогательная функция для обновления и перемешивания пула
        void RefreshShuffledPool()
        {
            shuffledPool = colors.OrderBy(_ => random.Next()).ToList();
            poolIndex = 0;
        }

        // Первичная инициализация
        RefreshShuffledPool();

        // 2. Собираем результат
        return words.Select(word =>
        {
            // Если мы использовали все цвета из текущего пула — перемешиваем их снова
            if (poolIndex >= shuffledPool.Count)
            {
                RefreshShuffledPool();
            }

            var color = shuffledPool[poolIndex++];

            return new TrainingWord
            {
                WordId = word.Id,
                Word = word.WordValue.ToUpperInvariant(),
                ColorId = color.Id,
                Color = color.ColorName
            };
        }).ToList();
    }


    public async Task<int> ImportWordsAsync(List<string> words)
    {
        if (words == null || !words.Any()) return 0;

        // 1. Подготовка данных в памяти (чистим и в нижний регистр)
        var cleanWords = words
            .Select(w => w.Trim().ToLowerInvariant()) // Храним в lowercase
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Distinct()
            .ToList();

        // 2. Вставляем через RAW SQL с игнорированием дубликатов
        // Делим на батчи по 500-1000 слов, чтобы не раздувать SQL-запрос
        int totalAdded = 0;
        foreach (var chunk in cleanWords.Chunk(500))
        {
            // Формируем VALUES (?, ?, ?)
            var values = string.Join(",", chunk.Select(w => $"('{w.Replace("'", "''")}')"));
            var sql = $"INSERT IGNORE INTO Words (WordValue) VALUES {values};";

            // Выполняем напрямую в БД
            totalAdded += await _db.Database.ExecuteSqlRawAsync(sql);
        }

        return totalAdded;
    }
}