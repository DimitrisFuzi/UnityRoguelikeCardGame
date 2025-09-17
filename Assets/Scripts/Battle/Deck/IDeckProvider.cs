using System;
using System.Collections.Generic;

public interface IDeckProvider<TCard>
{
    IReadOnlyList<TCard> GetDeck();
    event Action DeckChanged;
}
