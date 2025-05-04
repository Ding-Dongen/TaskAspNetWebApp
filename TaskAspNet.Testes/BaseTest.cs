using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.Context;
using TaskAspNet.Data.Interfaces;
using TaskAspNet.Data.Repositories;

namespace TaskAspNet.Tests;

public abstract class BaseTest : IDisposable
{
    protected readonly TestDatabaseContext DbContext;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseTest()
    {
        var serviceCollection = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        serviceCollection.AddDbContext<TestDatabaseContext>(options =>
            options.UseInMemoryDatabase(dbName));
        serviceCollection.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        serviceCollection.AddScoped<IProjectRepository, ProjectRepository>();
        serviceCollection.AddScoped<IClientRepository, ClientRepository>();
        serviceCollection.AddScoped<IProjectStatusRepository, ProjectStatusRepository>();
        serviceCollection.AddScoped<IMemberRepository, MemberRepository>();

        serviceCollection.AddScoped<IUnitOfWork>(sp =>
        new FakeUnitOfWork(sp.GetRequiredService<AppDbContext>()));

        ServiceProvider = serviceCollection.BuildServiceProvider();

        DbContext = ServiceProvider.GetRequiredService<TestDatabaseContext>();
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}
