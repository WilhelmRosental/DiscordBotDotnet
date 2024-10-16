using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DiscordBotDotNet.Models;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("UserId")]
    public ulong UserId { get; set; }

    [BsonElement("GuildId")]
    public ulong GuildId { get; set; }

    [BsonElement("Username")]
    public string? Username { get; set; }

    [BsonElement("LastActivity")]
    public DateTime LastActivity { get; set; }

    [BsonElement("Gender")]
    public string? Gender { get; set; }

    [BsonElement("Birthday")]
    public DateTime Birthday { get; set; }

    [BsonElement("PersonalityType")]
    public string? PersonalityType { get; set; }

    [BsonElement("QuestionnaireProgress")]
    public int? QuestionnaireProgress { get; set; }

    [BsonElement("House")] // Nouvelle propriété pour la maison
    public string? House { get; set; }
}