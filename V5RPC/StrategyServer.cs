using System;
using Google.Protobuf;
using V5RPC.Proto;

namespace V5RPC
{
    public class StrategyServer : IDisposable
    {
        readonly V5Server server;
        readonly IStrategy strategy;

        private static readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];

        byte[] Nothing => EMPTY_BYTE_ARRAY;

        public StrategyServer(int port, IStrategy strategy)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            server = new V5Server(port);
        }

        public void Run()
        {
            server.Run(ServerRoutine);
        }

        public void Dispose()
        {
            server.Dispose();
        }

        byte[] ServerRoutine(byte[] input)
        {
            var call = RPCCall.Parser.ParseFrom(input);
            switch (call.MethodCase)
            {
                case RPCCall.MethodOneofCase.OnEvent:
                    //TODO
                    strategy.OnEvent(call.OnEvent.Type, call.OnEvent.Arguments);
                    break;
                case RPCCall.MethodOneofCase.GetTeamInfo:
                    var info = strategy.GetTeamInfo(call.GetTeamInfo.ServerVersion);
                    info.Version = V5RPC.Proto.Version.V11;
                    return new GetTeamInfoResult
                    {
                        TeamInfo = info
                    }.ToByteArray();
                case RPCCall.MethodOneofCase.GetInstruction:
                    var (wheels, controlInfo) = strategy.GetInstruction(call.GetInstruction.Field);
                    return new GetInstructionResult
                    {
                        Wheels = { wheels },
                        Command = controlInfo,
                    }.ToByteArray();
                case RPCCall.MethodOneofCase.GetPlacement:
                    return new GetPlacementResult
                    {
                        Placement = strategy.GetPlacement(call.GetPlacement.Field)
                    }.ToByteArray();
            }
            return Nothing;
        }
    }
}
