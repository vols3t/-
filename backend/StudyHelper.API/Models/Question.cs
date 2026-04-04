namespace StudyHelper.API.Models;

public class Question
{
    public int Id { get; private set; }
    public string Text { get; private set; }
    public List<string> Answers { get; set; }
    public string CorrectAnswer { get; set; }

    public Question(int id, string text, string correctAnswer, List<string> answers)
    {
        Id = id;
        Text = text;
        CorrectAnswer = correctAnswer;
        Answers = answers;
    }
}