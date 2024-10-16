//using Discord;
//using Discord.Rest;
//using Discord.WebSocket;
//using DiscordBotDotNet.Models;
//using Microsoft.Extensions.Configuration;
//using DiscordBotDotNet.Handlers;
//using MongoDB.Driver;

//namespace DiscordBotDotNet.Listeners;

//public class GuildMemberAddListener
//{
//    private readonly DiscordSocketClient _client;
//    private readonly IMongoCollection<User> _userCollection;
//    private readonly IConfiguration _configuration;
//    private readonly QuizHandler _quizHandler;

//    private readonly ulong _categoryId = 1289308941242728476;

//    public GuildMemberAddListener(DiscordSocketClient client, IMongoCollection<User> userCollection, IConfiguration configuration)
//    {
//        _client = client;
//        _userCollection = userCollection;
//        _configuration = configuration;

//        _client.UserJoined += OnGuildMemberAddAsync;
//    }

//    private async Task OnGuildMemberAddAsync(SocketGuildUser member)
//    {
//        Console.WriteLine($"Un utilisateur a rejoint : {member.Username}");

//        try
//        {
//            // Récupération ou mise à jour des informations utilisateur dans MongoDB
//            var filter = Builders<User>.Filter.And(
//                Builders<User>.Filter.Eq("UserId", member.Id),
//                Builders<User>.Filter.Eq("GuildId", member.Guild.Id)
//            );

//            var userData = await _userCollection.Find(filter).FirstOrDefaultAsync();

//            if (userData == null)
//            {
//                userData = new User
//                {
//                    UserId = member.Id,
//                    GuildId = member.Guild.Id,
//                    Username = member.Username,
//                    LastActivity = DateTime.UtcNow
//                };
//                await _userCollection.InsertOneAsync(userData);
//            }
//            else
//            {
//                var update = Builders<User>.Update
//                    .Set(u => u.LastActivity, DateTime.UtcNow)
//                    .Set(u => u.Username, member.Username);

//                await _userCollection.UpdateOneAsync(filter, update);
//            }

//            // Appel de la méthode pour créer le channel privé
//            var privateChannel = await CreatePrivateChannelAsync(member.Guild, member, _categoryId);

//            if (privateChannel == null)
//            {
//                Console.WriteLine("Le channel privé n'a pas pu être récupéré via le cache.");
//            }

//            // Lancer la progression du questionnaire une fois que le channel privé a été créé
//            //await HandleQuizProgressAsync(privateChannel, member, userData);
//            await _quizHandler.HandleQuizProgress(privateChannel, member, userData);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("Erreur lors de l'exécution du guildMemberAdd: " + ex.Message);
//        }
//    }

//    private async Task<RestTextChannel?> CreatePrivateChannelAsync(SocketGuild guild, SocketGuildUser member, ulong categoryId)
//    {
//        var category = guild.GetCategoryChannel(categoryId);
//        if (category == null)
//        {
//            Console.WriteLine("La catégorie spécifiée n'existe pas.");
//            return null;
//        }

//        var restTextChannel = await guild.CreateTextChannelAsync(member.Username, x =>
//        {
//            x.CategoryId = categoryId;
//            x.PermissionOverwrites = new Overwrite[]
//            {
//            new Overwrite(member.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)),
//            new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny))
//            };
//        });

//        return restTextChannel;
//    }

//    private async Task HandleQuizProgressAsync(RestTextChannel? channel, SocketGuildUser member, User userData)
//    {
//        if (channel != null)
//            await channel.SendMessageAsync($"Bienvenue, {member.Username}! Commençons le questionnaire.");
//    }

//    private async Task HandleQuizProgressAsync(SocketTextChannel? channel, SocketGuildUser member, User userData)
//    {
//        if (channel != null)
//            await channel.SendMessageAsync($"Bienvenue, {member.Username}! Commençons le questionnaire.");
//    }
//}

using Discord;
using Discord.WebSocket;
using Discord.Rest;
using DiscordBotDotNet.Models;
using DiscordBotDotNet.Handlers; // Assurez-vous que cet espace de noms est correct
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DiscordBotDotNet.Listeners
{
    public class GuildMemberAddListener
    {
        private readonly DiscordSocketClient _client;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IConfiguration _configuration;
        private readonly QuizHandler _quizHandler; // Ajout de la variable QuizHandler

        private readonly ulong _categoryId = 1289308941242728476;

        public GuildMemberAddListener(DiscordSocketClient client, IMongoCollection<User> userCollection, IConfiguration configuration, QuizHandler quizHandler)
        {
            _client = client;
            _userCollection = userCollection;
            _configuration = configuration;
            _quizHandler = quizHandler; // Initialisation de QuizHandler

            _client.UserJoined += OnGuildMemberAddAsync;
        }

        private async Task OnGuildMemberAddAsync(SocketGuildUser member)
        {
            Console.WriteLine($"Un utilisateur a rejoint : {member.Username}");

            try
            {
                var filter = Builders<User>.Filter.And(
                    Builders<User>.Filter.Eq("UserId", member.Id),
                    Builders<User>.Filter.Eq("GuildId", member.Guild.Id)
                );

                var userData = await _userCollection.Find(filter).FirstOrDefaultAsync();

                if (userData == null)
                {
                    userData = new User
                    {
                        UserId = member.Id,
                        GuildId = member.Guild.Id,
                        Username = member.Username,
                        LastActivity = DateTime.UtcNow
                    };
                    await _userCollection.InsertOneAsync(userData);
                }
                else
                {
                    var update = Builders<User>.Update
                        .Set(u => u.LastActivity, DateTime.UtcNow)
                        .Set(u => u.Username, member.Username);

                    await _userCollection.UpdateOneAsync(filter, update);
                }

                var privateChannel = await CreatePrivateChannelAsync(member.Guild, member, _categoryId);

                if (privateChannel == null)
                {
                    Console.WriteLine("Le channel privé n'a pas pu être récupéré via le cache.");
                }

                // Appel de la méthode dans QuizHandler
                await _quizHandler.HandleQuizProgress(privateChannel, member, userData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'exécution du guildMemberAdd: " + ex.Message);
            }
        }

        //private async Task<SocketTextChannel?> CreatePrivateChannelAsync(SocketGuild guild, SocketGuildUser member, ulong categoryId)
        //{
        //    var category = guild.GetCategoryChannel(categoryId);
        //    if (category == null)
        //    {
        //        Console.WriteLine("La catégorie spécifiée n'existe pas.");
        //        return null;
        //    }

        //    var restTextChannel = await guild.CreateTextChannelAsync(member.Username, x =>
        //    {
        //        x.CategoryId = categoryId;
        //        x.PermissionOverwrites = new Overwrite[]
        //        {
        //            new Overwrite(member.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)),
        //            new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny))
        //        };
        //    });

        //    Console.WriteLine($"Channel ID créé : {restTextChannel.Id}");

        //    var privateChannel = guild.GetTextChannel(restTextChannel.Id);

        //    // Utilisez directement le RestTextChannel si le channel n'est pas récupéré
        //    return privateChannel ?? restTextChannel as SocketTextChannel;
        //}
        private async Task<SocketTextChannel?> CreatePrivateChannelAsync(SocketGuild guild, SocketGuildUser member, ulong categoryId)
        {
            var category = guild.GetCategoryChannel(categoryId);
            if (category == null)
            {
                Console.WriteLine("La catégorie spécifiée n'existe pas.");
                return null;
            }

            var restTextChannel = await guild.CreateTextChannelAsync(member.Username, x =>
            {
                x.CategoryId = categoryId;
                x.PermissionOverwrites = new Overwrite[]
                {
            new Overwrite(member.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)),
            new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny))
                };
            });

            Console.WriteLine($"Channel ID créé : {restTextChannel.Id}");

            // Récupération du channel via le cache
            var privateChannel = guild.GetTextChannel(restTextChannel.Id);

            if (privateChannel == null)
            {
                Console.WriteLine("Le channel privé n'a pas pu être récupéré via le cache.");
            }

            return privateChannel; // Retourne le channel depuis le cache
        }

    }
}
