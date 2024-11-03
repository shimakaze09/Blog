using Data.Models;
using FreeSql;
using Contrib.SiteMessage;

namespace Web.Middlewares;

public class VisitRecordMiddleware
{
    private readonly RequestDelegate _next;

    public VisitRecordMiddleware(RequestDelegate requestDelegate)
    {
        _next = requestDelegate;
    }

    public Task Invoke(HttpContext context, IBaseRepository<VisitRecord> visitRecordRepo)
    {
        var request = context.Request;
        var response = context.Response;

        visitRecordRepo.InsertAsync(new VisitRecord
        {
            Ip = "",
            RequestPath = request.Path,
            RequestQueryString = request.QueryString.Value,
            RequestMethod = request.Method,
            UserAgent = request.Headers.UserAgent,
            Time = DateTime.Now
        });

        return _next(context);
    }
}