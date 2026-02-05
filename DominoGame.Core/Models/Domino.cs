using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core
{
    public sealed class Domino
    {
        /// Nilai pip di sisi kiri domino.
        public Dot LeftPip { get; private set; }
        /// Nilai pip di sisi kanan domino.
        public Dot RightPip { get; private set; }
        /// Orientasi domino untuk keperluan tampilan.
        public DominoOrientation Orientation { get; set; }

        /// Membuat domino dengan nilai pip kiri dan kanan.
        public Domino(Dot left, Dot right)
        {
            LeftPip = left;
            RightPip = right;
            Orientation = DominoOrientation.Horizontal;
        }

        /// Mengembalikan domino baru dengan sisi kiri dan kanan ditukar.
        public Domino Flip()
        {
            return new Domino(RightPip, LeftPip) { Orientation = Orientation };
        }
    }
}
