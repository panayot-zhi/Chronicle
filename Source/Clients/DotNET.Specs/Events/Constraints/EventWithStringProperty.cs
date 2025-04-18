// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Events.Constraints;

public record EventWithStringProperty
{
    public string SomeProperty { get; init; } = string.Empty;
    public string SomeOtherProperty { get; init; } = string.Empty;
}
