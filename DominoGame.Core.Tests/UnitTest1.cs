using NUnit.Framework;
using Moq;
using DominoGame.Core;
using System.Collections.Generic;

namespace DominoGame.Core.Tests;

[TestFixture]
public class GameControllerTests
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
        IDomino domino = _gameController.GetHands(_playerMock.Object)[0];

        bool result = _gameController.PlayDomino(_otherPlayerMock.Object, domino, BoardSide.Left);

        Assert.That(result, Is.False);
    }

    [Test]
    public void PlayDomino_Concrete_ThrowsExeption()
    {
        IDomino fakeDomino = new Mock<IDomino>().Object;

        Assert.Throws<InvalidOperationException>(() =>
        {
            _gameController.PlayDomino(_playerMock.Object, fakeDomino, BoardSide.Left);
        });
    }

    [Test]
    public void PlayDomino_CanPlace_ReturnsFalse()
    {
        Domino domino = (Domino)_gameController.GetHands(_playerMock.Object)[0];
        
        _boardMock
            .Setup(b => b.CanPlace(domino, BoardSide.Left))
            .Returns(false);
        
        bool result = _gameController.PlayDomino(_playerMock.Object, domino, BoardSide.Left);

        Assert.That(result, Is.False);
    }

    [Test]
    public void PlayDomino_ValidMove_ReturnsTrue_AndEventRaised()
    {
        Domino domino = (Domino)_gameController.GetHands(_playerMock.Object)[0];
        
        bool eventRaised = false;

        _boardMock
            .Setup(b => b.CanPlace(domino, BoardSide.Right))
            .Returns(true);
        
        _gameController.OnDominoPlaced += (player, d, side) =>
        {
            eventRaised = true;
            Assert.That(_playerMock.Object, Is.EqualTo(player));
            Assert.That(domino, Is.EqualTo(d));
            Assert.That(BoardSide.Right, Is.EqualTo(side));
        };

        bool result = _gameController.PlayDomino(_playerMock.Object, domino, BoardSide.Right);

        Assert.That(result, Is.True);
        Assert.That(eventRaised, Is.True);

        _boardMock.Verify(b => b.Place(domino, BoardSide.Right), Times.Once);
    }
}