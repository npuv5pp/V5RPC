# V5RPC
[Simuro5v5](https://github.com/npuv5pp/Simuro5v5)比赛平台使用的通信模块。

## 文档

### 概述
新比赛平台使用网络与双方的策略服务器通信。本模块实现了底层通信协议和比赛通信接口，并面向平台和策略提供了对应的实现类。
如果你使用C#等语言编写策略，并运行于.NET CLR环境，则你可以直接引用此程序集。

如果你使用C/C++等语言编写策略，并希望将策略编译为DLL，请使用[V5DLLAdapter](https://github.com/npuv5pp/V5DLLAdapter)。

你也可以使用你喜欢的语言实现相同的通讯协议。本模块只是作为参考的C# .NET Standard类库。

### 系统要求
本模块为.NET Standard 2.0类库。若要引用此类库，你需要满足条件的对应平台的SDK。
- .NET Framework 4.6.1或更高版本
- .NET Core 2.0或更高版本

### 基本概念
比赛所用的通信协议分为上下两层。下层协议负责数据报文的传输，上层协议负责实现比赛逻辑。

考虑到旧平台的通信方式为策略DLL导出相应接口供比赛平台调用，双方代码同时在比赛平台内不受限制地执行所导致的一系列问题，
新的通信方式使用网络协议取代了DLL接口，并采用了远程过程调用（RPC）风格。
- **客户端**：指调用的发起方，即比赛平台。比赛平台会不停地向双方队伍的策略服务器发起调用，并将结果作为双方机器人的行动依据。
- **服务器**：指调用的接受方，即策略服务器。双方的策略均作为服务器存在，接受平台的调用并返回相应的结果。
- **下层协议**：下层协议由`V5RPC.cs`中的`V5Client`和`V5Server`类实现。该协议基于UDP，设计目标是提供一个简单可靠的数据报协议，拥有确认和重传机制，
保证客户端发起的单次调用会被服务器处理至少一次。
- **上层协议**：上层协议使用Protocol Buffer封装比赛逻辑。`V5RPC/Proto`文件夹中的proto源代码定义了相关的数据结构，
在生成项目时会被编译为对应的C#源代码。`StrategyClient`类基于比赛逻辑封装了客户端功能，由比赛平台使用。
`StrategyServer`类基于比赛逻辑封装了服务器功能，编写策略时只需创建该类的实例并提供`IStrategy`接口的实现即可。

### 比赛通信接口
策略需要处理如下的RPC调用：

```csharp
void OnEvent(EventType type, EventArguments arguments);

TeamInfo GetTeamInfo();

Wheel[] GetInstruction(Field field);

Placement GetPlacement(Field field);
```

数据类型请参见下文。

#### OnEvent
当特定事件发生时，平台调用该函数通知策略。
- **type**：事件类型。
- **arguments**：事件参数。不同的事件类型具有不同的参数，有些事件类型没有参数。

#### GetTeamInfo
平台调用该函数获得策略所属队伍的信息。
- **返回值**：队伍信息。

#### GetInstruction
在比赛的每一拍中，平台调用该函数获得由策略控制的机器人轮速。
- **field**：表示当前的场上状态。
- **返回值**：每个机器人的轮速。

#### GetPlacement
当需要自动摆位时，平台调用该函数获得由策略控制的机器人位置。
- **field**：表示当前的场上状态。
- **返回值**：自动摆位信息。

### 基本数据结构

```julia
struct TeamInfo
    teamName::String
end

struct Vector2
    x::Float32
    y::Float32
end

struct Ball
    position::Vector2
end

struct Wheel
    leftSpeed::Float32
    rightSpeed::Float32
end

struct Robot
    position::Vector2
    rotation::Float32
    wheel::Wheel
end

struct Field
    selfRobots::Array{Robot}#5
    opponentRobots::Array{Robot}#5
    ball::Ball
    tick::Int32
end
```

上述字段的意义与旧平台基本相同。

### 事件定义
可能的事件类型如下：

```julia
@enum EventType begin
    JudgeResult = 0
    MatchStart = 1
    MatchStop = 2
end
```

- **JudgeResult**：当平台公布裁判结果时发送。参数类型为`JudgeResultEvent`。
- **MatchStart**：当比赛开始时发送。没有参数。
- **MatchStop**：当比赛结束时发送。没有参数。

可能的事件参数如下：

#### JudgeResultEvent
```julia
struct JudgeResultEvent
    type::ResultType
    offensiveTeam::Team
    reason::String
end

@enum ResultType begin
    PlaceKick = 0
    GoalKick = 1
    PenaltyKick = 2
    FreeKickRightTop = 3
    FreeKickRightBot = 4
    FreeKickLeftTop = 5
    FreeKickLeftBot = 6
end

@enum Team begin
    Self = 0
    Opponent = 1
    None = 2
end
```

## 作者

该项目当前由AzureFx编写和维护。保留所有权利。

Simuro5v5是西北工业大学V5++团队的项目。
