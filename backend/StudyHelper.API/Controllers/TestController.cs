using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.Models;
using System.Text.Json;
using System.Text;
using StudyHelper.API.Data;
using StudyHelper.API.DTO;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly string _apiKey;
    private readonly ApplicationDbContext _context;

    public TestController(IConfiguration configuration, ApplicationDbContext context)
    {
        _context = context;
        _apiKey = configuration["OpenRouter:ApiKey"]
                  ?? throw new Exception("Ключ API не найден в настройках!");
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTest([FromBody] TestRequestModel? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest(new { error = "Тема теста не может быть пустой" });

        var count = request.QuestionsCount > 0 ? request.QuestionsCount : 3;

        var prompt = $@"Ты крутой учитель. Создай тест из {count} вопросов по теме: '{request.Topic}'.
        Верни ответ СТРОГО в формате массива JSON. Никаких приветствий, никакого текста до и после массива!
        Структура каждого объекта должна быть ТОЧНО такой, как у этого примера:
        [
            {{
                ""id"": 1,
                ""questionText"": ""Какой спутник у Земли?"",
                ""options"": [""Фобос"", ""Луна"", ""Европа"", ""Титан""],
                ""correctAnswer"": ""Луна""
            }}
        ]";

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5152");
            client.DefaultRequestHeaders.Add("X-Title", "StudyHelperLocal");

            var requestBody = new
            {
                model = "nvidia/nemotron-3-nano-30b-a3b:free",
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 2000
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("\n=== ОШИБКА ОТ OPENROUTER ===");
                Console.WriteLine($"Статус: {response.StatusCode}");
                Console.WriteLine($"Текст ошибки: {responseString}");
                Console.WriteLine("============================\n");
                return StatusCode(500, new { error = "Ошибка от API нейросети. Проверь консоль VS Code." });
            }

            using var jsonDoc = JsonDocument.Parse(responseString);
            var aiText = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(aiText))
                return BadRequest(new { error = "Нейросеть вернула пустой ответ" });

            aiText = aiText.Replace("```json", "").Replace("```", "").Trim();

            Console.WriteLine("\n=== ОТВЕТ НЕЙРОСЕТИ ===");
            Console.WriteLine(aiText);
            Console.WriteLine("=======================\n");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resultJson = JsonSerializer.Deserialize<List<Question>>(aiText, options);
            foreach (var question in resultJson)
                question.Id = 0;
            _context.Questions.AddRange(resultJson);
            await _context.SaveChangesAsync();

            var viewJson = resultJson.Select(q => new { q.Id, questionText = q.Text, options = q.Answers });

            return Ok(viewJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
            return StatusCode(500, new { error = "Произошла сбоя в коде сервера." });
        }
    }
}