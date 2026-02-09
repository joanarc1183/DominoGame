using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core;

public class Player : IPlayer
{
    /// Nama pemain.
    public string Name { get; }
    /// Skor pemain saat ini.
    public int Score { get; set; }

    /// Membuat pemain dengan nama tertentu.
    public Player(string name)
    {
        Name = name;
    }
}
