using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace PititiBot.Modules;

public class AttackModule : InteractionModuleBase<SocketInteractionContext>
{
    // {0} is the target's mention, {1} is the rolled damage. Lines that don't land
    // simply never reference {1} — string.Format ignores the spare argument.
    private static readonly string[] Attacks =
    {
        // Direct hits
        "PITITI ATTACK!! Pititi throw whole TOOLBOX at {0}!! DIRECT HITSIES!! {1} damages!! YAYA!!",
        "PITITI BONK!! Pititi hit {0} with big CROWBAR!! CLONK!! {1} damages!! YAYA!!",
        "PITITI SNEAKY!! Pititi bite {0} on the anklesies!! CHOMP!! {1} damages!! HEHE!!",
        "PITITI POKE!! Pititi jab {0} with pointy spear!! POKESIES!! {1} damages!!",
        "PITITI YEET!! Pititi throw whole CARGO CRATE at {0}!! SPLAT!! {1} damages!! BIG YAYA!!",
        "PITITI WELD!! Pititi make {0} very warm and crispsies!! {1} damages!! Is smell like dinner!!",
        "PITITI WHACK!! Pititi whack {0} with fire extinguisher!! BONK!! {1} damages!! Now {0} is cold AND hurt!!",
    };

    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    [SlashCommand("attack", "Pititi attacks someone (no one is really hurt, is just for fun)")]
    public async Task HandleAttackCommand(
        [Summary("user", "Who Pititi should attack")] SocketUser user)
    {
        if (user.Id == Context.Client.CurrentUser.Id)
        {
            await RespondAsync($" NO!! PITITI IS OF FRIEND!! Pititi no attack Pititi!! Pititi bite {Context.User.Mention} instead!! CHOMP!!");
            return;
        }

        if (user.Id == Context.User.Id)
        {
            await RespondAsync($" PITITI CONFUSED!! Why {user.Mention} want hit {user.Mention}?? Is of silly!! Pititi no help with that.");
            return;
        }

        var damage = Random.Shared.Next(1, 100);
        var attack = Attacks[Random.Shared.Next(Attacks.Length)];

        await RespondAsync(string.Format(attack, user.Mention, damage));
    }
}
