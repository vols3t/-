using StudyHelper.API.Data;
using StudyHelper.API.Models;

namespace StudyHelper.API.Repository;

public class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Question?> GetByIdAsync(int id)
    {
        return await _context.Questions.FindAsync(id);
    }

    public async Task AddRangeAsync(IEnumerable<Question> questions)
    {
        await _context.Questions.AddRangeAsync(questions);
        
        await _context.SaveChangesAsync();
    }
}