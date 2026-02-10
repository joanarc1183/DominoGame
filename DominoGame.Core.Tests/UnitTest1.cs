using NUnit.Framework;
using Moq;
using DominoGame.Core;
using System.Collections.Generic;

namespace DominoGame.Core.Tests;

[TestFixture]
public class GameController_PlayDomino
{
    private GameController _gameController;

    private Mock<IPlayer> _playerMock;
    private Mock<IPlayer> _otherPlayerMock;
    private Mock<IDomino> _dominoMock;
    private Mock<IBoard> _boardMock;

    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {

        _playerMock = new Mock<IPlayer>();
        _otherPlayerMock = new Mock<IPlayer>();
        _dominoMock = new Mock<IDomino>();
        _boardMock = new Mock<IBoard>();

        _players = new List<IPlayer>
        {
            _playerMock.Object,
            _otherPlayerMock.Object,
        };
        
        _gameController = new GameController(_players, _boardMock.Object, 100);
    
        _gameController.StartRound();
    }

    [Test]
    public void PlayDomino_CurrentPlayer_ReturnsFalse()
    {
        var domino = _gameController.GetHands(_playerMock.Object)[0];

        var result = _gameController.PlayDomino(_otherPlayerMock.Object, domino, BoardSide.Left);

        Assert.That(result, Is.False);
    }
}
