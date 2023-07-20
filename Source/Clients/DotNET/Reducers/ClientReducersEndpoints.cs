// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using Aksio.Commands;
using Aksio.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Aksio.Cratis.Reducers;

/// <summary>
/// Represents the endpoints for the client reducers.
/// </summary>
public static class ClientReducersEndpoints
{
    /// <summary>
    /// Maps the endpoints for the client reducers.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to extend.</param>
    public static IEndpointRouteBuilder MapClientReducers(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/.cratis/reducers/{reducerId}", async (HttpContext context) =>
        {
            if (context.GetRouteValue("reducerId") is not string reducerIdAsString)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            ReducerId reducerId;
            try
            {
                reducerId = (ReducerId)reducerIdAsString;
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            var reduce = await context.Request.ReadFromJsonAsync<Reduce>(Globals.JsonSerializerOptions);
            if (reduce is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            CommandResult commandResult;
            var reducers = context.RequestServices.GetRequiredService<ClientReducers>();
            try
            {
                var result = await reducers.OnNext(reducerId, reduce.Events, reduce.InitialState);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                commandResult = new CommandResult { Response = result };
            }
            catch (Exception ex)
            {
                commandResult = new CommandResult
                {
                    ExceptionMessages = ex.GetAllMessages(),
                    ExceptionStackTrace = ex.StackTrace ?? string.Empty
                };
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            await context.Response.WriteAsJsonAsync(commandResult, Globals.JsonSerializerOptions);
        });

        return endpoints;
    }
}
