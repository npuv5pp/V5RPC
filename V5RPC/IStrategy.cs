﻿using V5RPC.Proto;

namespace V5RPC
{
    public interface IStrategy
    {
        void OnEvent(EventType type, EventArguments arguments);//TODO
        TeamInfo GetTeamInfo(Version serverVersion);
        (Wheel[], ControlInfo) GetInstruction(Field field);
        Placement GetPlacement(Field field);
    }
}
