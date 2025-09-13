using System.Text.Json;
using System.Text.Json.Serialization;
using HrAPI.Models;

namespace HrAPI.Converters;

public class AbsenceTypeJsonConverter : JsonConverter<AbsenceType>
{
    public override AbsenceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (int.TryParse(stringValue, out var intValue) && Enum.IsDefined(typeof(AbsenceType), intValue))
            {
                return (AbsenceType)intValue;
            }
            
            if (Enum.TryParse<AbsenceType>(stringValue, true, out var enumValue))
            {
                return enumValue;
            }
            
            throw new JsonException($"Unable to convert '{stringValue}' to AbsenceType.");
        }
        
        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(AbsenceType), intValue))
            {
                return (AbsenceType)intValue;
            }
            
            throw new JsonException($"Unable to convert '{intValue}' to AbsenceType.");
        }
        
        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, AbsenceType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}
