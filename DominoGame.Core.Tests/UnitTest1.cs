using NUnit.Framework;
using Moq;
using DominoGame.Core;
using System.Collections.Generic;
using NUnit.Framework.Internal.Execution;
using System.Reflection;

namespace DominoGame.Core.Tests;

[TestFixture]
public class GameControllerTests
{
    private GameController _gameController;

    private Mock<IPlayer> _playerMock;
    private Mock<IPlayer> _otherPlayerMock;
    private Mock<IDomino> _dominoMock;
    private Board _board;

    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {

        _playerMock = new Mock<IPlayer>();
        _otherPlayerMock = new Mock<IPlayer>();
        _dominoMock = new Mock<IDomino>();
        _board = new Board();

        _players = new List<IPlayer>
        {
            _playerMock.Object,
            _otherPlayerMock.Object,
        };
        
        _gameController = new GameController(_players, _board, 100);
    }

    [Test]
    public void StartRound_InitializesGameState()
    {
        _gameController.StartRound();

        Assert.That(_gameController.CurrentPlayer, Is.EqualTo(_playerMock.Object));
        Assert.That(_gameController.Board, Is.EqualTo(_board));
        Assert.That(_gameController.IsRoundEnded, Is.False);
        Assert.That(_gameController.IsGameEnded, Is.False);
        Assert.That(_gameController.GameWinner, Is.Null);
    }

    [Test]
    public void StartRound_OnTurnChanged()
    {
        bool eventTriggered = false;
        IPlayer? turnPlayer = null;

        _gameController.OnTurnChanged += (player) =>
        {
            eventTriggered = true;
            turnPlayer = player;
        };

        _gameController.StartRound();

        Assert.That(eventTriggered, Is.True);
        Assert.That(turnPlayer, Is.EqualTo(_playerMock.Object));
    }

    [Test]
    public void NextTurn_ChangesCurrentPlayer()
    {
        _gameController.StartRound();

        IPlayer firstPlayer = _gameController.CurrentPlayer;

        _gameController.NextTurn();

        IPlayer secondPlayer = _gameController.CurrentPlayer;

        Assert.That(firstPlayer, Is.Not.EqualTo(secondPlayer));
        Assert.That(secondPlayer, Is.EqualTo(_otherPlayerMock.Object));
    }

    [Test]
    public void NextTurn_CannotPlay_MustPassTurn()
    {
        _gameController.StartRound();

        _board.Dominoes.AddFirst(new Domino((Dot)6, (Dot)6));
        SetHand(_playerMock.Object, new Domino((Dot)1, (Dot)2));

        bool passTriggered = false;
        IPlayer? passedPlayer = null;
        IPlayer currentPlayerBefore = _gameController.CurrentPlayer;

        _gameController.OnPlayerPassed += (player) =>
        {
            passTriggered = true;
            passedPlayer = player;
        };

        _gameController.NextTurn();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(passTriggered, Is.True);
            Assert.That(passedPlayer, Is.EqualTo(currentPlayerBefore));
            Assert.That(_gameController.CurrentPlayer, Is.EqualTo(_otherPlayerMock.Object));
        }
    }

    [Test]
    public void NextTurn_RoundEnded_DoesNotChangeCurrentPlayer()
    {
        _gameController.StartRound();

        FieldInfo roundEndedField = typeof(GameController)
            .GetField("_roundEnded", BindingFlags.NonPublic | BindingFlags.Instance)!;
        roundEndedField.SetValue(_gameController, true);

        IPlayer currentPlayerBefore = _gameController.CurrentPlayer;

        _gameController.NextTurn();

        IPlayer currentPlayerAfter = _gameController.CurrentPlayer;

        Assert.That(currentPlayerBefore, Is.EqualTo(currentPlayerAfter));
    }

    [Test]
    public void NextTurn_OnTurnChanged()
    {
        bool eventTriggered = false;

        _gameController.StartRound();

        _gameController.OnTurnChanged += (player) =>
        {
            eventTriggered = true;
        };

        _gameController.NextTurn();

        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void GetHands_ReturnsCorrectDominoes()
    {
        _gameController.StartRound();

        IReadOnlyList<IDomino> playerHand = _gameController.GetHands(_playerMock.Object);
        IReadOnlyList<IDomino> otherPlayerHand = _gameController.GetHands(_otherPlayerMock.Object);

        Assert.That(playerHand, Is.Not.Null);
        Assert.That(otherPlayerHand, Is.Not.Null);
        Assert.That(playerHand.Count, Is.EqualTo(7));
        Assert.That(otherPlayerHand.Count, Is.EqualTo(7));
    }

    [Test]
    public void GetHands_ReturnsEmptyListDominoes()
    {
        IReadOnlyList<IDomino> playerHand = _gameController.GetHands(_playerMock.Object);
        IReadOnlyList<IDomino> otherPlayerHand = _gameController.GetHands(_otherPlayerMock.Object);

        Assert.That(playerHand, Is.Not.Null);
        Assert.That(otherPlayerHand, Is.Not.Null);
        Assert.That(playerHand.Count, Is.EqualTo(0));
        Assert.That(otherPlayerHand.Count, Is.EqualTo(0));
    }

    [Test]
    public void PlayDomino_NotCurrentPlayer_ReturnsFalse()
    {
        _gameController.StartRound();

        IDomino domino = _gameController.GetHands(_playerMock.Object)[0];

        bool result = _gameController.PlayDomino(_otherPlayerMock.Object, domino, BoardSide.Left);
        // bool result = _gameController.PlayDomino(_otherPlayerMock.Object, domino, BoardSide.Right);

        Assert.That(result, Is.False);
    }

    [Test]
    public void PlayDomino_DominoIsNotConcrete_ThrowsExeption()
    {
        _gameController.StartRound();

        IDomino fakeDomino = new Mock<IDomino>().Object;

        AddDominoToHand(_playerMock.Object, fakeDomino);

        Assert.Throws<InvalidOperationException>(() =>
        {
            _gameController.PlayDomino(_playerMock.Object, fakeDomino, BoardSide.Left);
        });
    }

    [Test]
    public void PlayDomino_CannotPlace_ReturnsFalse()
    {
        _gameController.StartRound();
        
        _board.Dominoes.AddFirst(new Domino((Dot)6, (Dot)6));

        Domino domino = new Domino((Dot)1, (Dot)2);

        AddDominoToHand(_playerMock.Object, domino);

        bool result = _gameController.PlayDomino(_playerMock.Object, domino, BoardSide.Left);

        Assert.That(result, Is.False);
    }

    [Test]
    public void PlayDomino_CanPlace_ReturnsTrue()
    {
        _gameController.StartRound();
        
        Domino domino = (Domino)_gameController.GetHands(_playerMock.Object)[0];

        bool result = _gameController.PlayDomino(_playerMock.Object, domino, BoardSide.Left);

        Assert.That(result, Is.True);
    }

    [Test]
    public void PlayDomino_ValidMove_ReturnsTrue()
    {
        _gameController.StartRound();

        Domino domino = (Domino)_gameController.GetHands(_playerMock.Object)[0];
        
        bool eventRaised = false;
        
        _gameController.OnDominoPlaced += (player, d, side) =>
        {
            eventRaised = true;
            Assert.That(_playerMock.Object, Is.EqualTo(player));
            Assert.That(domino, Is.EqualTo(d));
            Assert.That(side, Is.EqualTo(BoardSide.Right));
        };

        bool result = _gameController.PlayDomino(_playerMock.Object, domino, BoardSide.Right);

        Assert.That(result, Is.True);
        Assert.That(eventRaised, Is.True);
        Assert.That(_board.Dominoes.Count, Is.EqualTo(1));
    }

    [Test]
    public void PassTurn_NotCurrentPlayer_ReturnsFalse()
    {
        bool result = _gameController.PassTurn(_otherPlayerMock.Object);
        Assert.That(result, Is.False);
    }

    [Test]
    public void PassTurn_ValidMove_ReturnsTrue()
    {
        _gameController.StartRound();

        bool result = _gameController.PassTurn(_playerMock.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void PassTurn_OnPlayerPassed()
    {
        _gameController.StartRound();

        bool eventRaised = false;
        
        IPlayer? passedPlayer = null;

        _gameController.OnPlayerPassed += (player) =>
        {
            eventRaised = true;
            passedPlayer = player;
        };

        _gameController.PassTurn(_playerMock.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(passedPlayer, Is.EqualTo(_playerMock.Object));
        }
    }

    [Test]
    public void CanPlay_BoardIsEmpty_PlayerHasDomino_ReturnsTrue()
    {
        _gameController.StartRound();
        
        bool result = _gameController.CanPlay(_playerMock.Object);

        Assert.That(result, Is.True);
    }
    
    [Test]
    public void CanPlay_BoardIsEmpty_PlayerHasNoDomino_ReturnsFalse()
    {
        bool result = _gameController.CanPlay(_playerMock.Object);

        Assert.That(result, Is.False);
    }

    [Test]
    public void CanPlay_BoardNotEmpty_PlayerHasDomino_ReturnsTrue()
    {
        _gameController.StartRound();

        _board.Dominoes.AddFirst(new Domino((Dot)3, (Dot)5));

        Domino domino = new Domino((Dot)2, (Dot)3);
        SetHand(_playerMock.Object, domino);

        bool result = _gameController.CanPlay(_playerMock.Object);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CanPlay_BoardNotEmpty_PlayerHasNoDomino_ReturnsFalse()
    {
        _gameController.StartRound();

        _board.Dominoes.AddFirst(new Domino((Dot)6, (Dot)6));

        Domino domino = new Domino((Dot)1, (Dot)2);
        SetHand(_playerMock.Object, domino);

        bool result = _gameController.CanPlay(_playerMock.Object);

        Assert.That(result, Is.False);
    }

    [Test]
    public void CanPlay_DominoIsNotConcrete_ThrowsInvalidOperationException()
    {
        _gameController.StartRound();

        // _board.Dominoes.AddFirst(new Domino((Dot)0, (Dot)0));

        IDomino fakeDomino = new Mock<IDomino>().Object;

        AddDominoToHand(_playerMock.Object, fakeDomino);

        Assert.Throws<InvalidOperationException>(() =>
        {
            _gameController.CanPlay(_playerMock.Object);
        });
    }

    [Test]
    public void CanPlace_BoardNotEmpty_CanPlaceOnLeft_ReturnsTrue()
    {
        _gameController.StartRound();

        _board.Dominoes.AddFirst(new Domino((Dot)4, (Dot)6));

        Domino domino = new Domino((Dot)4, (Dot)2);
        SetHand(_playerMock.Object, domino);

        bool result = _gameController.CanPlay(_playerMock.Object);

        Assert.That(result, Is.True);
    }
    
    [Test]
    public void CanPlace_BoardNotEmpty_CanPlaceOnRight_ReturnsTrue()
    {
        _gameController.StartRound();

        _board.Dominoes.AddFirst(new Domino((Dot)1, (Dot)3));

        Domino domino = new Domino((Dot)5, (Dot)3);
        SetHand(_playerMock.Object, domino);

        bool result = _gameController.CanPlay(_playerMock.Object);

        Assert.That(result, Is.True);
    }
    
    private void AddDominoToHand(IPlayer player, IDomino domino)
    {
        FieldInfo fieldInfo = typeof(GameController)
            .GetField("_dominoInHands", BindingFlags.NonPublic | BindingFlags.Instance)!;
        
        Dictionary<IPlayer, List<IDomino>> hands = (Dictionary<IPlayer, List<IDomino>>)fieldInfo.GetValue(_gameController)!;

        hands[player].Add(domino);
    }

    private void SetHand(IPlayer player, params IDomino[] dominoes)
    {
        FieldInfo fieldInfo = typeof(GameController)
            .GetField("_dominoInHands", BindingFlags.NonPublic | BindingFlags.Instance)!;

        Dictionary<IPlayer, List<IDomino>> hands = (Dictionary<IPlayer, List<IDomino>>)fieldInfo.GetValue(_gameController)!;

        hands[player].Clear();
        hands[player].AddRange(dominoes);
    }

    [Test]
    public void CountPips_ReturnsTotalPips()
    {
        _gameController.StartRound();

        Domino domino1 = new Domino((Dot)2, (Dot)3); // Total pips = 5
        Domino domino2 = new Domino((Dot)4, (Dot)6); // Total pips = 10

        SetHand(_playerMock.Object, domino1, domino2);

        int totalPips = _gameController.CountPips(_playerMock.Object);

        Assert.That(totalPips, Is.EqualTo(15));
    }
}
