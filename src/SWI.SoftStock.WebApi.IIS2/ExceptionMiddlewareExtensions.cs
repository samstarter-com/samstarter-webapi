using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace SWI.SoftStock.WebApi.IIS2
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger log)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature?.Error is SecurityTokenException)
                    {
                        log.LogInformation($"Request: {context.Request.GetDisplayUrl()} Something went wrong: {contextFeature.Error}");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync(new { context.Response.StatusCode, contextFeature.Error.Message }.ToString());
                        return;
                    }
                    if (contextFeature != null)
                    {
                        log.LogError($"Request: {context.Request.GetDisplayUrl()} Something went wrong: {contextFeature.Error}");

                        await context.Response.WriteAsync(new
                        {
                            context.Response.StatusCode,
                            Message = "Something went wrong"
                        }.ToString());
                    }
                });
            });
        }
    }
}