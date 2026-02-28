using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.Models;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpPost("create")]
    public IActionResult CreateTest([FromBody] TestRequestModel request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Topic))
        {
            return BadRequest(new { error = "Тема теста не может быть пустой" });
        }

        var response = new
        {
            message = "Тест успешно создан",
            test = new
            {
                topic = request.Topic,
                questionsCount = request.QuestionsCount,
            }
        };
        return Ok(response);
    }
}