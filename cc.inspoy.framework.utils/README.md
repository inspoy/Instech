# Instech.Framework.Utils

该程序集包含了许多实用工具

## 命令行参数解析

提供了一套简单的接口，用来获取运行游戏exe程序时传递进来的参数信息，会解析成C#友好的数据结构再返回。其中命令行参数有一定的约定，每个参数必须形如`/key:value`(`:value`可省略)才会被解析。用法如下：

```csharp
using Instech.Framework.Utils;
// 首先通过下面的静态方法获取到实例对象
var args = CommandArguments.Get();
// 查看命令行中是否有`/key`参数
print(args.HasKey("key"));
// 获取给`/key`参数指定的参数值
// 如果没有指定参数值则会返回string.Empty
print(args.GetValue("key"));
```

## 运行环境的软硬件信息

可以获取到运行环境简要的软硬件信息，可以用于用户行为统计和分析，以便根据实际的玩家数据对游戏进行针对性的优化

其中硬件信息包括：

* 硬件型号，计算机名，硬件唯一ID
* CPU型号，CPU逻辑线程数，CPU主频
* 内存容量，显卡型号，显存容量

软件信息包括：

* 当前运行平台，网络连接状态
* 操作系统名称，操作系统版本，逻辑驱动器列表
* 系统盘符，系统盘可用空间，系统盘容量
* 游戏安装目录，安装分区剩余空间，安装分区容量
* 设备屏幕分辨率，当前游戏分辨率

## Yield指令对象的缓存

缓存常用的Unity协程的Yield指令，如`WaitForEndOfFrame`等，避免在协程中重复调用`yield return new WaitForEndOfFrame();`之类的语句导致不必要的GC Alloc

## Gzip压缩和解压

简单便捷地对数据进行压缩也是常用的手段（框架中的二进制配置表便是使用本功能压缩后再存储到本地的），这里提供使用Gzip算法对字符串或字节数组压缩的一组接口：

1. 源字符串<->Base64字符串
2. 源字符串<->字节数组
3. 源字节数组<->压缩后的字节数组

## 缓动插值

提供了多种插值方法用于缓动动画，比如线性插值、三次曲线插值等，主要用于UI动画

```csharp
using Instech.Framework.Utils;

Logger.Print(Interpolation.Calc(0.3f, 0, 100, EaseType.CubeInOut));
```

## StringBuilder对象池

利用框架自带的通用对象池，给StringBuilder提供了池化接口：

```csharp
var sb = StringBuilderPool.Acquire();
// do something with sb
sb.ReleaseToPool(); // 回收时会自动调用"sb.Clear()"以供下次使用
```

## 杂项工具函数

详见`Instech.Framework.Utils.Utility`类

1. 随机数
2. 获取当前Unix时间戳
3. 根据名字递归查找子GameObject
4. 计算MD5
5. 返回某Array/List的随机一个元素
6. 根据指定权重获取某ICollection的随机一个元素
7. 返回指定概率的true
8. 把字符串分割成int/float/string数组
9. 获取一个全局唯一的ID，线程安全
10. 计算一个集合的哈希值
11. 获取Transform的层级路径

## [EDITOR]统计打印工程编译时间

每次修改C#代码重新编译后，可以在控制台看到类似这样的日志：`Compiling cost: 6.66s`。

## [EDITOR]自定义定义符号管理器

作用和直接在`ProjectSettings->Player->OtherSettings->ScriptingDefineSymbols`中设置是一样的，这里只是提供了一套API和配套的编辑器窗口，方便用户修改

使用`DefineSymbolManager.AddDefine("MY_SYMBOL");`来添加，重新编译C#工程，代码中的`#if MY_SYMBOL`即可生效。

全部API如下：

```charp
DefineSymbolManager.AddDefine("symbol");
bool succ = DefineSymbolManager.RemoveDefine("symbol");
bool exists = DefineSymbolManager.HasDefine("symbol");
IEnumerable<string> all = DefineSymbolManager.GetAllDefines();
```

## [EDITOR]杂项编辑器工具

1. 自动为源文件生成版权声明用的头部注释
2. 打开Log目录
3. 打开Crash目录
4. 按F1快速切换选定GameObject的Active状态（支持多选，撤销）
5. 查找选定资产的所有引用
