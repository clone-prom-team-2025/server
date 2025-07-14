using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

public class UserAdditionalInfo
{
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public bool? PhoneNumberConfirmed { get; set; } = false;

    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Allowed values: Male, Female, Other")]
    public string? Gender { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DateOfBirth { get; set; }
}