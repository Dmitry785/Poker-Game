using Poker.Connection;
using Poker.Models;
using Poker.Services;
using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Poker.Connection
{
    [JsonDerivedType(typeof(ClientConnectData), typeDiscriminator: "clientConnectData")]
    [JsonDerivedType(typeof(ClientDisconnectData), typeDiscriminator: "clientDisconnectData")]
    [JsonDerivedType(typeof(ClientMove), typeDiscriminator: "clientMove")]
    [JsonDerivedType(typeof(GameUpdated), typeDiscriminator: "gameUpdated")]
    [JsonDerivedType(typeof(GameStateAll), typeDiscriminator: "gameStateAll")]
    [JsonDerivedType(typeof(GameStateUpdated), typeDiscriminator: "gameStateUpdated")]
    [JsonDerivedType(typeof(ConnectionDeclined), typeDiscriminator: "connectionDeclined")]
    [JsonDerivedType(typeof(ClientConnected), typeDiscriminator: "clientConnected")]
    [JsonDerivedType(typeof(DealCardsData), typeDiscriminator: "dealCardsData")]
    [JsonDerivedType(typeof(InviteData), typeDiscriminator: "inviteData")]
    [JsonDerivedType(typeof(GameHostingClosed), typeDiscriminator: "gameHostingClosed")]
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
    public record GameHostingClosed(string reason) : DataTransferBase;
    public record GameUpdated(Guid playeId, GameUpdatedType updateType, int? amount = null) : DataTransferBase;
    public enum GameUpdatedType
    {
        Call,
        Bet,
        Fold,
        Check,
        Raise,
        Disconnected
    }
    public record ClientConnected(PlayerInfo player) : DataTransferBase;
    public record InviteData(IPEndPoint hostEndPoint) : DataTransferBase;
    public record GameStateAll(string roomName, int dealerIndex,
        decimal smallBlind, decimal bigBlind, decimal currentMaxBet, decimal lastRaiseStep,
        decimal pot, List<PokerCard> communityCards, GameStage stage,
        int currentPlayerIndex, List<PlayerInfo> players, Guid playerId) : DataTransferBase;
    public record GameStateUpdated(int dealerIndex, decimal currentMaxBet, decimal lastRaiseStep,
        decimal pot, List<PokerCard> communityCards, GameStage stage,
        int currentPlayerIndex) : DataTransferBase;
    public record DealCardsData(List<PokerCard> hand) : DataTransferBase;//нужно зашифровать
    public record ConnectionDeclined(string reason) : DataTransferBase;
}
