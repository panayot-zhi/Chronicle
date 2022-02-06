// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Aksio.Cratis.Objects;
using Aksio.Cratis.Properties;

namespace Aksio.Cratis.Changes
{
    /// <summary>
    /// Represents an implementation of <see cref="IChangeset{TSource, TTarget}"/>.
    /// </summary>
    /// <typeparam name="TSource">Type of the source object we are working from.</typeparam>
    /// <typeparam name="TTarget">Type of target object we are applying changes to.</typeparam>
    public class Changeset<TSource, TTarget> : IChangeset<TSource, TTarget>
    {
        readonly List<Change> _changes = new();

        /// <inheritdoc/>
        public TSource Incoming { get; }

        /// <inheritdoc/>
        public TTarget InitialState { get; }

        /// <inheritdoc/>
        public IEnumerable<Change> Changes => _changes;

        /// <inheritdoc/>
        public bool HasChanges => _changes.Count > 0;

        /// <summary>
        /// Initializes a new instance of <see cref="Changeset{TSource, TTarget}"/>.
        /// </summary>
        /// <param name="incoming"><see cref="Incoming"/> that the <see cref="Changeset{TSource, TTarget}"/> is for.</param>
        /// <param name="initialState">The initial state before any changes are applied.</param>
        public Changeset(TSource incoming, TTarget initialState)
        {
            Incoming = incoming;
            InitialState = initialState;
        }

        /// <inheritdoc/>
        public void Add(Change change)
        {
            _changes.Add(change);
        }

        /// <inheritdoc/>
        public void SetProperties(IEnumerable<PropertyMapper<TSource, TTarget>> propertyMappers)
        {
            var workingState = InitialState.Clone()!;
            foreach (var propertyMapper in propertyMappers)
            {
                propertyMapper(Incoming, workingState);
            }

            var comparer = new ObjectsComparer.Comparer<TTarget>();
            if (!comparer.Compare(InitialState, workingState, out var differences))
            {
                Add(new PropertiesChanged<TTarget>(workingState, differences.Select(_ => new PropertyDifference<TTarget>(InitialState, workingState, _))));
            }
        }

        /// <inheritdoc/>
        public void SetChildProperties<TChild>(
            TChild item,
            PropertyPath childrenProperty,
            PropertyPath identifiedByProperty,
            ValueProvider<TSource> keyResolver,
            IEnumerable<PropertyMapper<TSource, TChild>> propertyMappers)
        {
            var workingItem = item.Clone()!;
            foreach (var propertyMapper in propertyMappers)
            {
                propertyMapper(Incoming, workingItem);
            }

            var comparer = new ObjectsComparer.Comparer();
            if (!comparer.Compare(item, workingItem, out var differences))
            {
                Add(new ChildPropertiesChanged<TChild>(
                    workingItem,
                    childrenProperty,
                    identifiedByProperty,
                    keyResolver(Incoming),
                    differences.Select(_ => new PropertyDifference<TChild>(item, workingItem, _))));
            }
        }

        /// <inheritdoc/>
        public void AddChild<TChild>(
            PropertyPath childrenProperty,
            PropertyPath identifiedByProperty,
            object key,
            IEnumerable<PropertyMapper<TSource, TChild>> propertyMappers,
            params ArrayIndexer[] arrayIndexers)
            where TChild : new()
        {
            var workingState = InitialState.Clone()!;
            var items = workingState.EnsureCollection<TTarget, TChild>(childrenProperty, arrayIndexers);

            if (!items.Contains(identifiedByProperty, key))
            {
                var item = new TChild();

                foreach (var propertyMapper in propertyMappers)
                {
                    propertyMapper(Incoming, item);
                }

                identifiedByProperty.SetValue(item, key);
                ((IList<TChild>)items).Add(item);

                Add(new ChildAdded(item, childrenProperty, identifiedByProperty, key!));
            }
        }

        /// <inheritdoc/>
        public void Remove()
        {
            Add(new Removed(InitialState.Clone()!));
        }

        /// <inheritdoc/>
        public void RemoveChild()
        {
        }

        /// <inheritdoc/>
        public bool HasChildBeenAddedWithKey(PropertyPath childrenProperty, object key)
        {
            return Changes
                    .Select(_ => _ as ChildAdded)
                    .Any(_ => _ != null && _.ChildrenProperty == childrenProperty && _.Key == key);
        }

        /// <inheritdoc/>
        public bool HasBeenRemoved() => Changes.Any(_ => _ is Removed);

        /// <inheritdoc/>
        public TChild GetChildByKey<TChild>(object key)
        {
            foreach (var change in _changes)
            {
                if (change is ChildAdded childAdded && childAdded.Key == key)
                {
                    return (TChild)childAdded.Child;
                }
            }

            return default!;
        }
    }
}
