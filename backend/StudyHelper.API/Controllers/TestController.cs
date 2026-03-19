using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.Models;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpPost("create")]
    public IActionResult CreateTest([FromBody] TestRequestModel? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Topic))
        {
            return BadRequest(new { error = "Тема теста не может быть пустой" });
        }

        // Здесь позже будет вызов к ChatGPT API, пока просто заглушка
        var mockQuestions = new List<object>();
        for (var i = 1; i <= request.QuestionsCount; i++)
        {
            mockQuestions.Add(new
            {
                id = i,
                questionText =
                    $"Вопрос: Lorem ipsum, dolor sit amet consectetur adipisicing elit. Repudiandae perferendis aliquam officia praesentium voluptatum similique reprehenderit recusandae obcaecati! Ut quaerat cumque laborum amet ducimus similique?",
                options = new[] { "Вариант А", "Вариант Б", "Вариант В", "Вариант Г" }
            });
        }

        return Ok(mockQuestions);
    }
}