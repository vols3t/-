using StudyHelper.API.DTO;
using StudyHelper.API.Models;
using StudyHelper.API.Repository;

namespace StudyHelper.API.Services;

public class TestService : ITestService
{
    private readonly IQuestionRepository _questionRepository;

    public TestService(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public async Task<TestResultViewModel> CheckTestAsync(List<UserAnswerDto> userAnswers)
    {
        var result = new TestResultViewModel();
        foreach (var userAnswer in userAnswers)
        {
            var questionId = userAnswer.QuestionID;
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null) continue;
            var answer = userAnswer.Answer;
            var isCorrectAnswer = string.Equals(question.CorrectAnswer.Trim(), answer.Trim(),
                StringComparison.OrdinalIgnoreCase);
            var questionResultDto = new QuestionResultDto()
            {
                QuestionId = question.Id,
                QuestionText = question.Text,
                Answers = question.Answers,
                RealAnswer = answer,
                IsCorrectAnswer = isCorrectAnswer,
            };
            result.Questions.Add(questionResultDto);
        }

        return result;
    }
}