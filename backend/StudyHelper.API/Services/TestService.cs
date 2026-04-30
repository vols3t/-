// using StudyHelper.API.Models;
// using StudyHelper.API.DTO;
// using StudyHelper.API.Repository;
// using Ganss.Xss;
// using System.Text.Json;
// using System.Text;
// using System.Net;
//
// namespace StudyHelper.API.Services;
//
// public class TestService : ITestService
// {
//     private readonly IQuestionRepository _questionRepository;
//     private readonly string _apiKey;
//
//     public TestService(IQuestionRepository questionRepository, IConfiguration configuration)
//     {
//         _questionRepository = questionRepository;
//         _apiKey = configuration["OpenRouter:ApiKey"] ?? throw new Exception("API Key missing");
//     }
//
//     public async Task<List<Question>> CreateAndSaveTestAsync(string topic, int count)
//     {
//         var aiText = await GetQuestionsFromAi(topic, count);
//
//         var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//         var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(aiText, options);
//         var questions = aiResponse?.Questions ?? new List<Question>();
//
//         var sanitizer = new HtmlSanitizer();
//         foreach (var q in questions)
//         {
//             q.Id = 0;
//             q.Text = sanitizer.Sanitize(q.Text); 
//
//             if (q.Answers != null)
//             {
//                 for (var i = 0; i < q.Answers.Count; i++)
//                 {
//                     q.Answers[i] = sanitizer.Sanitize(q.Answers[i]); 
//                 }
//             }
//
//             q.CorrectAnswer = sanitizer.Sanitize(q.CorrectAnswer); 
//         }
//
//         await _questionRepository.AddRangeAsync(questions);
//
//         return questions;
//     }
//
//     private async Task<string> GetQuestionsFromAi(string topic, int count)
//     {
//         var prompt = $@"Ты крутой учитель. Создай тест из {count} вопросов по теме: '{topic}'.
//                Верни ответ СТРОГО в формате JSON. Никаких приветствий или текста до и после JSON!
//                 Структура должна быть ТОЧНО такой (объект со списком вопросов):
//                 {{
//                   ""questions"": [
//                     {{
//                       ""id"": 1,
//                       ""questionText"": ""Какой спутник у Земли?"",
//                       ""options"": [""Фобос"", ""Луна"", ""Европа"", ""Титан""],
//                       ""correctAnswer"": ""Луна""
//                     }}
//                   ]
//                 }}";
//
//         // var proxy = new WebProxy { Address = new Uri("socks5://127.0.0.1:1080") };
//         // var handler = new HttpClientHandler { Proxy = proxy };
//         // using var client = new HttpClient(handler);
//         using var client = new HttpClient();
//
//         client.DefaultRequestHeaders.Clear();
//         client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");
//         client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", "http://studyhelper.ru");
//         client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", "StudyHelperApp");
//
//         var requestBody = new
//         {
//             model = "openai/gpt-4o-mini",
//             messages = new[] { new { role = "user", content = prompt } },
//             response_format = new { type = "json_object" },
//         };
//
//         var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions",
//             new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
//
//         var responseString = await response.Content.ReadAsStringAsync();
//
//         using var jsonDoc = JsonDocument.Parse(responseString);
//         return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content")
//             .GetString() ?? "";
//     }
//     
//     public async Task<TestResultViewModel> CheckTestAsync(List<UserAnswerDto> userAnswers)
//     {
//         var result = new TestResultViewModel();
//         foreach (var userAnswer in userAnswers)
//         {
//             var questionId = userAnswer.QuestionID;
//             var question = await _questionRepository.GetByIdAsync(questionId);
//             if (question == null) continue;
//             var answer = userAnswer.Answer;
//             var isCorrectAnswer = string.Equals(question.CorrectAnswer.Trim(), answer.Trim(),
//                 StringComparison.OrdinalIgnoreCase);
//             var questionResultDto = new QuestionResultDto()
//             {
//                 QuestionId = question.Id,
//                 QuestionText = question.Text,
//                 Answers = question.Answers,
//                 RealAnswer = answer,
//                 IsCorrectAnswer = isCorrectAnswer,
//                 CorrectAnswer = question.CorrectAnswer
//             };
//             result.Questions.Add(questionResultDto);
//         }
//
//         return result;
//     }
// }

using StudyHelper.API.Models;
using StudyHelper.API.DTO;
using StudyHelper.API.Repository;
using Ganss.Xss;
using System.Text.Json;
using System.Text;
using System.Net;

namespace StudyHelper.API.Services;

public class TestService : ITestService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;

    public TestService(IQuestionRepository questionRepository, IConfiguration configuration)
    {
        _questionRepository = questionRepository;
        _configuration = configuration;
        _apiKey = configuration["OpenRouter:ApiKey"] ?? throw new Exception("API Key missing");
    }

    public async Task<List<Question>> CreateAndSaveTestAsync(string topic, int count)
    {
        var aiText = await GetQuestionsFromAi(topic, count);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var aiResponse = JsonSerializer.Deserialize<AiResponseDto>(aiText, options);
        var questions = aiResponse?.Questions ?? new List<Question>();

        var sanitizer = new HtmlSanitizer();
        foreach (var q in questions)
        {
            q.Id = 0;
            q.Text = sanitizer.Sanitize(q.Text);
            if (q.Answers != null)
                for (var i = 0; i < q.Answers.Count; i++)
                    q.Answers[i] = sanitizer.Sanitize(q.Answers[i]);
            q.CorrectAnswer = sanitizer.Sanitize(q.CorrectAnswer);
        }

        await _questionRepository.AddRangeAsync(questions);
        return questions;
    }

    private async Task<string> GetQuestionsFromAi(string topic, int count)
    {
        var prompt =
            $@"Создай тест из {count} вопросов по теме: '{topic}'. 
        СТРОГО JSON: {{""questions"": [{{""id"": 1, ""questionText"": ""текст"", ""options"": [""а"",""б""], 
        ""correctAnswer"": ""а""}}]}}";

        var useProxy = _configuration.GetValue<bool>("OpenRouter:UseProxy");
        HttpClient client;

        if (useProxy)
        {
            var handler = new HttpClientHandler { Proxy = new WebProxy("socks5://127.0.0.1:1080") };
            client = new HttpClient(handler);
        }
        else
        {
            client = new HttpClient();
        }

        using (client)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");
            var requestBody = new
            {
                model = "openai/gpt-4o-mini",
                messages = new[] { new { role = "user", content = prompt } },
                response_format = new { type = "json_object" }
            };

            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);
            return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content")
                .GetString() ?? "";
        }
    }

    public async Task<TestResultViewModel> CheckTestAsync(List<UserAnswerDto> userAnswers)
    {
        var result = new TestResultViewModel();
        foreach (var userAnswer in userAnswers)
        {
            var question = await _questionRepository.GetByIdAsync(userAnswer.QuestionID);
            if (question == null) continue;
            var isCorrect = string.Equals(question.CorrectAnswer.Trim(), userAnswer.Answer.Trim(),
                StringComparison.OrdinalIgnoreCase);
            result.Questions.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                QuestionText = question.Text,
                Answers = question.Answers,
                RealAnswer = userAnswer.Answer,
                IsCorrectAnswer = isCorrect,
                CorrectAnswer = question.CorrectAnswer,
                Explanation = isCorrect ? null : question.Explanation
            });
        }

        return result;
    }
}