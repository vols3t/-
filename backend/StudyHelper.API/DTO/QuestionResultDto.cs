namespace StudyHelper.API.DTO;

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public string? QuestionText { get; set; }
    public List<string>? Answers { get; set; }
    public string? RealAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrectAnswer { get; set; } = false;
}