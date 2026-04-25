using StudyHelper.API.Models;

namespace StudyHelper.API.Repository;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(int id);
    
    Task AddRangeAsync(IEnumerable<Question> questions);
}