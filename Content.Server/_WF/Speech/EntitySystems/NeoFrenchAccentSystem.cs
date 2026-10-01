// WOLFGATE:Customization
using Content.Server.Speech.Components;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems;

/// <summary>
/// System that gives the speaker an alternate faux-French accent.
/// </summary>
public sealed class NeoFrenchAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    private static readonly Regex RegexTh = new(@"th", RegexOptions.IgnoreCase);
    private static readonly Regex RegexSpacePunctuation = new(@"(?<=\w\w)[!?;:](?!\w)", RegexOptions.IgnoreCase);
    private static readonly Regex RegexI = new(@"(?<!\w)i(?!\w)|i", RegexOptions.IgnoreCase);
    private static readonly Regex RegexFinalEr = new(@"er\b", RegexOptions.IgnoreCase);
    private static readonly Regex RegexR = new(@"r", RegexOptions.IgnoreCase);
    private static readonly Regex RegexInitialVowel = new(@"\b([aeiou])", RegexOptions.IgnoreCase);
    private static readonly Regex RegexCh = new(@"ch", RegexOptions.IgnoreCase);
    private static readonly Regex RegexH = new(@"(?<![cs])h", RegexOptions.IgnoreCase);
    private static readonly Regex RegexSEdge = new(@"(?<=[aeiou])s\b|\bs(?=[aeiou])", RegexOptions.IgnoreCase);
    private static readonly Regex RegexSBetweenVowels = new(@"(?<=[aeiou])s(?=[aeiou])", RegexOptions.IgnoreCase);
    private static readonly Regex RegexS = new(@"s", RegexOptions.IgnoreCase);
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeoFrenchAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, NeoFrenchAccentComponent component)
    {
        var msg = message;

        msg = _replacement.ApplyReplacements(msg, "neofrench");

        // Replaces "th" with "'z" (65%) or "'d" (35%).
        msg = RegexTh.Replace(msg, match =>
        {
            return Random.Shared.NextDouble() < 0.35 ? "'d" : "'z";
        });

        // Converts "ch" to "sh"
        msg = RegexCh.Replace(msg, "sh");

        // Replaces "h" with "'" unless it is part of "sh".
        msg = RegexH.Replace(msg, "'");

        // Replaces "s" with "z" at word edges when adjacent to a vowel.
        msg = RegexSEdge.Replace(msg, "z");

        // Replaces "s" with "z" between vowels.
        msg = RegexSBetweenVowels.Replace(msg, "z");

        // Replaces other "s" with "z" at a low chance.
        msg = RegexS.Replace(msg, match =>
        {
            return Random.Shared.NextDouble() < 0.15 ? "z" : match.Value;
        });

        // Spaces out ! ? : and ;.
        msg = RegexSpacePunctuation.Replace(msg, " $&");

        msg = RegexI.Replace(msg, match =>
        {
            return match.Value.Equals("I", StringComparison.Ordinal) &&
                match.Index + match.Length < msg.Length &&
                char.IsWhiteSpace(msg[match.Index + match.Length])
                ? "I"
                : "e";
        });

        // Replaces final "er" with "eur".
        msg = RegexFinalEr.Replace(msg, "eur");

        // Randomly doubles "r" (50%), excluding the r in "eur".
        msg = RegexR.Replace(msg, match =>
        {
            if (match.Index >= 2 &&
                msg.Substring(match.Index - 2, 2).Equals("eu", StringComparison.OrdinalIgnoreCase))
            {
                return match.Value;
            }

            return Random.Shared.NextDouble() < 0.5 ? "rr" : "r";
        });

        // Randomly adds an initial "h" before vowels (20%).
        // Does not affect words beginning with "y".
        msg = RegexInitialVowel.Replace(msg, match =>
        {
            return Random.Shared.NextDouble() < 0.2
                ? "h" + match.Value
                : match.Value;
        });

        return msg;
    }

    private void OnAccentGet(EntityUid uid, NeoFrenchAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
