using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Authentication.Commands.Login;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(IMediator mediator, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Forgot password – lookup stored plain password by email (for recovery / admin support).
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponseDto>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email))
        {
            return Ok(ApiResponse<ForgotPasswordResponseDto>.ErrorResponse("Email is required"));
        }

        var userRepo = _unitOfWork.Repository<User>();
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepo.FirstOrDefaultIgnoreFiltersAsync(
            u => u.Email != null && u.Email.ToLower() == email,
            CancellationToken.None);

        if (user == null)
        {
            return Ok(ApiResponse<ForgotPasswordResponseDto>.ErrorResponse("No account found with this email"));
        }

        var dto = new ForgotPasswordResponseDto
        {
            Email = user.Email,
            UserName = user.UserName,
            Password = user.Password ?? "(not stored – use reset)"
        };
        return Ok(ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(dto, "Password retrieved for recovery"));
    }

    /// <summary>
    /// Reset password – set a new password for the account (by email). Use when password was not stored or user forgot.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email))
        {
            return Ok(ApiResponse<bool>.ErrorResponse("Email is required"));
        }
        if (string.IsNullOrWhiteSpace(request?.NewPassword) || request.NewPassword.Length < 6)
        {
            return Ok(ApiResponse<bool>.ErrorResponse("New password must be at least 6 characters"));
        }

        var userRepo = _unitOfWork.Repository<User>();
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepo.FirstOrDefaultIgnoreFiltersAsync(
            u => u.Email != null && u.Email.ToLower() == email,
            CancellationToken.None);

        if (user == null)
        {
            return Ok(ApiResponse<bool>.ErrorResponse("No account found with this email"));
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.Password = request.NewPassword;
        user.ModifiedDate = DateTime.UtcNow;
        await userRepo.UpdateAsync(user, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password updated. You can now login with the new password."));
    }
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
