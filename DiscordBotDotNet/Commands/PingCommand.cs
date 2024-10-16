using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

public class PingCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Renvoie Pong! et des informations supplémentaires")]
    public async Task Ping()
    {
        var initialEmbed = new EmbedBuilder()
            .WithDescription("🏓 Ping...")
            .WithColor(Color.Blue)
            .Build();

        await RespondAsync(embed: initialEmbed);

        var initialResponse = await GetOriginalResponseAsync();
        var latency = (initialResponse.Timestamp - Context.Interaction.CreatedAt).TotalMilliseconds;
        var wsLatency = Context.Client.Latency;
        var memoryUsage = GetMemoryUsage();
        var uptime = GetUptime();

        var infoEmbed = new EmbedBuilder()
            .WithAuthor(new EmbedAuthorBuilder
            {
                Name = "🏓 Ping",
                IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
            })
            .WithColor(Color.Green)
            .AddField("Temps de réponse", $"{latency:F0} ms", true)
            .AddField("Latence API Discord", $"{wsLatency} ms", true)
            .AddField("Utilisation RAM", memoryUsage, true)
            .AddField("Démarré le", uptime.Item1, true)
            .AddField("Temps de fonctionnement", uptime.Item2, true)
            .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
            .WithFooter(footer =>
            {
                footer.Text = $"Requête par {Context.User.Username}";
                footer.IconUrl = Context.User.GetAvatarUrl();
            })
            .WithTimestamp(DateTimeOffset.Now)
            .Build();
        await ModifyOriginalResponseAsync(msg => msg.Embed = infoEmbed);
    }

    private string GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        var memoryInMB = process.WorkingSet64 / 1024 / 1024;
        return $"{memoryInMB:F2} MB";
    }

    private (string, string) GetUptime()
    {
        var startTime = Process.GetCurrentProcess().StartTime;
        var uptimeSpan = DateTime.Now - startTime;
        string startTimeFormatted = startTime.ToString("yyyy/MM/dd - HH:mm:ss");
        string uptimeFormatted = $"{uptimeSpan.Days}j {uptimeSpan.Hours}h {uptimeSpan.Minutes}m {uptimeSpan.Seconds}s";

        return (startTimeFormatted, uptimeFormatted);
    }
}
