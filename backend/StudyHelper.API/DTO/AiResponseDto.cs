using System.Text.Json.Serialization;
using StudyHelper.API.Models;

namespace StudyHelper.API.DTO;

public class AiResponseDto
{
    [JsonPropertyName("questions")] public List<Question> Questions { get; set; } = new();
}