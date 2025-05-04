
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

// Created from gpt4.5
// If the binding is valid then continue
// Get the request and check if it is an ajax request and what content type is requested
// If it is an ajax request then return a bad request with the model state errors
// Check if it is not an ajax request then return the view with the model state errors, only if the controller is a MVC controller
// If the controller is not a MVC controller then grab the model and return the view with the invalid model to render errors
// If it is not an MVC controller then return a bad request with the model state errors

public sealed class ValidateModel : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        if (ctx.ModelState.IsValid)      
            return;

        var req = ctx.HttpContext.Request;

        bool wantsJson =
            req.Headers["X-Requested-With"] == "XMLHttpRequest"      
            || (req.GetTypedHeaders().Accept ?? [])
                   .Any(h => h.MediaType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true);

        if (wantsJson)
        {
            ctx.Result = new BadRequestObjectResult(ctx.ModelState);
            return;
        }


        if (ctx.Controller is not Controller mvc)
        {
            ctx.Result = new BadRequestObjectResult(ctx.ModelState);
            return;
        }

        var model = ctx.ActionArguments.Values.FirstOrDefault();
        ctx.Result = mvc.View(model);
    }
}
