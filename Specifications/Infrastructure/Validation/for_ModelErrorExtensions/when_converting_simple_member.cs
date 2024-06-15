// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Applications.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cratis.Chronicle.Validation.for_ModelErrorExtensions;

public class when_converting_simple_member : Specification
{
    const string member = "TheMember";
    readonly ModelError model_error = new("Some message");
    ValidationResult validation_error;

    void Because() => validation_error = model_error.ToValidationResult(member);

    [Fact] void should_hold_message() => validation_error.Message.ShouldEqual(model_error.ErrorMessage);
    [Fact] void should_hold_camel_cased_member() => validation_error.Members.First().ShouldEqual("theMember");
}
