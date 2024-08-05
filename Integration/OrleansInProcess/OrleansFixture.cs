// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Integration.Base;
using Cratis.Chronicle.Setup;
using Cratis.Json;
using DotNet.Testcontainers.Networks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cratis.Chronicle.Integration.OrleansInProcess;

public class OrleansFixture : WebApplicationFactory<Startup>
{
    string _name = string.Empty;

    public OrleansFixture(GlobalFixture globalFixture)
    {
        GlobalFixture = globalFixture;
    }

    protected override IWebHostBuilder CreateWebHostBuilder()
    {
        var builder = WebApplication.CreateBuilder();

        builder.UseCratisMongoDB(
            mongo =>
            {
                mongo.Server = "mongodb://localhost:27018";
                mongo.Database = "testing";
            });

        builder.Host.UseDefaultServiceProvider(_ => _.ValidateOnBuild = false);
        builder.Services.AddSingleton(Globals.JsonSerializerOptions);
        builder.UseCratisChronicle();

        builder.UseOrleans(silo =>
            {
                silo
                    .UseLocalhostClustering()
                    .AddCratisChronicle(
                        options => options.EventStoreName = "testing",
                        _ => _.WithMongoDB());
            });

        return builder.WebHost;
    }

    public INetwork Network => GlobalFixture.Network;
    public MongoDBDatabase EventStore => GlobalFixture.EventStore;
    public MongoDBDatabase EventStoreForNamespace => GlobalFixture.EventStore;
    public MongoDBDatabase ReadModels => GlobalFixture.ReadModels;
    public GlobalFixture GlobalFixture { get; }

    public void SetName(string name) => _name = name;

    protected override void Dispose(bool disposing)
    {
        GlobalFixture.PerformBackup(_name);
        GlobalFixture.RemoveAllDatabases().GetAwaiter().GetResult();
        base.Dispose(disposing);
    }
}
