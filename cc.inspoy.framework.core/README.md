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
