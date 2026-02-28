using Poker.Connection;
using Poker.Models;
using Poker.Services;
using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Poker.Connection
{
    [JsonDerivedType(typeof(ClientConnectData), typeDiscriminator: "clientConnectData")]
    [JsonDerivedType(typeof(ClientDisconnectData), typeDiscriminator: "clientDisconnectData")]
    [JsonDerivedType(typeof(ClientMove), typeDiscriminator: "clientMove")]
    [JsonDerivedType(typeof(GameUpdated), typeDiscriminator: "gameUpdated")]
    [JsonDerivedType(typeof(GameState), typeDiscriminator: "gameState")]
    [JsonDerivedType(typeof(ConnectionDeclined), typeDiscriminator: "connectionDeclined")]
    public abstract record DataTransferBase { }

    public record ClientConnectData(string name) : DataTransferBase;
    public record ClientDisconnectData(string reason) : DataTransferBase;
    public record ClientMove(ClientMoveType moveType, int? amount = null) : DataTransferBase;
    public enum ClientMoveType
    {
        Call,
        Bet,
        Fold,
        Check,
        Raise
    }
    public record GameUpdated(Guid playeId, GameUpdatedType updateType, int SequenceNumber, int? amount = null) : DataTransferBase;
    public enum GameUpdatedType
    {
        Call,
        Bet,
        Fold,
        Check,
        Raise,
        Disconnected,
        Connected
    }
    public record GameState(string roomName, int dealerIndex,
        decimal smallBlind, decimal bigBlind, decimal minBet,
        decimal pot, List<PokerCard> communityCards, GameStage stage,
        int currentPlayerIndex, List<PlayerInfo> players, Guid playerId) : DataTransferBase;
    public record DealCardsData(List<PokerCard> hand);//нужно зашифровать
    public record ConnectionDeclined(string reason) : DataTransferBase;
}
