using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Web.ViewModels.Response;

/// <summary>
///     Represents a generic API response.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
public class ApiResponse<T> : IApiResponse<T>
{
    public ApiResponse()
    {
    }

    public ApiResponse(T? data)
    {
        Data = data;
    }

    /// <summary>
    ///     Gets or sets the status code of the response.
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    ///     Gets or sets a value indicating whether the response is successful.
    /// </summary>
    public bool Successful { get; set; } = true;

    /// <summary>
    ///     Gets or sets the message of the response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    ///     Gets or sets the data of the response.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    ///     Implicitly converts an <see cref="ApiResponse" /> to an <see cref="ApiResponse{T}" />.
    /// </summary>
    /// <param name="apiResponse">The API response.</param>
    /// <returns>The converted API response.</returns>
    public static implicit operator ApiResponse<T>(ApiResponse apiResponse)
    {
        return new ApiResponse<T>
        {
            StatusCode = apiResponse.StatusCode,
            Successful = apiResponse.Successful,
            Message = apiResponse.Message
        };
    }
}

/// <summary>
///     Represents a non-generic API response.
/// </summary>
public class ApiResponse : IApiResponse, IApiErrorResponse
{
    public ApiResponse()
    {
    }

    public ApiResponse(object data)
    {
        Data = data;
    }

    /// <summary>
    ///     Gets or sets the data of the response.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    ///     Gets or sets the serializable error data.
    ///     <para>Used to store model validation error information.</para>
    /// </summary>
    public Dictionary<string, object>? ErrorData { get; set; }

    /// <summary>
    ///     Gets or sets the status code of the response.
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    ///     Gets or sets a value indicating whether the response is successful.
    /// </summary>
    public bool Successful { get; set; } = true;

    /// <summary>
    ///     Gets or sets the message of the response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    ///     Creates a response indicating no content.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse NoContent(string message = "NoContent")
    {
        return new ApiResponse
        {
            StatusCode = StatusCodes.Status204NoContent,
            Successful = true,
            Message = message
        };
    }

    /// <summary>
    ///     Creates a response indicating success.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse Ok(string message = "Ok")
    {
        return new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Successful = true,
            Message = message
        };
    }

    /// <summary>
    ///     Creates a response indicating success with data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="message">The message.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse Ok(object data, string message = "Ok")
    {
        return new ApiResponse
        {
            StatusCode = StatusCodes.Status200OK,
            Successful = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    ///     Creates a response indicating unauthorized access.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse Unauthorized(string message = "Unauthorized")
    {
        return new ApiResponse
        {
            StatusCode = StatusCodes.Status401Unauthorized,
            Successful = false,
            Message = message
        };
    }

    /// <summary>
    ///     Creates a response indicating resource not found.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse NotFound(string message = "NotFound")
    {
        return new ApiResponse
        {
            StatusCode = StatusCodes.Status404NotFound,
            Successful = false,
            Message = message
        };
    }

    /// <summary>
    ///     Creates a response indicating a bad request.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse BadRequest(string message = "BadRequest")
    {
        return new ApiResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Successful = false,
            Message = message
        };
    }

    /// <summary>
    ///     Creates a response indicating a bad request with model state errors.
    /// </summary>
    /// <param name="modelState">The model state dictionary.</param>
    /// <param name="message">The message.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse BadRequest(ModelStateDictionary modelState, string message = "ModelState is not valid.")
    {
        return new ApiResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Successful = false,
            Message = message,
            ErrorData = new SerializableError(modelState)
        };
    }

    /// <summary>
    ///     Creates a response indicating an internal server error.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    /// <returns>The API response.</returns>
    public static ApiResponse Error(string message = "Error", Exception? exception = null)
    {
        object? data = null;
        if (exception != null)
            data = new
            {
                exception.Message,
                exception.Data
            };

        return new ApiResponse
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Successful = false,
            Message = message,
            Data = data
        };
    }
}