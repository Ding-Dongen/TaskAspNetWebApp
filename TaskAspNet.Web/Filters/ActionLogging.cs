using Microsoft.AspNetCore.Mvc.Filters;

public class ActionLogging : IAsyncActionFilter
{
    private readonly ILogger<ActionLogging> _log;
    public ActionLogging(ILogger<ActionLogging> log) => _log = log;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        _log.LogDebug(">> {Action} {RouteData}", ctx.ActionDescriptor.DisplayName,
                      ctx.RouteData.Values);

        var executed = await next();      

        if (executed.Exception is not null)
            _log.LogError(executed.Exception,
                          "<< {Action} threw", ctx.ActionDescriptor.DisplayName);
        else
            _log.LogDebug("<< {Action} completed", ctx.ActionDescriptor.DisplayName);
    }
}
