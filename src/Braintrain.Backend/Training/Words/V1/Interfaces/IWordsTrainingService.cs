using Braintrain.Backend.Training.Words.V1.Models;

namespace Braintrain.Backend.Training.Words.V1.Interfaces;

public interface IWordsTrainingService
{
    public Task<List<TrainingWord>> GetWordsAsync(int wordsCount, int colorsCount, long? userId);
    public Task<int> ImportWordsAsync(List<string> words);
}
