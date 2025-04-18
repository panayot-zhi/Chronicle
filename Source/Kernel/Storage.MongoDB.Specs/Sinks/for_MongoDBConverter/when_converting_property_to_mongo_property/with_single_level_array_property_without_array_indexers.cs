// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Properties;

namespace Cratis.Chronicle.Storage.MongoDB.Sinks.for_MongoDBConverter.when_converting_property_to_mongo_property;

public class with_single_level_array_property_without_array_indexers : given.a_mongodb_converter
{
    MongoDBProperty result;

    void Because() => result = _converter.ToMongoDBProperty(new PropertyPath("[ArrayProperty]"), ArrayIndexers.NoIndexers);

    [Fact] void should_have_the_correct_property_name() => result.Property.ShouldEqual("arrayProperty");
    [Fact] void should_not_have_any_array_filters() => result.ArrayFilters.ShouldBeEmpty();
}
