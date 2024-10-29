using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Web.ViewModels.Response;

public class ApiResponse<T> : IApiResponse<T>
{
    public ApiResponse()
    {
    }

    public ApiResponse(T? data)
    {
        Data = data;
    }

    public int StatusCode { get; set; } = 200;
    public bool Successful { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }

    /// <summary>
    /// Implements implicit conversion from <see cref="ApiResponse"/> to <see cref="ApiResponse{T}"/>
    /// </summary>
    /// <param name="apiResponse">The <see cref="ApiResponse"/> instance</param>
    /// <returns>A new <see cref="ApiResponse{T}"/> instance</returns>
    public static implicit operator ApiResponse<T>(ApiResponse apiResponse)
    {
        return new ApiResponse<T>
        {
            Successful = apiResponse.Successful,
            Message = apiResponse.Message
        };
    }
}

public class ApiResponse : IApiResponse, IApiErrorResponse
{
    public int StatusCode { get; set; } = 200;
    public bool Successful { get; set; }
    public string? Message { get; set; }
    public SerializableError ErrorData { get; set; }

    public static ApiResponse NoContent(HttpResponse httpResponse, string message = "NoContent")
    {
        httpResponse.StatusCode = StatusCodes.Status204NoContent;
        return new ApiResponse
        {
            StatusCode = httpResponse.StatusCode,
            Successful = true,
            Message = message
        };
    }

    public static ApiResponse Ok(HttpResponse httpResponse, string message = "Ok")
    {
        httpResponse.StatusCode = StatusCodes.Status200OK;
        return new ApiResponse
        {
            StatusCode = httpResponse.StatusCode,
            Successful = true,
            Message = message
        };
    }

    public static ApiResponse Unauthorized(HttpResponse httpResponse, string message = "Unauthorized")
    {
        httpResponse.StatusCode = StatusCodes.Status401Unauthorized;
        return new ApiResponse
        {
            StatusCode = httpResponse.StatusCode,
            Successful = false,
            Message = message
        };
    }

    public static ApiResponse NotFound(HttpResponse httpResponse, string message = "NotFound")
    {
        httpResponse.StatusCode = StatusCodes.Status404NotFound;
        return new ApiResponse
        {
            StatusCode = httpResponse.StatusCode,
            Successful = false,
            Message = message
        };
    }

    public static ApiResponse BadRequest(HttpResponse httpResponse, string message = "BadRequest")
    {
        httpResponse.StatusCode = StatusCodes.Status400BadRequest;
        return new ApiResponse
        {
            StatusCode = httpResponse.StatusCode,
            Successful = false,
            Message = message
        };
    }

    public static ApiResponse BadRequest(HttpResponse httpResponse,
        ModelStateDictionary modelState, string message = "ModelState is not valid.")
    {
        httpResponse.StatusCode = StatusCodes.Status400BadRequest;
        return new ApiResponse
        {
            StatusCode = httpResponse.StatusCode,
            Successful = false,
            Message = message,
            ErrorData = new SerializableError(modelState)
        };
    }

    public static ApiResponse Error(HttpResponse httpResponse, string message = "Error")
    {
        httpResponse.StatusCode = StatusCodes.Status500InternalServerError;
        return new ApiResponse
        {
            StatusCode = httpResponse.StatusCode,
            Successful = false,
            Message = message
        };
    }
}