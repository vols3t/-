using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.Models;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController: ControllerBase
{
    [HttpPost("create")]
    public IActionResult CreateTest([FromBody] TestRequestModel request)
    {
        return Ok();
    }
}