# 1.4.0 (2020.10.21)

## Version

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
