using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoGame.Core;

public class Player
{
    public string Name { get; }
    public List<Domino> Hand { get; } = new();

    public Player(string name) => Name = name;
}
