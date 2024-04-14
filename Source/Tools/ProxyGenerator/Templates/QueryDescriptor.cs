// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.ProxyGenerator.Templates;

/// <summary>
/// Describes a query for templating purposes.
/// </summary>
/// <param name="Controller">The controller type that owns the query.</param>
/// <param name="Method">The method that represents the query.</param>
/// <param name="Route">API route for the query.</param>
/// <param name="Name">Name of the query.</param>
/// <param name="Model">Type of model the query is for.</param>
/// <param name="Constructor">The JavaScript constructor for the model type.</param>
/// <param name="IsEnumerable">Whether or not the result is an enumerable or not.</param>
/// <param name="Imports">Additional import statements.</param>
/// <param name="Arguments">Arguments for the query.</param>
public record QueryDescriptor(
    Type Controller,
    MethodInfo Method,
    string Route,
    string Name,
    string Model,
    string Constructor,
    bool IsEnumerable,
    IEnumerable<ImportStatement> Imports,
    IEnumerable<RequestArgumentDescriptor> Arguments);
