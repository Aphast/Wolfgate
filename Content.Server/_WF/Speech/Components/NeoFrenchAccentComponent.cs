using Content.Server.Speech.EntitySystems;

namespace Content.Server.Speech.Components;

/// <summary>
/// Alternate French accent replaces spoken letters. "th" becomes "z" and "H" at the start of a word becomes "'".
/// </summary>
[RegisterComponent]
[Access(typeof(NeoFrenchAccentSystem))]
public sealed partial class NeoFrenchAccentComponent : Component {}
