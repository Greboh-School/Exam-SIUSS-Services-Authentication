namespace School.Exam.SIUSS.Services.Authentication.Models.Options;

public class TokenOptions
{
    public static string Section => "Token";
    public float LifeTimeInMinutes { get; set; }
}