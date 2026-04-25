using StudyHelper.API.DTO;
using StudyHelper.API.Models;

namespace StudyHelper.API.Services;

public interface ITestService
{
    Task<TestResultViewModel> CheckTestAsync(List<UserAnswerDto> userAnswers);
    
    Task<List<Question>> CreateAndSaveTestAsync(string topic, int count);
}