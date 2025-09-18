using System;
using System.Collections.Generic;

/// <summary>
/// Read-only deck provider contract with change notification.
/// </summary>
public interface IDeckProvider<TCard>
{
    IReadOnlyList<TCard> GetDeck();
    event Action DeckChanged;
}
