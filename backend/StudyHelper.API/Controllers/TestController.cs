using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.Models;
using System.Text.Json;
using System.Text;
using System.Net;
using StudyHelper.API.Data;

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
                  ?? throw new Exception("Ключ API OpenRouter не найден в настройках!");
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTest([FromBody] TestRequestModel? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest(new { error = "Тема теста не может быть пустой" });

        var count = request.QuestionsCount > 0 ? request.QuestionsCount : 3;
        var prompt = $@"Ты крутой учитель. Создай тест из {count} вопросов по теме: '{request.Topic}'.
Верни ответ СТРОГО в формате JSON. Никаких приветствий или текста до и после JSON!
Структура должна быть ТОЧНО такой (объект со списком вопросов):
{{
  ""questions"": [
    {{
      ""id"": 1,
      ""questionText"": ""Какой спутник у Земли?"",
      ""options"": [""Фобос"", ""Луна"", ""Европа"", ""Титан""],
      ""correctAnswer"": ""Луна""
    }}
  ]
}}";

        try
        {
            var proxy = new WebProxy { Address = new Uri("socks5://127.0.0.1:1080") };
            var handler = new HttpClientHandler { Proxy = proxy };
            using var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", "http://studyhelper.ru");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", "StudyHelperApp");

            var requestBody = new
            {
                model = "openai/gpt-4o-mini",
                messages = new[] { new { role = "user", content = prompt } },
                response_format = new { type = "json_object" },
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ERROR] Status: {response.StatusCode}");
                Console.WriteLine($"[ERROR] Body: {responseString}");
                return StatusCode((int)response.StatusCode, new { error = "Ошибка нейросети", details = responseString });
            }

            using var jsonDoc = JsonDocument.Parse(responseString);
            var aiText = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return Ok(aiText);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}