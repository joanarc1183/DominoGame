using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core
{
    public sealed class Domino
    {
        public Dot LeftPip { get; private set; }
        public Dot RightPip { get; private set; }
        public DominoOrientation Orientation { get; set; }

        public Domino(Dot left, Dot right)
        {
            LeftPip = left;
            RightPip = right;
            Orientation = DominoOrientation.Horizontal;
        }

        public bool IsDouble() => LeftPip == RightPip;

        public bool CanConnect(Domino other)
        {
            return LeftPip == other.LeftPip ||
                   LeftPip == other.RightPip ||
                   RightPip == other.LeftPip ||
                   RightPip == other.RightPip;
        }

        public Domino Flip()
        {
            return new Domino(RightPip, LeftPip) { Orientation = Orientation };
        }

        public override string ToString() => $"[{LeftPip}|{RightPip}]";
    }
}
