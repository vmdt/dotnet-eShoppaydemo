using System;

namespace BuildingBlocks.Core.EFCore;

public interface IDataSeeder
{
    Task SeedAllAsync();
}

public interface ITestDataSeeder
{
    Task SeedAllAsync();
}