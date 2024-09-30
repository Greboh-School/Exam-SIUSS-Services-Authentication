using System.ComponentModel;
using System.Text.Json.Serialization;

namespace School.Exam.SIUSS.Services.Authentication.Models.Requests;

public sealed record CreateIdentityRequest
{
    public required string UserName { get; set; } = default!;
    public required string Password { get; set; } = default!;

    [DefaultValue("SYSTEM"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; } = "SYSTEM";
}