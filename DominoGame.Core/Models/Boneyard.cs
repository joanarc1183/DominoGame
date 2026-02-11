using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core;

public class Boneyard : IBoneyard
{
    /// Daftar domino yang masih tersisa di boneyard.
    public List<Domino> Dominoes { get; private set; } = new();
    /// Menandakan apakah boneyard sudah habis.
    public bool IsEmpty => Dominoes.Count == 0;

    /// Membuat boneyard dari set domino dan mengacak urutannya.
    public Boneyard(IEnumerable<Domino> set)
    {
        Dominoes = set.ToList();
        Shuffle();
    }

    /// Mengacak urutan domino di boneyard.
    private void Shuffle()
    {
        var rnd = new Random();
        Dominoes = Dominoes.OrderBy(_ => rnd.Next()).ToList();
    }
}
