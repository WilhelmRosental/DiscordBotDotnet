using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotDotNet.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DiscordBotDotNet.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IConfiguration _configuration;

    public InteractionHandler(DiscordSocketClient client, IMongoCollection<User> userCollection, IConfiguration configuration, InteractionService interactionService, IServiceProvider services)
    {
        _client = client;
        _userCollection = userCollection;
        _configuration = configuration;
        _interactionService = interactionService;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        // Charger tous les modules d'interactions
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        _client.InteractionCreated += async (interaction) =>
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        };

        // S'abonner à l'événement InteractionCreated
        _client.InteractionCreated += HandleInteractionAsync;
    }

    // La méthode pour traiter les interactions des composants (boutons, etc.)
    public async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (interaction is SocketMessageComponent component)
        {
            // Récupérer l'utilisateur à partir de MongoDB
            var userData = await _userCollection.Find(u => u.UserId == component.User.Id).FirstOrDefaultAsync();

            if (userData != null)
            {
                // Récupérer les rôles depuis appsettings.json
                var maleRoleId = ulong.Parse(_configuration["Server:Roles:Genders:Male"] ?? throw new InvalidOperationException());
                var femaleRoleId = ulong.Parse(_configuration["Server:Roles:Genders:Female"] ?? throw new InvalidOperationException());

                if (component.Data.CustomId == "male")
                {
                    userData.Gender = "male";
                }
                else if (component.Data.CustomId == "female")
                {
                    userData.Gender = "female";
                }

                // Mettre à jour MongoDB avec les nouvelles données
                await _userCollection.ReplaceOneAsync(u => u.UserId == component.User.Id, userData);

                // Répondre à l'utilisateur
                await component.RespondAsync($"Vous avez sélectionné {userData.Gender}.", ephemeral: true);

                // Modifier le message pour retirer les boutons
                await component.Message.ModifyAsync(m => m.Components = new ComponentBuilder().Build());
            }
        }
    }
}