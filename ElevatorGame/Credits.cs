using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevatorGame;

public class CreditsData()
{
    public static JsonSerializerOptions SerializerOptions => new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        TypeInfoResolver = CreditsDataSourceGenContext.Default,
    };

    public Dictionary<string, List<string>> Sections { get; set; } = [];
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CreditsData))]
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
[JsonSerializable(typeof(List<string>))]
internal partial class CreditsDataSourceGenContext : JsonSerializerContext;
