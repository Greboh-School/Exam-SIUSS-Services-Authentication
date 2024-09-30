namespace School.Exam.SIUSS.Services.Authentication.Models.Requests;

public sealed record CreateUserSessionRequest(string UserName, string Password);