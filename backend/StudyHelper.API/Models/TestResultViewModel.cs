using StudyHelper.API.DTO;

namespace StudyHelper.API.Models;

public class TestResultViewModel
{
    public List<QuestionResultDto> Questions { get; set; }
    public double TotalAnswers => Questions.Count;
    public int CorrectAnswers => Questions.Count(x => x.IsCorrectAnswer);
    public double AverageScore => TotalAnswers > 0 ? CorrectAnswers / TotalAnswers * 100 : 0;

    public TestResultViewModel()
    {
        Questions = new List<QuestionResultDto>();
    }
}