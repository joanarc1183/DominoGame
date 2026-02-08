using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core
{
    /// Nilai pip pada domino (0-6).
    public enum Dot { Blank, One, Two, Three, Four, Five, Six }
    /// Orientasi domino untuk kebutuhan tampilan.
    public enum DominoOrientation { Horizontal, Vertical }
    /// Sisi board tempat domino diletakkan.
    public enum BoardSide { Left, Right }
}
