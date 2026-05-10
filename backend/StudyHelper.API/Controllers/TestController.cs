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
            var count = Math.Min(request.QuestionsCount > 0 ? request.QuestionsCount : 3, 20);

            var difficulty = (request.Difficulty ?? "medium").Trim().ToLowerInvariant();
            string difficultyBlock;
            switch (difficulty)
            {
                case "easy":
                    difficultyBlock =
                        "УРОВЕНЬ СЛОЖНОСТИ: ЛЁГКИЙ.\n" +
                        "- Базовые определения, термины, ключевые факты по теме.\n" +
                        "- Уровни Блума: запоминание и понимание.\n" +
                        "- Формулировки короткие, без двойных отрицаний и подвохов.\n" +
                        "- Правильный ответ виден тому, кто прочитал учебник по теме.";
                    break;
                case "hard":
                    difficultyBlock =
                        "УРОВЕНЬ СЛОЖНОСТИ: СЛОЖНЫЙ.\n" +
                        "- Анализ, синтез, оценка, нестандартные ситуации, ловушки на типовые заблуждения.\n" +
                        "- Уровни Блума: анализ, синтез, оценка. Никаких прямых определений.\n" +
                        "- Допустимы вопросы вида «что произойдёт, если...», «какой вывод следует из...», сравнение похожих понятий.\n" +
                        "- Дистракторы должны выглядеть правдоподобно для слабого студента и отражать частые ошибки.\n" +
                        "- Правильный ответ требует понимания механики/причинно-следственных связей, не зубрёжки.";
                    break;
                default:
                    difficulty = "medium";
                    difficultyBlock =
                        "УРОВЕНЬ СЛОЖНОСТИ: СРЕДНИЙ.\n" +
                        "- Применение знаний, разбор примеров, отличение похожих понятий друг от друга.\n" +
                        "- Уровни Блума: применение и анализ.\n" +
                        "- Дистракторы должны быть осмысленными, а не случайным мусором.";
                    break;
            }

            var systemPrompt =
                "Ты — опытный методист и преподаватель-предметник, который составляет качественные учебные тесты для российских студентов. " +
                "Твоя задача — сгенерировать тест с одиночным выбором (single-choice) по заданной теме.\n\n" +
                "ОБЩИЕ ТРЕБОВАНИЯ К КАЧЕСТВУ:\n" +
                "1. Каждый вопрос проверяет осмысленное знание по теме, а не общую эрудицию и не угадывание.\n" +
                "2. Ровно 4 варианта ответа. Ровно один правильный.\n" +
                "3. Дистракторы (неправильные варианты) — правдоподобные, близкие по длине и стилю к правильному ответу, " +
                "отражают типичные ошибки и заблуждения по теме. Никаких очевидно-абсурдных вариантов.\n" +
                "4. Запрещено: варианты вида «все вышеперечисленное», «ни один из вариантов», «нет правильного ответа», " +
                "«А и Б», нумерация внутри текста варианта. Никаких дубликатов или вариантов, отличающихся только пунктуацией.\n" +
                "5. Вопросы не должны повторять друг друга и не должны раскрывать ответы друг на друга.\n" +
                "6. Формулировка вопроса самодостаточна: студент должен понять вопрос без контекста соседних.\n" +
                "7. Поле explanation — короткое (1–2 предложения) объяснение, ПОЧЕМУ верен именно этот вариант, " +
                "со ссылкой на суть понятия. Не пересказывай вопрос.\n" +
                "8. Поле correctAnswer обязано быть ПОБАЙТОВО равной копии одного из элементов options (тот же текст, регистр, пробелы, знаки).\n" +
                "9. Язык — русский, грамотный, без канцелярита и без markdown внутри полей.\n\n" +
                "ФОРМАТ ОТВЕТА: строго один JSON-объект, без какого-либо текста до или после, без markdown-обёртки ```.\n" +
                "Схема:\n" +
                "{\n" +
                "  \"questions\": [\n" +
                "    {\n" +
                "      \"id\": 1,\n" +
                "      \"questionText\": \"...\",\n" +
                "      \"options\": [\"...\", \"...\", \"...\", \"...\"],\n" +
                "      \"correctAnswer\": \"...\",\n" +
                "      \"explanation\": \"...\"\n" +
                "    }\n" +
                "  ]\n" +
                "}";

            var userPrompt =
                $"Тема теста: «{request.Topic}».\n" +
                $"Количество вопросов: {count}.\n\n" +
                difficultyBlock + "\n\n" +
                "Перед тем как выдать JSON, мысленно выполни шаги (НЕ показывай их в ответе):\n" +
                "  a) выдели ключевые подтемы темы, чтобы вопросы покрывали разные её аспекты, а не дублировались;\n" +
                "  b) для каждого вопроса сформулируй правильный ответ, затем придумай 3 правдоподобных дистрактора, " +
                "соответствующих типичным ошибкам по уровню сложности;\n" +
                "  c) перемешай порядок вариантов так, чтобы правильный ответ не всегда стоял первым;\n" +
                "  d) проверь, что correctAnswer побайтово совпадает с одним из options.\n\n" +
                "Теперь выдай итоговый JSON по схеме выше. Только JSON.";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(90);

            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", "http://studyhelper.ru");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", "StudyHelperApp");

            var requestBody = new
            {
                model = "anthropic/claude-sonnet-4.5",
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.7,
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

            var aiText = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            // Иногда модель оборачивает JSON в ```json ... ``` или добавляет префиксный текст,
            // несмотря на инструкции и response_format. Вырезаем тело JSON между первой { и последней }.
            var cleanedJson = ExtractJsonObject(aiText);
            if (string.IsNullOrWhiteSpace(cleanedJson))
            {
                Console.WriteLine("--- ИИ не вернул JSON, сырой ответ: ---");
                Console.WriteLine(aiText);
                return StatusCode(500, new { error = "ИИ прислал ответ без JSON. Попробуй ещё раз." });
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(cleanedJson, options);
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

    // Вытаскивает JSON-объект из произвольного текста модели:
    // отрезает markdown-фенсы ```json ... ```, преамбулы и постамбулы.
    private static string ExtractJsonObject(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        var text = raw.Trim();

        // Снимаем markdown-фенс, если он есть: ```json\n...\n``` или ```\n...\n```
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline >= 0) text = text.Substring(firstNewline + 1);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }

        // На всякий случай — режем по фигурным скобкам верхнего уровня.
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) return string.Empty;

        return text.Substring(start, end - start + 1);
    }
}