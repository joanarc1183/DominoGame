using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core;

public class Boneyard
{
    public List<Domino> Dominoes { get; private set; } = new();
    public bool IsEmpty => Dominoes.Count == 0;

    public Boneyard(IEnumerable<Domino> set)
    {
        Dominoes = set.ToList();
        Shuffle();
    }

    public Domino Draw()
    {
        if (IsEmpty) throw new InvalidOperationException("Boneyard empty");
        var d = Dominoes[0];
        Dominoes.RemoveAt(0);
        return d;
    }

    private void Shuffle()
    {
        var rnd = new Random();
        Dominoes = Dominoes.OrderBy(_ => rnd.Next()).ToList();
    }
}