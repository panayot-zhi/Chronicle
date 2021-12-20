// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using Cratis.Reflection;
using Cratis.Schemas;
using Cratis.Strings;
using Humanizer;

namespace Cratis.Events.Projections
{
    /// <summary>
    /// /// Represents an implementation of <see cref="IProjectionBuilderFor{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">Type of model.</typeparam>
    public class ProjectionBuilderFor<TModel> : IProjectionBuilderFor<TModel>
    {
        readonly ProjectionId _identifier;
        readonly IEventTypes _eventTypes;
        readonly IJsonSchemaGenerator _schemaGenerator;
        string _modelName;
        readonly Dictionary<string, FromDefinition> _fromDefintions = new();
        readonly Dictionary<string, ChildrenDefinition> _childrenDefinitions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionBuilderFor{TModel}"/> class.
        /// </summary>
        /// <param name="identifier">The unique identifier for the projection.</param>
        /// <param name="eventTypes"><see cref="IEventTypes"/> for providing event type information.</param>
        /// <param name="schemaGenerator"><see cref="IJsonSchemaGenerator"/> for generating JSON schemas.</param>
        public ProjectionBuilderFor(
            ProjectionId identifier,
            IEventTypes eventTypes,
            IJsonSchemaGenerator schemaGenerator)
        {
            _identifier = identifier;
            _eventTypes = eventTypes;
            _schemaGenerator = schemaGenerator;
            _modelName = typeof(TModel).Name.Pluralize().ToCamelCase();
        }

        /// <inheritdoc/>
        public IProjectionBuilderFor<TModel> ModelName(string modelName)
        {
            _modelName = modelName;
            return this;
        }

        /// <inheritdoc/>
        public IProjectionBuilderFor<TModel> From<TEvent>(Action<IFromBuilder<TModel, TEvent>> builderCallback)
        {
            var builder = new FromBuilder<TModel, TEvent>();
            builderCallback(builder);
            var eventType = _eventTypes.GetEventTypeIdFor(typeof(TEvent));
            _fromDefintions[eventType.ToString()] = builder.Build();
            return this;
        }

        /// <inheritdoc/>
        public IProjectionBuilderFor<TModel> Children<TChildModel>(Expression<Func<TModel, IEnumerable<TChildModel>>> targetProperty, Action<IChildrenBuilder<TModel, TChildModel>> builderCallback)
        {
            var builder = new ChildrenBuilder<TModel, TChildModel>(_eventTypes, _schemaGenerator);
            builderCallback(builder);
            _childrenDefinitions[targetProperty.GetPropertyInfo().Name.ToCamelCase()] = builder.Build();
            return this;
        }

        /// <inheritdoc/>
        public ProjectionDefinition Build()
        {
            return new ProjectionDefinition(
                _identifier,
                typeof(TModel).FullName ?? "[N/A]",
                new ModelDefinition(_modelName, _schemaGenerator.Generate(typeof(TModel)).ToJson()),
                _fromDefintions,
                _childrenDefinitions);
        }
    }
}
