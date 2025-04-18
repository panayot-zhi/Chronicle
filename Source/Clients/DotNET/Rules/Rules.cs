// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using Cratis.Chronicle.Models;
using Cratis.Chronicle.Projections;
using Cratis.Strings;

namespace Cratis.Chronicle.Rules;

/// <summary>
/// Represents an implementation of <see cref="IRules"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Rules"/> class.
/// </remarks>
/// <param name="serializerOptions"><see cref="JsonSerializerOptions"/> to use for deserialization.</param>
/// <param name="projections"><see cref="IProjections"/> client.</param>
/// <param name="clientArtifacts">Optional <see cref="IClientArtifactsProvider"/> for the client artifacts.</param>
[Singleton]
public class Rules(
    JsonSerializerOptions serializerOptions,
    IProjections projections,
    IClientArtifactsProvider clientArtifacts) : IRules
{
    readonly Dictionary<Type, IEnumerable<Type>> _rulesPerCommand = clientArtifacts.Rules
            .GroupBy(_ => _.BaseType!.GetGenericArguments()[1])
            .ToDictionary(_ => _.Key, _ => _.ToArray().AsEnumerable());

    /// <inheritdoc/>
    public bool HasFor(Type type) => _rulesPerCommand.ContainsKey(type);

    /// <inheritdoc/>
    public IEnumerable<Type> GetFor(Type type) => HasFor(type) ? _rulesPerCommand[type] : [];

    /// <inheritdoc/>
    public void ProjectTo(IRule rule, object? modelIdentifier = default)
    {
        var identifier = rule.GetType().GetRuleId();
        if (!projections.HasFor(identifier.Value)) return;

        var result = projections.GetInstanceById(
            identifier.Value,
            modelIdentifier is null ? ModelKey.Unspecified : modelIdentifier.ToString()!).GetAwaiter().GetResult();

        var properties = rule.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty);
        properties = properties.Where(_ => _.CanWrite).ToArray();
        foreach (var property in properties)
        {
            var name = property.Name.ToCamelCase();
            var node = result.Model[name];
            if (node is not null)
            {
                property.SetValue(rule, node.Deserialize(property.PropertyType, serializerOptions));
            }
        }
    }
}
