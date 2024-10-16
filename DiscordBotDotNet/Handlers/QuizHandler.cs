//using System;
//using System.Threading.Tasks;
//using Discord;
//using Discord.Interactions;
//using Discord.WebSocket;
//using Microsoft.Extensions.Configuration;
//using MongoDB.Driver;

//public class QuizHandler
//{
//    private readonly IMongoCollection<User> _userCollection;

//    public QuizHandler(IMongoCollection<User> userCollection)
//    {
//        _userCollection = userCollection;
//    }

//    public async Task HandleQuizProgress(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        int progress = userData.QuestionnaireProgress ?? 0;

//        switch (progress)
//        {
//            case 0:
//                await AskForGender(channel, member, userData);
//                break;
//            case 1:
//                await AskForBirthday(channel, member, userData);
//                break;
//            case 2:
//                await AskPersonalityQuestion1(channel, member, userData);
//                break;
//            case 3:
//                await AskPersonalityQuestion2(channel, member, userData);
//                break;
//            case 4:
//                await AskPersonalityQuestion3(channel, member, userData);
//                break;
//            case 5:
//                await AskPersonalityQuestion4(channel, member, userData);
//                break;
//            case 6:
//                await DetermineHouse(channel, member, userData);
//                break;
//            default:
//                await channel.SendMessageAsync("Vous avez déjà complété le questionnaire.");
//                break;
//        }
//    }

//    private async Task AskForGender(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        var builder = new ComponentBuilder()
//            .WithButton("Homme", "male", ButtonStyle.Primary)
//            .WithButton("Femme", "female", ButtonStyle.Primary);

//        var message = await channel.SendMessageAsync($"Bienvenue sur le serveur, {member.DisplayName}! Quel est votre sexe ?", components: builder.Build());

//        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection);
//        await interactionCollector.CollectGenderResponse(userData);
//    }

//    private async Task AskForBirthday(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        var message = await channel.SendMessageAsync("Merci de fournir votre date de naissance au format JJ/MM/AAAA :");

//        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection);
//        await interactionCollector.CollectBirthdayResponse(userData);
//    }

//    private async Task AskPersonalityQuestion1(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        var embed = new EmbedBuilder()
//            .WithTitle("Question 1 : Extraversion vs. Introversion")
//            .WithDescription("Comment préférez-vous recharger vos batteries après une longue journée ?\n" +
//                             "**A)** En passant du temps avec des amis (E)\n" +
//                             "**B)** En vous retirant pour réfléchir ou vous détendre seul(e) (I)")
//            .WithColor(Color.Blue)
//            .Build();

//        var builder = new ComponentBuilder()
//            .WithButton("Option A (E)", "E", ButtonStyle.Primary)
//            .WithButton("Option B (I)", "I", ButtonStyle.Primary);

//        var message = await channel.SendMessageAsync(embed: embed, components: builder.Build());

//        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection);
//        await interactionCollector.CollectPersonalityResponse(userData, "E", "I", 3);
//    }

//    private async Task AskPersonalityQuestion2(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        var embed = new EmbedBuilder()
//            .WithTitle("Question 2 : Sensation (S) vs. Intuition (N)")
//            .WithDescription("Comment préférez-vous recevoir de nouvelles informations ?\n" +
//                             "**A)** Je me concentre sur les faits et les détails pratiques (S)\n" +
//                             "**B)** J'aime envisager les possibilités et les idées abstraites (N)")
//            .WithColor(Color.Blue)
//            .Build();

//        var builder = new ComponentBuilder()
//            .WithButton("Option A (S)", "S", ButtonStyle.Primary)
//            .WithButton("Option B (N)", "N", ButtonStyle.Primary);

//        var message = await channel.SendMessageAsync(embed: embed, components: builder.Build());

//        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection);
//        await interactionCollector.CollectPersonalityResponse(userData, "S", "N", 4);
//    }

//    private async Task AskPersonalityQuestion3(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        var embed = new EmbedBuilder()
//            .WithTitle("Question 3 : Pensée (T) vs. Sentiment (F)")
//            .WithDescription("Comment prenez-vous généralement vos décisions ?\n" +
//                             "**A)** Basé(e) sur la logique et l'analyse (T)\n" +
//                             "**B)** Basé(e) sur les émotions et l'harmonie des relations (F)")
//            .WithColor(Color.Blue)
//            .Build();

//        var builder = new ComponentBuilder()
//            .WithButton("Option A (T)", "T", ButtonStyle.Primary)
//            .WithButton("Option B (F)", "F", ButtonStyle.Primary);

//        var message = await channel.SendMessageAsync(embed: embed, components: builder.Build());

//        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection);
//        await interactionCollector.CollectPersonalityResponse(userData, "T", "F", 5);
//    }

//    private async Task AskPersonalityQuestion4(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        var embed = new EmbedBuilder()
//            .WithTitle("Question 4 : Jugement (J) vs. Perception (P)")
//            .WithDescription("Comment préférez-vous organiser votre vie quotidienne ?\n" +
//                             "**A)** J'aime planifier et organiser à l'avance (J)\n" +
//                             "**B)** Je préfère être flexible et spontané(e) (P)")
//            .WithColor(Color.Blue)
//            .Build();

//        var builder = new ComponentBuilder()
//            .WithButton("Option A (J)", "J", ButtonStyle.Primary)
//            .WithButton("Option B (P)", "P", ButtonStyle.Primary);

//        var message = await channel.SendMessageAsync(embed: embed, components: builder.Build());

//        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection);
//        await interactionCollector.CollectPersonalityResponse(userData, "J", "P", 6);
//    }

//    private async Task DetermineHouse(SocketTextChannel channel, SocketGuildUser member, User userData)
//    {
//        string houseName = GetPersonalityTypeHouse(userData.PersonalityType);

//        if (string.IsNullOrEmpty(houseName))
//        {
//            await channel.SendMessageAsync($"Désolé, votre type de personnalité \"{userData.PersonalityType}\" ne correspond à aucune maison.");
//            return;
//        }

//        //userData.HouseRoleId = houses[houseName.ToLower()];  // Les IDs des maisons
//        //userData.House = houseName;

//        await channel.SendMessageAsync($"Votre type de personnalité est **{userData.PersonalityType}**, vous appartenez à la maison **{userData.House}**.");

//        bool success = await AssignRoles(member, userData);
//        if (success)
//        {
//            await channel.SendMessageAsync("Tous les rôles ont été attribués avec succès.");
//        }
//        else
//        {
//            await channel.SendMessageAsync("Une erreur s'est produite lors de l'attribution des rôles.");
//        }
//    }

//    private string GetPersonalityTypeHouse(string personalityType)
//    {
//        var personalityToHouse = new Dictionary<string, string>
//        {
//            { "ESTJ", "Nebula" },
//            { "ESTP", "Galaxia" },
//            { "ESFJ", "Lunaris" },
//            { "ESFP", "Galaxia" },
//            { "ENTJ", "Nebula" },
//            { "ENTP", "Stellaris" },
//            { "ENFJ", "Galaxia" },
//            { "ENFP", "Galaxia" },
//            { "ISTJ", "Lunaris" },
//            { "ISTP", "Nebula" },
//            { "ISFJ", "Lunaris" },
//            { "ISFP", "Lunaris" },
//            { "INTJ", "Nebula" },
//            { "INTP", "Stellaris" },
//            { "INFJ", "Stellaris" },
//            { "INFP", "Stellaris" }
//        };

//        return personalityToHouse.ContainsKey(personalityType) ? personalityToHouse[personalityType] : null;
//    }

//    private async Task<bool> AssignRoles(SocketGuildUser member, User userData)
//    {
//        try
//        {
//            //if (userData.GenderRoleId != 0)
//            //{
//            //    await member.AddRoleAsync(userData.GenderRoleId);
//            //}

//            //if (userData.AgeRoleId != 0)
//            //{
//            //    await member.AddRoleAsync(userData.AgeRoleId);
//            //}

//            //if (userData.HouseRoleId != 0)
//            //{
//            //    await member.AddRoleAsync(userData.HouseRoleId);
//            //}

//            return true;
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Erreur lors de l'attribution des rôles: {ex.Message}");
//            return false;
//        }
//    }
//}

using Discord;
using Discord.WebSocket;
using DiscordBotDotNet.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DiscordBotDotNet.Handlers;

public class QuizHandler
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;

    public QuizHandler(IMongoCollection<User> userCollection, IConfiguration configuration, DiscordSocketClient client)
    {
        _userCollection = userCollection;
        _configuration = configuration;
        _client = client;
    }

    public async Task HandleQuizProgress(SocketTextChannel channel, SocketGuildUser member, User userData)
    {
        int progress = userData.QuestionnaireProgress ?? 0;
        
        switch (progress)
        {
            case 0:
                await AskForGender(channel, member, userData);
                break;
            case 1:
                await AskForBirthday(channel, member, userData);
                break;
            case 2:
                await AskPersonalityQuestion1(channel, member, userData);
                break;
            case 3:
                await AskPersonalityQuestion3(channel, member, userData);
                break;
            case 4:
                await AskPersonalityQuestion4(channel, member, userData);
                break;
            case 5:
                await DetermineHouse(channel, member, userData);
                break;
            default:
                await channel.SendMessageAsync("Vous avez déjà complété le questionnaire.");
                break;
        }
    }

    private async Task AskForGender(SocketTextChannel channel, SocketGuildUser member, User userData)
    {
        var builder = new ComponentBuilder()
            .WithButton("Homme", "male", ButtonStyle.Primary)
            .WithButton("Femme", "female", ButtonStyle.Primary);

        var message = await channel.SendMessageAsync($"Bienvenue sur le serveur, {member.DisplayName}! Quel est votre sexe ?", components: builder.Build());

        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection, _configuration, _client);
        await interactionCollector.CollectGenderResponse(userData);
    }

    private async Task AskForBirthday(SocketTextChannel channel, SocketGuildUser member, User userData)
    {
        var message = await channel.SendMessageAsync("Merci de fournir votre date de naissance au format JJ/MM/AAAA :");

        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection, _configuration, _client);
        await interactionCollector.CollectBirthdayResponse(userData);
    }

    private async Task AskPersonalityQuestion1(SocketTextChannel channel, SocketGuildUser member, User userData)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Question 1 : Extraversion vs. Introversion")
            .WithDescription("Comment préférez-vous recharger vos batteries après une longue journée ?\n" +
                             "**A)** En passant du temps avec des amis (E)\n" +
                             "**B)** En vous retirant pour réfléchir ou vous détendre seul(e) (I)")
            .WithColor(Color.Blue)
            .Build();

        var builder = new ComponentBuilder()
            .WithButton("Option A (E)", "E", ButtonStyle.Primary)
            .WithButton("Option B (I)", "I", ButtonStyle.Primary);

        var message = await channel.SendMessageAsync(embed: embed, components: builder.Build());

        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection, _configuration, _client);
        await interactionCollector.CollectPersonalityResponse(userData, "E", "I", 3);
    }

    private async Task AskPersonalityQuestion3(SocketTextChannel channel, SocketGuildUser member, User userData)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Question 3 : Pensée (T) vs. Sentiment (F)")
            .WithDescription("Comment prenez-vous généralement vos décisions ?\n" +
                             "**A)** Basé(e) sur la logique et l'analyse (T)\n" +
                             "**B)** Basé(e) sur les émotions et l'harmonie des relations (F)")
            .WithColor(Color.Blue)
            .Build();

        var builder = new ComponentBuilder()
            .WithButton("Option A (T)", "T", ButtonStyle.Primary)
            .WithButton("Option B (F)", "F", ButtonStyle.Primary);

        var message = await channel.SendMessageAsync(embed: embed, components: builder.Build());

        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection, _configuration, _client);
        await interactionCollector.CollectPersonalityResponse(userData, "T", "F", 4);
    }

    private async Task AskPersonalityQuestion4(SocketTextChannel channel, SocketGuildUser member, User userData)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Question 4 : Jugement (J) vs. Perception (P)")
            .WithDescription("Comment préférez-vous organiser votre vie quotidienne ?\n" +
                             "**A)** J'aime planifier et organiser à l'avance (J)\n" +
                             "**B)** Je préfère être flexible et spontané(e) (P)")
            .WithColor(Color.Blue)
            .Build();

        var builder = new ComponentBuilder()
            .WithButton("Option A (J)", "J", ButtonStyle.Primary)
            .WithButton("Option B (P)", "P", ButtonStyle.Primary);

        var message = await channel.SendMessageAsync(embed: embed, components: builder.Build());

        var interactionCollector = new InteractionCollector(message, channel, member, _userCollection, _configuration, _client);
        await interactionCollector.CollectPersonalityResponse(userData, "J", "P", 5);
    }

    private async Task DetermineHouse(SocketTextChannel channel, SocketGuildUser member, User userData)
    {
        if (userData.PersonalityType != null)
        {
            string? houseName = GetPersonalityTypeHouse(userData.PersonalityType);

            if (string.IsNullOrEmpty(houseName))
            {
                await channel.SendMessageAsync($"Désolé, votre type de personnalité \"{userData.PersonalityType}\" ne correspond à aucune maison.");
                return;
            }
        }

        await channel.SendMessageAsync($"Votre type de personnalité est **{userData.PersonalityType}**, vous appartenez à la maison **{userData.House}**.");

        bool success = await AssignRoles(member, userData);
        if (success)
        {
            await channel.SendMessageAsync("Tous les rôles ont été attribués avec succès.");
        }
        else
        {
            await channel.SendMessageAsync("Une erreur s'est produite lors de l'attribution des rôles.");
        }
    }

    private string? GetPersonalityTypeHouse(string personalityType)
    {
        var personalityToHouse = new Dictionary<string, string?>
        {
            { "ESTJ", "Nebula" },
            { "ESTP", "Galaxia" },
            { "ESFJ", "Lunaris" },
            { "ESFP", "Galaxia" },
            { "ENTJ", "Nebula" },
            { "ENTP", "Stellaris" },
            { "ENFJ", "Galaxia" },
            { "ENFP", "Galaxia" },
            { "ISTJ", "Lunaris" },
            { "ISTP", "Nebula" },
            { "ISFJ", "Lunaris" },
            { "ISFP", "Lunaris" },
            { "INTJ", "Nebula" },
            { "INTP", "Stellaris" },
            { "INFJ", "Stellaris" },
            { "INFP", "Stellaris" }
        };

        return personalityToHouse.ContainsKey(personalityType) ? personalityToHouse[personalityType] : null;
    }

    private Task<bool> AssignRoles(SocketGuildUser member, User userData)
    {
        try
        {
            // Assign roles logic
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'attribution des rôles: {ex.Message}");
            return Task.FromResult(false);
        }
    }
}