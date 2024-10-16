using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Discord.Rest;
using DiscordBotDotNet.Handlers;
using DiscordBotDotNet.Models;

public class InteractionCollector
{
    private readonly IUserMessage _message;
    private readonly SocketTextChannel _channel;
    private readonly SocketGuildUser _member;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;

    public InteractionCollector(IUserMessage message, SocketTextChannel channel, SocketGuildUser member, IMongoCollection<User> userCollection, IConfiguration configuration, DiscordSocketClient client)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _member = member ?? throw new ArgumentNullException(nameof(member));
        _userCollection = userCollection ?? throw new ArgumentNullException(nameof(userCollection));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task CollectGenderResponse(User userData)
    {
        _client.InteractionCreated += async (interaction) =>
        {
            if (interaction is SocketMessageComponent component)
            {
                if (component.User.Id == _member.Id)
                {
                    var maleRoleId = _configuration["Server:Roles:Genders:Male"] != null ? ulong.Parse(_configuration["Server:Roles:Genders:Male"] ?? throw new InvalidOperationException()) : 0;
                    var femaleRoleId = _configuration["Server:Roles:Genders:Female"] != null ? ulong.Parse(_configuration["Server:Roles:Genders:Female"] ?? throw new InvalidOperationException()) : 0;

                    if (component.Data.CustomId == "male")
                    {
                        userData.Gender = "male";
                    }
                    else if (component.Data.CustomId == "female")
                    {
                        userData.Gender = "female";
                    }

                    await _userCollection.ReplaceOneAsync(u => u.UserId == _member.Id, userData);

                    await component.RespondAsync($"Vous avez sélectionné {userData.Gender}.", ephemeral: true);

                    await _message.ModifyAsync(m => m.Components = new ComponentBuilder().Build());

                    var quizService = new QuizHandler(_userCollection, _configuration, _client);
                    await quizService.HandleQuizProgress(_channel, _member, userData);
                }
            }
        };

        await Task.Delay(TimeSpan.FromMinutes(5));
    }

    public async Task CollectBirthdayResponse(User userData)
    {
        TaskCompletionSource<SocketMessage> tcs = new TaskCompletionSource<SocketMessage>();

        Task HandleMessage(SocketMessage message)
        {
            if (message.Channel.Id == _channel.Id && message.Author.Id == _member.Id)
            {
                tcs.SetResult(message);
            }

            return Task.CompletedTask;
        }

        _client.MessageReceived += HandleMessage;

        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(2));
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        _client.MessageReceived -= HandleMessage;

        if (completedTask == timeoutTask)
        {
            await _channel.SendMessageAsync("Temps écoulé. Veuillez réessayer.");
            return;
        }

        var message = await tcs.Task;

        if (message != null)
        {
            if (DateTime.TryParse(message.Content, out DateTime birthdate))
            {
                userData.Birthday = birthdate;
                await _userCollection.ReplaceOneAsync(u => u.UserId == _member.Id, userData);

                await _channel.SendMessageAsync($"Merci, votre date de naissance est : {birthdate:dd/MM/yyyy}.");

                var quizService = new QuizHandler(_userCollection, _configuration, _client);
                await quizService.HandleQuizProgress(_channel, _member, userData);
            }
            else
            {
                await _channel.SendMessageAsync("Format de date incorrect. Essayez à nouveau (JJ/MM/AAAA).");
            }
        }
    }

    public async Task CollectPersonalityResponse(User userData, string optionA, string optionB, int nextStep)
    {
        _client.InteractionCreated += async (interaction) =>
        {
            if (interaction is SocketMessageComponent component)
            {
                if (component.User.Id == _member.Id)
                {
                    userData.PersonalityType = (component.Data.CustomId == optionA) ? optionA : optionB;

                    await _userCollection.ReplaceOneAsync(u => u.UserId == _member.Id, userData);

                    await component.RespondAsync($"Vous avez choisi {component.Data.CustomId}.", ephemeral: true);

                    await _message.ModifyAsync(m => m.Components = new ComponentBuilder().Build());

                    var quizService = new QuizHandler(_userCollection, _configuration, _client);
                    userData.QuestionnaireProgress = nextStep;
                    await quizService.HandleQuizProgress(_channel, _member, userData);
                }
            }
        };

        await Task.Delay(TimeSpan.FromMinutes(5));
    }
}
