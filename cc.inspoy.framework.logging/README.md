# Instech.Framework.Logging

该程序集包含了输出日志相关的功能

## 日志类

核心类是`Instech.Logging.Logger`，这是一个静态类，以静态方法的形式提供了所有必要的接口

首先介绍两个概念：LogLevel和LogModule

LogLevel就是日志的重要等级，比如`Error`，`Warning`之类的；LogModule则是为了划分功能模块，是字符串形式的，框架内置了几种常用的（如"UI", "Render"），用户也可以任意传入自定义的字符串

> 这里LogLevel的设置方式和其他日志系统常见的方式有所不同，这里不是根据一个level，level以上的输出，level以下的屏蔽；而是以"Bit field"的形式分别独立设置每种level是否启用。

所有支持的Level按照严重程度排序如下

1. Exception - 代码执行出现期望之外的异常
2. Assert - 有逻辑出现断言失败，这通常意味着有逻辑或配置出现错误
3. Error - 会导致正常流程中断的错误
4. Warning - 警告表示不影响整体运行但有必要处理的问题
5. Info - 一般信息，用于记录游戏运行情况（比如游戏状态切换，玩家环境信息等）
6. Verbose - 冗余信息，主要用于调试，不应该出现在release中
7. Debug - 临时的调试日志，不应该出现在release中

然后我们看一下接口

```csharp
// 仅用于编辑器环境下的临时输出，等同于LogDebug("Default", 1, "message")
// 非编辑器下使用会直接抛出异常
Logger.Print("message");
// flag(ulong)表示只开启特定模块的调试功能，通过Logger.CurDebugFlags设置
Logger.LogDebug("module", flag, "message");
// 下面四组顾名思义对应了不同的LogLevel
Logger.LogVerbose("module", "message");
Logger.LogInfo("module", "message");
Logger.LogWarning("module", "message");
Logger.LogError("module", "message");
// LogException需要手动把要输出的异常传进去
Logger.LogException("module", exc, "message");
// 断言方法则额外需要一个断言条件的参数
Logger.Assert("module", condition, "message");
```

默认情况下，只有出现Exception, Assert, Error三种等级的日志时，才会记录当前的方法调用栈，其他低等级的日志不会记录，这是为了在保留Debug所需必要信息的同时减少不必要的消耗

### 日志回调

任何有效的日志通过Logger类触发时，日志回调事件`Logger`将会被触发，用户可以通过这个来统一对日志信息做额外的处理，事件委托如下：

```csharp
/// <summary>
/// 用于日志的回调委托
/// </summary>
/// <param name="module">日志所属模块</param>
/// <param name="content">日志内容</param>
/// <param name="level">日志等级</param>
/// <param name="stackTrace">堆栈信息</param>
public delegate void LogCallback(string module, string content, LogLevels level, StackTrace stackTrace);
```

## 日志输出到文件

利用上面的日志回调，框架默认实现了一个功能：每次运行游戏时把日志输出到单独的文件并永久保存，通过以下代码开启：

```csharp
// 该功能需要用到对象池，所以在此之前必须先创建ObjectPoolManager的单例
LogToFile.CreateSingleton();
```

日志文件会被保存在persistentDataPath路径下的`GameLog`文件夹中，里面有以创建时间命名的历史日志，以及名为`LatestGameLog.log`的最新日志，每次运行时都会被重新覆盖，作用是可以使用第三方工具实时监视日志变化，而不用切换文件。

### 技术细节

1. 写文件是在单独的线程执行的，所以不会出现IO导致的CPU阻塞。
2. 对于没有使用Logger类输出的日志，同样会被捕获，并会以名为"Unhandled"的LogModule重定向到Logger。所以引擎或其他模块产生的日志也不会在日志文件中漏掉。
3. 日志文件中每一行的格式为：`yyyy/MM/dd HH:mm:ss-[LogLevel][LogModule]-Message`，使用VSCode可以获得良好的语法高亮。
