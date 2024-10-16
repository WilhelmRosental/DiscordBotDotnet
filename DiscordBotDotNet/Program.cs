using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using DiscordBotDotNet.Listeners;
using DiscordBotDotNet.Handlers;
using DiscordBotDotNet.Models;

//namespace DiscordBotDotNet;

//public class Program
//{
//    private DiscordSocketClient _client;
//    private InteractionService _interactionService;
//    private IServiceProvider _services;

//    public static async Task Main(string[] args) => await new Program().RunBotAsync();

//    public async Task RunBotAsync()
//    {
//        _client = new DiscordSocketClient(new DiscordSocketConfig
//        {
//            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions,
//            LogGatewayIntentWarnings = true
//        });

//        _services = ConfigureServices();

//        // Initialiser _interactionService après la création du client
//        _interactionService = new InteractionService(_client);

//        _client.Log += LogAsync;
//        _client.Ready += ReadyAsync;
//        _client.InteractionCreated += HandleInteraction;

//        string token = _services.GetRequiredService<IConfiguration>()["Discord:Token"] ?? throw new InvalidOperationException("Token manquant");
//        await _client.LoginAsync(TokenType.Bot, token);
//        await _client.StartAsync();

//        // Ajouter les modules après avoir initialisé l'interaction service
//        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

//        var guildMemberAddListener = _services.GetRequiredService<GuildMemberAddListener>();

//        await Task.Delay(-1);
//    }

//    private async Task ReadyAsync()
//    {
//        await _interactionService.RegisterCommandsGloballyAsync();
//        Console.WriteLine("Bot est prêt et les commandes sont enregistrées !");
//    }

//    private IServiceProvider ConfigureServices()
//    {
//        var configuration = new ConfigurationBuilder()
//            .SetBasePath(AppContext.BaseDirectory)
//            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//            .Build();

//        var mongoConnectionString = configuration["MongoDB:ConnectionString"];
//        var databaseName = configuration["MongoDB:DatabaseName"];

//        var mongoClient = new MongoClient(mongoConnectionString);
//        var database = mongoClient.GetDatabase(databaseName);

//        return new ServiceCollection()
//            .AddSingleton(_client)
//            .AddSingleton<QuizHandler>() // Enregistrement de QuizHandler
//            .AddSingleton(_interactionService)
//            .AddSingleton(database.GetCollection<User>("Users"))
//            .AddSingleton<IConfiguration>(configuration)
//            .AddSingleton<GuildMemberAddListener>()
//            .BuildServiceProvider();
//    }

//    private Task LogAsync(LogMessage log)
//    {
//        Console.WriteLine(log);
//        return Task.CompletedTask;
//    }

//    private async Task HandleInteraction(SocketInteraction interaction)
//    {
//        var ctx = new SocketInteractionContext(_client, interaction);
//        await _interactionService.ExecuteCommandAsync(ctx, _services);
//    }
//}

using DiscordBotDotNet.Handlers; // Assurez-vous que cet espace de noms est correct

public class Program
{
    private DiscordSocketClient _client;
    private InteractionService _interactionService;
    private IServiceProvider _services;

    public static async Task Main(string[] args) => await new Program().RunBotAsync();

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions,
            LogGatewayIntentWarnings = true
        });

        _services = ConfigureServices();

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.InteractionCreated += HandleInteraction;

        string token = _services.GetRequiredService<IConfiguration>()["Discord:Token"] ?? throw new InvalidOperationException("Token manquant");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        var guildMemberAddListener = _services.GetRequiredService<GuildMemberAddListener>();

        await Task.Delay(-1);
    }

    private IServiceProvider ConfigureServices()
    {
        // Initialisation du client DiscordSocketClient
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions,
            LogGatewayIntentWarnings = true
        });

        // Initialisation du service InteractionService
        _interactionService = new InteractionService(_client);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var mongoConnectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];

        var mongoClient = new MongoClient(mongoConnectionString);
        var database = mongoClient.GetDatabase(databaseName);

        return new ServiceCollection()
            .AddSingleton(_client) // S'assurer que _client n'est pas null ici
            .AddSingleton(_interactionService) // S'assurer que _interactionService n'est pas null ici
            .AddSingleton<QuizHandler>() // Enregistrement de QuizHandler
            .AddSingleton(database.GetCollection<User>("Users"))
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<GuildMemberAddListener>()
            .BuildServiceProvider();
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine("Bot is connected and ready!");
        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var ctx = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(ctx, _services);
    }
}
