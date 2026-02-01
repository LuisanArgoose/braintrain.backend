using Braintrain.Backend.Training.Words.V1.Interfaces;
using Braintrain.Backend.Training.Words.V1.Services;
using Microsoft.EntityFrameworkCore;

namespace Braintrain.Backend.Training.Words.V1.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddWordsTraining(this  IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BraintrainWordsDatabase");
        services
            .AddDbContext<WordsTrainingDbContext>(opt =>
            {
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
        services.AddScoped<IWordsTrainingService, WordsTrainingService>();
        return services;

    }
}
