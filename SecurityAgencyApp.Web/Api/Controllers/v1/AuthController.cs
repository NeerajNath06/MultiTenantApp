using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Authentication.Commands.Login;
using SecurityAgencyApp.Application.Features.Authentication.Commands.RegisterAgency;
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
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return Unauthorized(result);
    }

    [HttpPost("login")]
    [Consumes("application/x-www-form-urlencoded")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginForm([FromForm] LoginCommand command)
    {
        return Login(command);
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("register")]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterAgencyCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("register")]
    [Consumes("application/x-www-form-urlencoded")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task<ActionResult<ApiResponse<LoginResponseDto>>> RegisterForm([FromForm] RegisterAgencyCommand command)
    {
        return Register(command);
    }

    /// <summary>
    /// Forgot password – lookup stored plain password by email (for recovery / admin support).
    /// </summary>
    [HttpPost("forgot-password")]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponseDto>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email))
        {
            return BadRequest(ApiResponse<ForgotPasswordResponseDto>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create("Email is required", "email") }));
        }

        var userRepo = _unitOfWork.Repository<User>();
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepo.FirstOrDefaultIgnoreFiltersAsync(
            u => u.Email != null && u.Email.ToLower() == email,
            CancellationToken.None);

        if (user == null)
        {
            return NotFound(ApiResponse<ForgotPasswordResponseDto>.ErrorResponse(
                "No account found with this email",
                new[] { ApiError.Create("No account found with this email", "email") }));
        }

        var dto = new ForgotPasswordResponseDto
        {
            Email = user.Email,
            UserName = user.UserName,
            Password = user.Password ?? "(not stored – use reset)"
        };
        return Ok(ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(dto, "Password retrieved for recovery"));
    }

    [HttpPost("forgot-password")]
    [Consumes("application/x-www-form-urlencoded")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task<ActionResult<ApiResponse<ForgotPasswordResponseDto>>> ForgotPasswordForm([FromForm] ForgotPasswordRequest request)
    {
        return ForgotPassword(request);
    }

    /// <summary>
    /// Reset password – set a new password for the account (by email). Use when password was not stored or user forgot.
    /// </summary>
    [HttpPost("reset-password")]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create("Email is required", "email") }));
        }
        if (string.IsNullOrWhiteSpace(request?.NewPassword) || request.NewPassword.Length < 6)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create("New password must be at least 6 characters", "newPassword") }));
        }

        var userRepo = _unitOfWork.Repository<User>();
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepo.FirstOrDefaultIgnoreFiltersAsync(
            u => u.Email != null && u.Email.ToLower() == email,
            CancellationToken.None);

        if (user == null)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(
                "No account found with this email",
                new[] { ApiError.Create("No account found with this email", "email") }));
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.Password = request.NewPassword;
        user.ModifiedDate = DateTime.UtcNow;
        await userRepo.UpdateAsync(user, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password updated. You can now login with the new password."));
    }

    [HttpPost("reset-password")]
    [Consumes("application/x-www-form-urlencoded")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task<ActionResult<ApiResponse<bool>>> ResetPasswordForm([FromForm] ResetPasswordRequest request)
    {
        return ResetPassword(request);
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
