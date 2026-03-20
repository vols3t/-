namespace StudyHelper.API.DTO;

public class QuestionResultDto
{
    public int ID { get; private set; }

    public string Question { get; private set; }
    public List<string> Answers { get; set; }
    public string CorrectAnswer { get; private set; }
    public string RealAnswer { get; set; }
    public bool IsCorrectAnswer { get; set; } = false;

    public QuestionResultDto(int id, string question, string correctAnswer)
    {
        ID = id;
        Question = question;
        CorrectAnswer = correctAnswer;
    }
}