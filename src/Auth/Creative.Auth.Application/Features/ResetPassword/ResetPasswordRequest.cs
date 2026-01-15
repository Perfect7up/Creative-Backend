namespace Creative.Auth.Application.Features.ResetPassword;

public record ResetPasswordRequest(string Token, string NewPassword);