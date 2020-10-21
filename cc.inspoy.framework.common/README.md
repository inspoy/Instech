# Instech.Framework.Common

该程序集包含：事件系统，游戏状态机，定时器，编辑器下的项目设置

## 事件系统

和现在项目中常见的“由全局事件管理器派发所有事件”不同，本框架内的事件派发器并非单例，可以通过**组合**的模式加在任意想增加事件功能的类中。

```csharp
public class Foo
{
    public EventDispatcher Dispatcher { get; private set; }
}
```

这样一来，不同的派发器就只需要关心注册在自己身上的监听者了；同样的事件可以注册在不同的派发器上以实现不同的功能，以框架自带的`ReactiveProperty`对象为例，简单用法如下：

```csharp
// 准备事件回调方法
void valueChanged(Event e)
{
    var data = e.GetData<ValueChangeData<int>>();
    print($"Value changed from {data.OldValue} to {data.Value}");
}

// 定义一个“响应式属性”（值变化时触发事件），其基类型为int，初始值为123
var prop1 = new ReactiveProperty<int>(123);

// ReactiveProperty类中有一个Dispatcher对象，往上边加一个事件监听
prop1.Dispatcher.AddListener(EventEnum.ReactivePropChange, valueChanged);

// 此时便会调用Dispatcher对象的DispatchEvent方法，进而触发valueChangd回调
prop1.Value = 321;
```

### Event类

所有的事件回调的签名都必须是`void XXX(Event e)`，其中Event类中封装了所有与该事件有关的东西。

首先，Event类实现`IPoolable`接口，是可以被池化的，当然一般情况用户不用去关心其生命周期，新的Event对象会在派发事件之前从池中获取，全部派发完毕后会自动地被回收到对象池中。

通过`string EventType`属性可以知道该事件的类型，`object Target`属性可以获取到该事件的派发者（派发器所属的对象）。

通过`GetData<T>()`方法可以拿到事件附加的数据，这个方法内部有判断，如果事件本身不存在附加数据，或者泛型参数T和附加数据的类型不匹配，都会输出警告并返回null。需要注意的是，上述判断只在编辑器环境中生效，非编辑器环境是没有警告输出的（但还是会返回null，不会出现异常）

所有的事件数据类都实现`IEventData`接口和`IPoolable`接口，用户创建自定义事件数据时，需要定义池化相关的逻辑，例如在`OnRecycle`中执行必要的回收逻辑。

> 所有`RecycleData`方法的实现都是同样的一句话：`this.Recycle();`，这里是为了把泛型参数传递给对象池，所以暂时不得不用这种方式。。

## 游戏状态机

游戏状态`IGameState`是对于Gameplay层的一种抽象，通过对游戏运行过程进行合理的划分，把特定对象的生命周期明确化，减少不必要的耦合

例如，举个简单的例子，我们把某个小游戏分割为了标题状态和游戏状态，那么在游戏状态中才会有的状态数据（比如血量金币之类的）应该在进入游戏状态时进行一次初始化，并在离开游戏状态时清空容器，释放资源

最立竿见影的效果就是实现“重新开始”功能时就非常容易了，执行一次"游戏状态"到"游戏状态"的转换就行了，基本不会出现有数据没有重置的情况（在GameJam开发小游戏的场景中非常实用）

同时，`IGameState`接口声明了`UpdateFrame(float dt)`方法，本状态内的各种gameplay逻辑的帧更新(`Update()`)方法也应该都由这个方法来驱动，统一管理，确保执行顺序，还减少了MonoBehaviour自身的消耗

### 使用游戏状态机

首先定义游戏状态，实现`IGameState`接口：

```csharp
public class InGameState : IGameState
{
    void OnStateEnter(string lastState) {}
    void OnStateLeave(string nextState) {}
    void UpdateFrame(float dt) {}
    void UpdateLogic(float dt) {}
}
```

然后在游戏初始化时将这个状态注册进状态机（状态机是单例对象），之后执行`ChangeState()`方法即可在所有已定义的状态之间进行切换了。理论上只有一个状态也是可以工作的，切换到和当前状态相同的状态时，会重置当前状态的状态（第一个状态指游戏状态，第二个状态指该游戏状态中包含的状态数据）

```csharp
GameStateMachine.CreateSingleton();
GameStateMachine.Instance.RegisterGameState(new InGameState());

GameStateMachine.Instance.ChangeState(typeof(InGameState));
```

## 定时器

支持像`Scheduler.AddTimer(action, interval, loopTimes)`这样的通用定时器，也提供了一组语义化的简化写法，如下

```csharp
// 下一帧执行（所有的定时器回调都在LateUpdate过程中被调用）
Scheduler.NextFrame(()=>{/* Do something */});
// 每一帧都执行（相当于注册了一个LateUpdateFrame）
Scheduler.EveryFrame(()=>{/* Do something */});
// 延迟一段时间执行
Scheduler.DelayCall(()=>{/* Do something */}, 1.0f);
```

以上所有的接口都会返回一个定时器ID，用户可以通过这个ID来移除某个定时器，接口为`Scheduler.RemoveTimer(timerId)`。除此之外，还可以通过指定回调方法来移除定时器，但这个方法会对所有定时器进行遍历，所以还是推荐使用ID来移除。

## 项目设置

这个就是把各种路径做成配置，以`ScriptableObject`的形式保存在工程中。目前有配置如下路径（都是相对Assets）：

1. 美术资源根目录，默认为"/Artwork/"
2. UI代码导出路径 ，默认为"/Scripts/UI/"
3. Excel配置表路径，默认为"/../../Documents/GameConfig/"

其中前两个基本不用改，第三个配置表路径根据实际情况可能会有变化
