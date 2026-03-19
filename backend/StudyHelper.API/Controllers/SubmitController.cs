using Microsoft.AspNetCore.Mvc;
using StudyHelper.API.DTO;
using StudyHelper.API.Models;

namespace StudyHelper.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmitController : ControllerBase
{
    [HttpPost("submit")]
    public IActionResult SubmitTest([FromBody] List<UserAnswerDto> userAnswers)
    {
        var viewModel = new TestResultViewModel();
        foreach (var userAnswer in userAnswers)
        {
            // пока нет бд, в качестве заглушки правильный ответ всегда А
            var dto = new QuestionResultDto(userAnswer.QuestionID,
                $"здесь будет сам вопрос (найдем по ID) {userAnswer.QuestionID}",
                "Вариант А")
            {
                RealAnswer = userAnswer.Answer
            };
            if (userAnswer.Answer == "Вариант А")
            {
                dto.IsCorrectAnswer = true;
            }

            viewModel.Questions.Add(dto);
        }

        return Ok(viewModel);
    }
}