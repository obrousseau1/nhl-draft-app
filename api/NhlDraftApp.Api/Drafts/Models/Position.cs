using System.Text.Json.Serialization;

namespace NhlDraftApp.Api.Drafts.Models;

[JsonConverter(typeof(JsonStringEnumConverter<Position>))]
public enum Position { C, LW, RW, D, G, Team }
