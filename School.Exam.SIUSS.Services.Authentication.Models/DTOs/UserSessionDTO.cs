namespace School.Exam.SIUSS.Services.Authentication.Models.DTOs;

public sealed record UserSessionDTO(Guid UserId, string UserName, string AccessToken);