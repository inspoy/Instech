# 1.10.0 (2021.11.15)

## Versions

- AssetHelper: `1.2.0`
- Common: `1.4.1`
- Core: `1.1.2`
- Data: `1.3.0`(was `1.2.0`)
- Gameplay: `1.1.0`(was `1.0.0`)
- Logging: `1.1.3`
- MyJson: `1.1.0`
- Tests: `1.0.1`
- UI: `1.6.0`(was `1.5.0`)
- UiWidgets: `1.2.3`
- Utils: `1.7.0`(was `1.6.0`)

## New Feature

- [Gameplay]增加GameModule，用于划分功能模块以及逻辑/数据分离

## Bugfix

- [Gameplay]退出游戏时可能会报错（调整了单例卸载顺序）

---

# 1.9.0 (2021.10.26)

## Versions

- AssetHelper: `1.2.0`
- Common: `1.4.1`
- Core: `1.1.2`
- Data: `1.3.0`(was `1.2.0`)
- Gameplay: `1.0.0`(new)
- Logging: `1.1.3`
- MyJson: `1.1.0`
- Tests: `1.0.1`
- UI: `1.6.0`(was `1.5.0`)
- UiWidgets: `1.2.3`
- Utils: `1.7.0`(was `1.6.0`)

## Highlight

- 增加样例工程
- [Gameplay]增加Gameplay模块，把游戏公共的gameplay代码提了出来

## New Feature

- [Utils]增加一个随机打乱List的功能
- [UI]新增一个自动设置CameraStack的组件

## Optimizing

- [UI]Canvas渲染方式改成摄像机渲染
- [UI]优化了导出UI代码的逻辑
- [Data]Excel配置表的路径变成可配置的

## Bugfix

- [Data]本地化UI控件在退出游戏时可能会报错
- [Data]本地存储模块在保存后没有去除脏标记
- [Data]处理了几处错别字
- [Utils]Assert.AreNotEqual判断写反了

## Misc

- [Misc]Unity版本升级到2020.3

---

# 1.8.0 (2021.3.10)

## Versions

- AssetHelper: `1.2.0`
- Common: `1.4.1`(was `1.4.0`)
- Core: `1.1.2`
- Data: `1.2.0`(was `1.1.0`)
- Logging: `1.1.3`
- MyJson: `1.1.0`
- Tests: `1.0.1`
- UI: `1.5.0`(was `1.4.0`)
- UiWidgets: `1.2.3`(was `1.2.2`)
- Utils: `1.6.0`(was `1.5.0`)

## BREAKING CHANGE

- [Data]配置表生成代码结构调整，利用分部类(`partial class`)将自动生成部分和自定义部分分离

## Highlight

- [UI]新增UI代码导出对预设变体的支持

## New Feature

- [UI]新增鼠标悬停检测组件
- [Utils]新增了一组断言短语集合
- [Utils]新增FastStartsWith，比FCL自带的快不少

## Optimizing

- [Common]新建项目设置资产时确保目录存在
- [UiWidget]增加ClickMask对CanvasRenderer的依赖
- [Utils]StringBuilderPool加了一个返回字符串并自动释放的接口

## Bugfix

- [UI]修复UI预设嵌套的情形不能正常导出的问题
- [Utils]修复某些情况下不能顺利生成头部注释的问题

---

# 1.7.0 (2021.1.29)

## Versions

- AssetHelper: `1.2.0`(was `1.1.2`)
- Common: `1.4.0`(was `1.3.0`)
- Core: `1.1.2`
- Data: `1.1.0`(was `1.0.4`)
- Logging: `1.1.3`(was `1.1.2`)
- MyJson: `1.1.0`(was `1.0.2`)
- Tests: `1.0.1`
- UI: `1.4.0`(was `1.3.1`)
- UiWidgets: `1.2.2`(was `1.2.1`)
- Utils: `1.5.0`(was `1.4.1`)

## BREAKING CHANGE

- [Ui]BasePresenter接口改名

## Highlight

- [AssetHelper]AssetManager增加了自动卸载的功能
- [Data]本地化新增语言切换和重载本地化表的功能

## New Feature

- [Common]新增线程管理器
- [Data]ConfigManager新增一个HasId的接口
- [Data]导出配置表时支持忽略行/列
- [Logging]增加了Profile开关
- [MyJson]接入了LitJson作为默认库
- [UiWidgets]ClickMask加了个编辑器面板提示
- [Utils]增加了若干工具函数

## Optimizing

- [Common]新增BasicEventData，实现了不常用的接口
- [Common]EventDispatcher里，把listener改成了owner

---

# 1.6.0 (2020.12.7)

## Versions

- AssetHelper: `1.1.2`(was `1.1.1`)
- Common: `1.3.0`(was `1.2.1`)
- Core: `1.1.2`(was `1.1.1`)
- Data: `1.0.4`(was `1.0.3`)
- Logging: `1.1.2`(was `1.1.1`)
- MyJson: `1.0.2`(was `1.0.1`)
- Tests: `1.0.1`(was `1.0.0`)
- UI: `1.3.1`(was `1.3.0`)
- UiWidgets: `1.2.1`(was `1.2.0`)
- Utils: `1.4.1`(was `1.4.0`)

## Highlight

- 文件头部注释格式修改，从`/**/`改为`///`

## New Feature

- [Common]增加了Coroutine

## Optimizing

- [Core]可池化对象提供一个空基类
- [Common]EventDispatcher增加了派发耗时过高的警告

---

# 1.5.0 (2020.11.6)

## Versions

- AssetHelper: `1.1.1`
- Common: `1.2.1`(was `1.2.0`)
- Core: `1.1.1`
- Data: `1.0.3`(was `1.0.2`)
- Logging: `1.1.1`
- MyJson: `1.0.1`
- Tests: `1.0.0`
- UI: `1.3.0`(was `1.2.1`)
- UiWidgets: `1.2.0`
- Utils: `1.4.0`(was `1.3.0`)

## BREAKING CHANGE

- [Utils]FindChildWithName方法的主体和返回值由"GameObject"变为"Transform"

## Highlight

- [UI]新增常见补间动画

## New Feature

- [Utils]增加Transform根据路径获取子节点的方法
- [Utils]GzipHelper增加接口：byte array=>base64 string

## Optimizing

- [Common]ReactiveProperty增加隐式类型转换方法
- [UI]BaseView增加公共属性InitData
- [Data]本地化遇到空值时视为不存在
- [Data]本地化导出时不导出空值

## Bugfix

- 整体引用的包名改成"cc.inspoy.framework.package"，因为".framework"结尾的包不会被Unity2020.1正确导入
- [Common]EventDispatcher.AddListener的判断逻辑有误
- [UI]修复退出游戏时UI报错的问题

---

# 1.4.0 (2020.10.21)

## Versions

- AssetHelper: `1.1.1`
- Common: `1.2.0`(was `1.1.1`)
- Core: `1.1.1`
- Data: `1.0.2`(was `1.0.1`)
- Logging: `1.1.1`(was `1.1.0`)
- MyJson: `1.0.1`(was `1.0.0`)
- Tests: `1.0.0`
- UI: `1.2.1`(was `1.2.0`)
- UiWidgets: `1.2.0`(was `1.1.0`)
- Utils: `1.3.0`(was `1.2.0`)

## Highlight

- [UI]新增循环列表LoopedScrollView

## BREAKING CHANGE

- [Utils]插值函数统一成一个接口，通过参数区分
- [Utils]移除了之前的测试代码

## New Feature

- [Common]统一返回指定框架package的绝对路径的接口
- [Utils]插值函数新增圆弧插值
- [Utils]F1快速切换显隐支持多选和撤销

## Optimizing

- [Common]ReactiveProperty增加一个可以指定初始值的构造函数
- [Logging]使用StringBuilder优化LogToFile性能

## Doc & Misc

- 新增Common的文档
- 新增Logging的文档
- 新增Utils的文档

---

# 1.3.2 (2020.10.15)

## Versions

- AssetHelper: `1.1.1`
- Common: `1.1.1`(was `1.1.0`)
- Core: `1.1.1`(was `1.1.0`)
- Data: `1.0.1`
- Logging: `1.1.0`
- MyJson: `1.0.0`
- Tests: `1.0.0`
- UI: `1.2.0`
- UiWidgets: `1.1.0`
- Utils: `1.2.0`

## Optimizing

- [Common]EventDispatcher增加了判断：如果传入回调不是一般的类方法，则必须指定监听者

## Doc & Misc

- [Doc]Core新增对象池的文档

---

# 1.3.1 (2020.9.27)

## Versions

- AssetHelper: `1.1.1`
- Common: `1.1.0`
- Core: `1.1.0`
- Data: `1.0.1`
- Logging: `1.1.0`
- MyJson: `1.0.0`
- Tests: `1.0.0`
- UI: `1.2.0`
- UiWidgets: `1.1.0`
- Utils: `1.2.0`

## Doc & Misc

- 增加meta文件
- 修正package.json的格式
- [Doc]Readme新增整体引用的接入方式
- [Doc]Core新增单例的文档

---

# 1.3.0 (2020.9.23)

## Versions

- AssetHelper: `1.1.1`(was `1.1.0`)
- Common: `1.1.0`(was `1.0.0`)
- Core: `1.1.0`
- Data: `1.0.1`
- Logging: `1.1.0`(was `1.0.1`)
- MyJson: `1.0.0`
- Tests: `1.0.0`
- UI: `1.2.0`(was `1.1.0`)
- UiWidgets: `1.1.0`(was `1.0.0`)
- Utils: `1.2.0`(was `1.1.0`)

## Highlight

- 为整个仓库配置了一个package，打包了所有的模块

## BREAKING CHANGE

- [UI]把ProgressBar迁移到UiWidgets模块
  - 命名空间从`Instech.Framework.UI`改为了`Instech.Framework.UiWidgets`
- [Utils]修改CombineToString接口，取消泛型参数

## New Feature

- [Common]定时器增加一个DelayCall，相当于AddTimer简化参数后的别名
- [Common]所有定时器都返回唯一ID
- [UI]UiManager增加CloneView接口
- [UiWidgets]增加点击事件穿透组件
- [UiWidgets]增加任意多边形点击区域组件

## Optimizing

- [AssetHelper]清理代码异味
- [UI]把UiExtention整理到单独的目录
- [UI]更新代码生成模板（增加Region，美化）
- [Utils]清理代码异味

## Bugfix

- [Logging]去掉了花里胡哨的预定义符号判断（在其他文件不起作用）
- [Logging]DebugFlags默认为1，否则在默认情况下Print不起作用

---

# 1.2.0 (2020.9.15)

## Versions

- AssetHelper: `1.1.0`(was `1.0.0`)
- Common: `1.0.0`
- Core: `1.1.0`
- Data: `1.0.1`(was `1.0.0`)
- Logging: `1.0.1`(was `1.0.0`)
- MyJson: `1.0.0`
- Tests: `1.0.0`
- UI: `1.1.0`(was `1.0.0`)
- UiWidgets: `1.0.0`
- Utils: `1.1.0`(was `1.0.1`)

## Highlight

- 增加整个仓库的整体版本号（体现在CHANGELOG中）

## New Feature

- [AssetBuilder]新增生成构建报告
- [UI]新增嵌套SubView的导出支持
- [Utils]新增预定义宏编辑器
- [Logging]相关接口增加条件编译

## Optimizing

- [Data]ConfigManager的GetAll接口始终返回非空结果

---

# 1.1.0 (2020.8.21)

## Versions

- AssetHelper: `1.0.0`
- Common: `1.0.0`
- Core: `1.1.0`(was `1.0.0`)
- Data: `1.0.0`
- Logging: `1.0.0`
- MyJson: `1.0.0`
- Tests: `1.0.0`
- UI: `1.0.0`
- UiWidgets: `1.0.0`
- Utils: `1.0.1`(was `1.0.0`)

## New Feature

- [Core]对象池新增了对于普通对象（没有实现IPoolable接口）的支持
- [Tests]增加Logging模块的覆盖测试

---

# 1.0.0 (2020.8.19)

## Versions

- AssetHelper: `1.0.0`
- Common: `1.0.0`
- Core: `1.0.0`
- Data: `1.0.0`
- Logging: `1.0.0`
- MyJson: `1.0.0`
- Tests: `1.0.0`
- UI: `1.0.0`
- UiWidgets: `1.0.0`
- Utils: `1.0.0`

## Highlight

- Update all package version to 1.0.0

## BREAKING CHANGE

- `Instech.Logging.Logger.DebugFlags`'s type has been changed to `ulong`, which previously is `HashSet<int>`

## New Feature

- [Tests]Add tests package(`cc.framework.core` only yet)
- [Data]Write unencrypted LocalStorage file in Editor environment(for debugging)
- [Data]Add a thread-safe method for `ConfigManager.Get`
- [UI]Add general InitData for UI(passed in AddView and AddItemToView)
- [Utils]Add a method to get md5 in byte array
- [UiWidgets]Create package

## Optimizing

- [Utils]Change way to get assembly name by source flie name
- [Utils]Cache user name to avoid generate user name every time
- [Misc]Clean up code smell

## Bugfix

- [AssetHelper]Fix path error in AssetBuilder
- [UI]Fix bugs in UiGenerator(Generated code cannot be compiled)

---

# 0.0.1 (2020.7.24)

> Very first public version after refactoring to upm(Unity Package Manager)
