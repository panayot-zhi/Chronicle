// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Integration.for_Adapters;

public class when_getting_mapping_for_unknown_model_and_external_model : given.no_adapters
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => _adapters.GetProjectionFor<string, object>());

    [Fact] void should_throw_missing_adapter_for_model_and_external_model() => _result.ShouldBeOfExactType<MissingAdapterForModelAndExternalModel>();
}
