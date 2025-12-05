using System.Collections.Generic;
using CharactersStats;

public interface IStatCollection
{
    CharacterStat GetStat(StatId id);
    IEnumerable<CharacterStat> AllStats { get; }
}