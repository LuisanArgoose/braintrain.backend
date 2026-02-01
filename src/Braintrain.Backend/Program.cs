using Asp.Versioning;
using Braintrain.Backend.Training.Words.V1.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);




var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddWordsTraining(configuration);

services
    .AddCors(options =>
    {
        options.AddPolicy("DevCorsPolicy", policy =>
        {
            policy.WithOrigins("http://localhost:3000") // Разрешаем твой фронтенд
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.UseCors("DevCorsPolicy");

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
