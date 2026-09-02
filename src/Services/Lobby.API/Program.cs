using GameBackend.Services.Lobby.API.Application.Commands;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Application.Queries;
using GameBackend.Services.Lobby.API.Hubs;
using GameBackend.Services.Lobby.API.Infrastructure.Events;
using GameBackend.Services.Lobby.API.Infrastructure.Persistence;
using GameBackend.Services.Lobby.API.Infrastructure.Persistence.Repositories;
using GameBackend.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddDbContext<LobbyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LobbyDb"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

builder.Services.AddScoped<ILobbyRepository, LobbyRepository>();
builder.Services.AddScoped<IDomainEventDispatcher, SignalRDomainEventDispatcher>();

builder.Services.AddScoped<CreateLobbyCommandHandler>();
builder.Services.AddScoped<JoinLobbyCommandHandler>();
builder.Services.AddScoped<StartAuctionCommandHandler>();
builder.Services.AddScoped<PlaceBidCommandHandler>();
builder.Services.AddScoped<CompleteAuctionCommandHandler>();
builder.Services.AddScoped<GetOpenLobbiesQueryHandler>();
builder.Services.AddScoped<GetLobbyQueryHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<LobbyHub>("/hubs/lobby");

app.Run();