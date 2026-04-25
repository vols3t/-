using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.Models;
using StudyHelper.API.Services;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ITestService _testService;

    public TestController(ITestService testService)
    {
        _testService = testService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTest([FromBody] TestRequestModel? request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest(new { error = "Тема пуста" });

        try
        {
            var questions = await _testService.CreateAndSaveTestAsync(request.Topic, request.QuestionsCount);

            var responseForFront = new
            {
                questions = questions.Select(q => new
                {
                    q.Id,
                    questionText = q.Text,
                    options = q.Answers
                })
            };

            return Ok(responseForFront);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка сервера", details = ex.Message });
        }
    }
}