using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.Models;
using System.Text.Json;
using System.Text;
using System.Net;
using Ganss.Xss;
using StudyHelper.API.Data;
using StudyHelper.API.DTO;
using StudyHelper.API.Repository;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly string _apiKey;
    private readonly ApplicationDbContext _context;
    private readonly IQuestionRepository _questionRepository;

    public TestController(IConfiguration configuration, ApplicationDbContext context, IQuestionRepository questionRepository)
    {
        _context = context;
        _questionRepository = questionRepository;
        _apiKey = configuration["OpenRouter:ApiKey"]
                  ?? throw new Exception("Ключ API OpenRouter не найден в настройках!");
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTest([FromBody] TestRequestModel? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest(new { error = "Тема теста не может быть пустой" });

        try
        {
            var count = Math.Min(request.QuestionsCount > 0 ? request.QuestionsCount : 3, 5);
            var prompt = $@"Создай JSON тест на тему: '{request.Topic}'. Количество вопросов: {count}.
СТРОГО верни только JSON без пояснений. Важно: поле correctAnswer должно быть ТОЧНОЙ копией одного из элементов массива options (тот же текст, регистр, пробелы).
Пример: {{""questions"":[{{""id"":1,""questionText"":""Столица Франции?"",""options"":[""Москва"",""Париж"",""Берлин"",""Рим""],""correctAnswer"":""Париж"",""explanation"":""Париж является столицей Франции с X века.""}}]}}";

            // var proxy = new WebProxy { Address = new Uri("socks5://127.0.0.1:1080") };
            // var handler = new HttpClientHandler { Proxy = proxy };
            // using var client = new HttpClient(handler);
            using var client = new HttpClient();

            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");

            var requestBody = new
            {
                model = "openai/gpt-4o-mini",
                messages = new[] { new { role = "user", content = prompt } },
                response_format = new { type = "json_object" },
            };

            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);
            
            if (!jsonDoc.RootElement.TryGetProperty("choices", out var choices))
            {
                Console.WriteLine("--- ОШИБКА ФОРМАТА ОТ ИИ ---");
                Console.WriteLine(responseString); 
                return StatusCode(500, new { error = "ИИ прислал странный ответ. Посмотри логи сервера." });
            }

            var aiText = choices[0].GetProperty("message").GetProperty("content").GetString();
            
            // var aiText = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content")
            //     .GetString();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(aiText, options);
            var questions = aiResponse?.Questions ?? new List<Question>();

            var sanitizer = new HtmlSanitizer();
            foreach (var q in questions)
            {
                q.Id = 0;
                q.Text = sanitizer.Sanitize(q.Text);
                if (q.Answers != null)
                {
                    for (int i = 0; i < q.Answers.Count; i++)
                        q.Answers[i] = sanitizer.Sanitize(q.Answers[i]);
                }
                q.CorrectAnswer = sanitizer.Sanitize(q.CorrectAnswer);
                q.Explanation = sanitizer.Sanitize(q.Explanation);
            }

            await _questionRepository.AddRangeAsync(questions);

            return Ok(new
            {
                questions = questions.Select(q => new
                {
                    q.Id,
                    questionText = q.Text,
                    options = q.Answers,
                    correctAnswer = q.CorrectAnswer,
                    explanation = q.Explanation
                })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}