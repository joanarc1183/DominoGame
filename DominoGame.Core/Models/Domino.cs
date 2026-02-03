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

        public Domino Flip()
        => new Domino(RightPip, LeftPip);
    }

}
