using Microsoft.AspNetCore.Mvc;
using HabitContract.Application.DTOs;

namespace HabitContract.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "操作成功")
        => Ok(ApiResponse<T>.Success(data, message));

    protected IActionResult Success(string message = "操作成功")
        => Ok(ApiResponse.Success(message));

    protected int GetCurrentUserId()
        => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
