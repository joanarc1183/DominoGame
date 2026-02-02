using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core
{

    public class Domino
    {
        public Dot LeftPip { get; set; }
        public Dot RightPip { get; set; }

        public Domino(Dot left, Dot right)
        {
            LeftPip = left;
            RightPip = right;
        }
    }


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

    public class Board : IBoard
    {
        private readonly List<Domino> _dominoes = new();

        public void PlaceDomino(Domino domino, BoardSide side)
        {
            if (side == BoardSide.Left)
                _dominoes.Insert(0, domino);
            else
                _dominoes.Add(domino);
        }

        public IEnumerable<Domino> GetPlacedDominoes() => _dominoes;

        public Dot GetLeftEnd() => _dominoes.Count > 0 ? _dominoes[0].LeftPip : Dot.Blank;
        public Dot GetRightEnd() => _dominoes.Count > 0 ? _dominoes[^1].RightPip : Dot.Blank;
    }

    public class Player
    {
        public string Name { get; }
        public List<Domino> Hand { get; } = new();

        public Player(string name) => Name = name;
    }
}
