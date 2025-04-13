using Mediator;
using System.Diagnostics;

// https://github.com/martinothamar/Mediator?tab=readme-ov-file#42-add-mediator-to-di-container

var builder = WebApplication.CreateBuilder(args);

var services = new ServiceCollection(); // Most likely IServiceCollection comes from IHostBuilder/Generic host abstraction in Microsoft.Extensions.Hosting

services.AddMediator();

services.AddSingleton<IPipelineBehavior<Ping, Pong>, PingValidator>();
var serviceProvider = services.BuildServiceProvider();

var mediator = serviceProvider.GetRequiredService<IMediator>();
var ping = new Ping(Guid.NewGuid());
var pong = await mediator.Send(ping);
Debug.Assert(ping.Id == pong.Id);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}

public sealed class PingValidator : IPipelineBehavior<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, MessageHandlerDelegate<Ping, Pong> next, CancellationToken cancellationToken)
    {
        if (request is null || request.Id == default)
            throw new ArgumentException("Invalid input");

        return next(request, cancellationToken);
    }
}