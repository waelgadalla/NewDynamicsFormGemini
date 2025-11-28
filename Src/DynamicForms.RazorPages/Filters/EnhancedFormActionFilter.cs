using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DynamicForms.RazorPages.Filters
{
    /// <summary>
    /// Action filter to handle enhanced form operations with proper error handling and logging
    /// </summary>
    public class EnhancedFormActionFilter : ActionFilterAttribute
    {
        private readonly ILogger<EnhancedFormActionFilter> _logger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public EnhancedFormActionFilter(ILogger<EnhancedFormActionFilter> logger)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            _logger = logger;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override void OnActionExecuting(ActionExecutingContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var moduleId = context.ActionArguments.TryGetValue("moduleId", out var modId) ? modId : null;
            var opportunityId = context.ActionArguments.TryGetValue("opportunityId", out var oppId) ? oppId : null;
            
            _logger.LogDebug("Enhanced form action executing for module {ModuleId}, opportunity {OpportunityId}", 
                moduleId, opportunityId);

            base.OnActionExecuting(context);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override void OnActionExecuted(ActionExecutedContext context)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (context.Exception != null)
            {
                _logger.LogError(context.Exception, "Enhanced form action failed");
                
                if (context.HttpContext.Request.Headers.ContainsKey("X-Requested-With"))
                {
                    // AJAX request
                    context.Result = new JsonResult(new { success = false, error = "An error occurred" })
                    {
                        StatusCode = 500
                    };
                    context.ExceptionHandled = true;
                }
            }

            base.OnActionExecuted(context);
        }
    }
}