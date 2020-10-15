# Instech.Framework.Core

该程序集包含了单例和对象池的实现

## 单例

这里一共有三种单例，分别是`Singleton`，`AutoCreateSingleton`和`MonoSingleton`

### Singleton和AutoCreateSingleton

首先是最普通常规的单例实现，代码如下：

```csharp
using Instech.Framework.Core;

public class Foo : Singleton<Foo>
{
    protected override void Init()
    {
        // your code for initialization
    }

    protected override void Deinit()
    {
        // your code for deinitialization
    }
}
```

其中`Deinit`方法是可选的，基类中有个默认的空实现，`Init`则是必须实现的。

> 在这套框架中，任何实际作用相反的接口，应当采用正确的反义词来命名，如`Create/Destroy`，`Acquire/Release`等。
> 对于没有明确/常用反义词的，在其前面添加"De-"或者"Un-"前缀作为反义词，其中动词使用"De-"，形容词使用"Un-"，如`Init/Deinit`。

这种普通单例在使用前，必须手动显式调用`Foo.CreateSingleton()`方法来创建单例对象（此时会调用`Init()`方法），销毁时则应当调用`Foo.DestroySingleton()`（此时则会调用`Deinit()`方法）。其余时候，使用静态属性`Foo.Instance`来获取单例的唯一实例对象。

显式调用"Create"和"Destroy"是为了更好地控制对象的生命周期，否则在项目规模扩大后，可能会出现某单例已经被销毁，但在之后发生的某些逻辑中又被重新创建的情况。当然为了方便起见，框架还提供了无需显式创建的单例模板（当然手动调`CreateSingleton`也完全没有问题）`AutoCreateSingleton`，用法和`Singleton`完全一致。

### MonoSingleton

然后是Unity特色：`MonoSingleton`，用法同普通单例，区别在于这是继承自`MonoBehaviour`的。内部创建时，会在Unity场景中创建一个名为`[Singleton]<TYPE_NAME>`（`<TYPE_NAME>`为类型全名）的GameObject，然后把自己作为组件挂在它上面。

`MonoSingleton`的特点在于它继承于`MonoBehaviour`，所以会自带Update方法，非常方便。但是我们知道`MonoBehaviour`的Update执行顺序是没有保证的，虽然可以设置但终究还是有些繁琐，另外引擎调用Unity事件本身就有额外的消耗，所以还是尽量少用。

## 通用对象池

### 常规用法

新建一个类，实现`IPoolable`接口即可，参考下面的样例代码：

```csharp
public class Foo : IPoolable
{
    public int SomeProperty { get; set; }

    public void OnActivate()
    {
        SomeProperty = 123;
    }

    public void OnDestroy()
    {
        // do nothing
    }

    public void OnRecycle()
    {
        // do nothing
    }
}
```

其中，`OnActivate()`会在对象被激活（从池里拿出来或新创建）时被调用，可以用来做一些初始化相关的事情；`OnRecycle`顾名思义会在对象入池时调用，用来清理一些临时性的状态；最后`OnDestroy`会在对象被彻底销毁（不会入池）时调用，用来清理必要的资源。

使用时，可以通过`ObjectPool<Foo>.Instance.Get()`来获取一个新的对象，如果池里有缓存则会从对象池里拿出一个来复用，如果没有的话则会新创建一个。

使用完毕后，通过`ObjectPool<Foo>.Instance.Recycle()`来把对象进行回收，如果已有的缓存数量已达上限，这个对象则会被直接销毁，否则缓存进对象池。该上限可通过`ObjectPool<Foo>.Instance.MaxCount`进行设置。

为了精简代码，这里还提供了一些简化写法，如下：

```csharp
// Foo还是上面定义的那个
// 创建对象 - 标准写法
var obj1 = ObjectPool<Foo>.Instance.Get();
// 创建对象 - 简化写法
var obj2 = ObjectPool<Foo>.GetNew();

// 回收对象 - 标准写法
ObjectPool<Foo>.Instance.Recycle(obj1);
// 回收对象 - 简化写法
obj2.Recyle();
```

### 对一般对象的支持

显然，`IPoolable`接口只适用于自定义的对象，对于系统或框架自带的类型，能不能用上面这套通用的对象池呢？当然是可以的，用户只需准备好`IPoolable`接口中的三个方法，按照下面的方式定义，就能愉快地使用了。

```csharp
// 这里以StringBuilder为例，我们希望回收时将其清空
ObjectPool<StringBuilder>.Instance.RecycleCallback = (StringBuilder sb) =>
{
    sb?.Clear();
};
// 另外两种方法我们虽然不需要，但还是要设置一个空方法，推荐使用现成的EmptyCallback
ObjectPool<StringBuilder>.Instance.ActivateCallback = ObjectPool<StringBuilder>.Instance.EmptyCallback;
ObjectPool<StringBuilder>.Instance.DestroyCallback = ObjectPool<StringBuilder>.Instance.EmptyCallback;
```

然后还是一样用`ObjectPool<StringBuilder>`的`Get`和`Recyle`方法，跟其他对象池是完全一样的逻辑。

### 彩蛋

ObjectPool类还提供了4种统计接口，可以随时查看这个对象池的工作状态，通过这些数据或许可以提供一些优化参考

|Name|Description|
|-|-|
|CreatedCount|通过对象池创建的对象数量|
|PooledCount|对象池当前已缓存的对象数量|
|ActiveCount|对象池当前正在使用的（没在池里）对象数量|
|SavedCount|通过对象池再利用的对象数量|
