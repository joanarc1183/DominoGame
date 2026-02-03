using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core;

public class Boneyard
{
    public List<Domino> Dominoes { get; private set; }
    public bool IsEmpty => Dominoes.Count == 0;

    public Boneyard(IEnumerable<Domino> fullSet)
    {
        Dominoes = fullSet.ToList();
        Shuffle();
    }

    public Domino Draw()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Boneyard is empty");

        var domino = Dominoes[0];
        Dominoes.RemoveAt(0);
        return domino;
    }

    private void Shuffle()
    {
        var rnd = new Random();
        Dominoes = Dominoes.OrderBy(_ => rnd.Next()).ToList();
    }
}
