﻿using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Collections;
using V5RPC.Proto;

namespace V5RPC
{
    public class StrategyServer : IDisposable
    {
        V5Server server;
        IStrategy strategy;

        private static readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];

        byte[] Nothing { get { return EMPTY_BYTE_ARRAY; } }

        public StrategyServer(int port, IStrategy strategy)
        {
            this.strategy = strategy ?? throw new ArgumentNullException("strategy");
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
                    //TODO
                    return new GetTeamInfoResult
                    {
                        TeamInfo = strategy.GetTeamInfo()
                    }.ToByteArray();
                case RPCCall.MethodOneofCase.GetInstruction:
                    return new GetInstructionResult
                    {
                        Wheels = { strategy.GetInstruction(call.GetInstruction.Field) }
                    }.ToByteArray();
                case RPCCall.MethodOneofCase.GetPlacement:
                    return new GetPlacementResult
                    {
                        Robots = { strategy.GetPlacement(call.GetPlacement.Field) }
                    }.ToByteArray();
            }
            return Nothing;
        }
    }
}