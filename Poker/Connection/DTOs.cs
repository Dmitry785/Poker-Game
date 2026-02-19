using Poker.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Connection
{
    public abstract record DataTransferBase { }

    //при Conected/NotConnected
    public record ClientMove(ClientMoveType moveType, int? amount = null) : DataTransferBase;
    }
    public enum ClientMoveType
    {
        Call,
        Bet,
        Fold,
        Check,
        Raise,
        Connect,
        Disconnect
    }
    //при Connecting ничего не отправляем
    //при Hosting
    public record GameUpdated(Guid playeId, GameUpdatedType updateType, int? amount = null) : DataTransferBase;//отправляем всем подключенным

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
    //при десинхронизации или при первом
    //подключении клиента к хосту
    public record GameState(bool connectAccepted) : DataTransferBase;
        //название комнаты
        //список игроков
        //состояние игры
