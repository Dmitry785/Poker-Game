using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Services
{
    public abstract record GameCommand;
    public record CallCommand : GameCommand;
    public record FoldCommand : GameCommand;
    public record CheckCommand : GameCommand;
    public record SendCommonMessage(string message) : GameCommand;
    //приватное сообщение
    public record BetRaiseCommand(decimal amount) : GameCommand;
    public record StartHostCommand : GameCommand;
    public record StartGameCommand : GameCommand;
    public record ConnectCommand(IPEndPoint hostEndPoint) : GameCommand;
    public record DisconnectCommand(string reason = "") : GameCommand;
    //пригласить
}
