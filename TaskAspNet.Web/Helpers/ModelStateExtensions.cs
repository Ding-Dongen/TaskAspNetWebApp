// Not in use, still working on it

using Microsoft.AspNetCore.Mvc.ModelBinding;


public static class ModelStateExtensions
{
    public static Dictionary<string, string[]> GetErrors(this ModelStateDictionary modelState)
    {
        return modelState
            .Where(x => x.Value.Errors.Any())
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }
}