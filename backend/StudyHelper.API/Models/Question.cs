using System.Text.Json.Serialization;

namespace StudyHelper.API.Models;

public class Question
{
    public int Id { get; set; }

    [JsonPropertyName("questionText")] public string Text { get; set; } = "";

    [JsonPropertyName("options")] public List<string> Answers { get; set; } = new();

    public string CorrectAnswer { get; set; } = "";

    [JsonPropertyName("explanation")] public string Explanation { get; set; } = "";
}