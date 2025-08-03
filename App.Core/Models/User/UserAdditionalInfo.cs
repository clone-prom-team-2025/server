using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.User;

public class UserAdditionalInfo
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }

    public bool? PhoneNumberConfirmed { get; set; } = false;

    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Allowed values: Male, Female, Other")]
    public string? Gender { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DateOfBirth { get; set; }
}