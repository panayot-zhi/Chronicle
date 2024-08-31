// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Chronicle.Integration.Orleans.InProcess;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseCratisChronicle();
    }
}
