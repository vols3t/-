using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.DTO;
using StudyHelper.API.Models;
using StudyHelper.API.Services;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmitController : ControllerBase
{
    private readonly ITestService _testService;

    public SubmitController(ITestService testService)
    {
        _testService = testService;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitTest([FromBody] List<UserAnswerDto> userAnswers)
    {
        var viewModel = await _testService.CheckTestAsync(userAnswers);
        return Ok(viewModel);
    }
}