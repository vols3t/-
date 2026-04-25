using StudyHelper.API.Models;

namespace StudyHelper.API.DTO;

public class AiResponseDto
{
    public List<Question> Questions { get; set; } = new();
}