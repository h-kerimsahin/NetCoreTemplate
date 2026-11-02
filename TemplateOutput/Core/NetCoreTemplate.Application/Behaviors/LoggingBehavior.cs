using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace NetCoreTemplate.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        try
        {
            _logger.LogInformation("Handling {RequestName} with request: {Request}", requestName, JsonSerializer.Serialize(request));
            var response = await next();
            _logger.LogInformation("Handled {RequestName} successfully", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {RequestName}", requestName);
            throw;
        }
    }
}
