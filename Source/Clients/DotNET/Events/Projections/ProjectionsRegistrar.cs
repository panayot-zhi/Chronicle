// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Aksio.Cratis.Events.Projections.Definitions;
using Aksio.Cratis.Execution;
using Aksio.Cratis.Reflection;
using Aksio.Cratis.Schemas;
using Aksio.Cratis.Types;
using Orleans;

namespace Aksio.Cratis.Events.Projections
{
    /// <summary>
    /// Represents an implementation of <see cref="IProjectionsRegistrar"/>.
    /// </summary>
    public class ProjectionsRegistrar : IProjectionsRegistrar
    {
        static class ProjectionDefinitionCreator<TModel>
        {
            public static ProjectionDefinition CreateAndDefine(Type type, IEventTypes eventTypes, IJsonSchemaGenerator schemaGenerator)
            {
                var instance = (Activator.CreateInstance(type) as IProjectionFor<TModel>)!;
                var builder = new ProjectionBuilderFor<TModel>(instance.Identifier, eventTypes, schemaGenerator);
                instance.Define(builder);
                return builder.Build();
            }
        }

        readonly IEnumerable<ProjectionDefinition> _projections;
        readonly IClusterClient _clusterClient;
        readonly IExecutionContextManager _executionContextManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Projections"/> class.
        /// </summary>
        /// <param name="clusterClient">Orleans <see cref="IClusterClient"/>.</param>
        /// <param name="executionContextManager"><see cref="IExecutionContextManager"/> for establishing execution context.</param>
        /// <param name="eventTypes"><see cref="IEventTypes"/> to use.</param>
        /// <param name="types"><see cref="ITypes"/> for type discovery.</param>
        /// <param name="schemaGenerator"><see cref="IJsonSchemaGenerator"/> for generating JSON schemas.</param>
        public ProjectionsRegistrar(
            IClusterClient clusterClient,
            IExecutionContextManager executionContextManager,
            IEventTypes eventTypes,
            ITypes types,
            IJsonSchemaGenerator schemaGenerator)
        {
            _projections = FindAllProjectionDefinitions(eventTypes, types, schemaGenerator);
            _clusterClient = clusterClient;
            _executionContextManager = executionContextManager;
        }

        /// <summary>
        /// Find all projection definitions.
        /// </summary>
        /// <param name="eventTypes"><see cref="IEventTypes"/> to use.</param>
        /// <param name="types"><see cref="ITypes"/> to find from.</param>
        /// <param name="schemaGenerator"><see cref="IJsonSchemaGenerator"/> for generating the schema for the model.</param>
        /// <returns>Collection of <see cref="ProjectionDefinition"/>.</returns>
        public static IEnumerable<ProjectionDefinition> FindAllProjectionDefinitions(IEventTypes eventTypes, ITypes types, IJsonSchemaGenerator schemaGenerator) =>
            types.All
                    .Where(_ => _.HasInterface(typeof(IProjectionFor<>)))
                    .Select(_ =>
                    {
                        var modelType = _.GetInterface(typeof(IProjectionFor<>).Name)!.GetGenericArguments()[0]!;
                        var creatorType = typeof(ProjectionDefinitionCreator<>).MakeGenericType(modelType);
                        var method = creatorType.GetMethod(nameof(ProjectionDefinitionCreator<object>.CreateAndDefine), BindingFlags.Public | BindingFlags.Static)!;
                        return (method.Invoke(null, new object[] { _, eventTypes, schemaGenerator }) as ProjectionDefinition)!;
                    }).ToArray();

        /// <inheritdoc/>
        public async Task StartAll()
        {
            // TODO: Observe for all tenants
            _executionContextManager.Establish("3352d47d-c154-4457-b3fb-8a2efb725113", CorrelationId.New());

            var projections = _clusterClient.GetGrain<Grains.IProjections>(Guid.Empty);
            foreach (var projectionDefinition in _projections)
            {
                var pipelineDefinition = new ProjectionPipelineDefinition(
                    projectionDefinition.Identifier,
                    "c0c0196f-57e3-4860-9e3b-9823cf45df30", // Cratis default
                    new[]
                    {
                        new ProjectionSinkDefinition(
                            "12358239-a120-4392-96d4-2b48271b904c",
                            "22202c41-2be1-4547-9c00-f0b1f797fd75") // MongoDB
                    });
                await projections.Register(projectionDefinition, pipelineDefinition);
            }
        }
    }
}
