# Instech.Framework

这里是由inspoy整理的Unity Gameplay框架（或者叫功能合集？）。主要目的是自用，参加GameJam之类的时候可以尽量把精力放在实现玩法上。

大概就是把自己之前整理的公共代码整合为UnityPackage，不再把所有代码全部放到工程目录里，方便自由选取需要的模块，也方便为公共代码做单独的版本管理。

该仓库会不定期同步到Github，开放给需要的同学~

> 注意，本文档并非*API说明*，这类东西我会尽量放到代码注释里面去。

## 主要功能

1. 为配置表和资源加载分别提供了一套简单易用的工作流，每套工作流均有两套实现，一套用在编辑器环境（为了使开发流程保持敏捷），另一套用在打包后的环境（针对加载性能，追加压缩和加密等特性）。
2. 基于`View-Presenter`结构的UI框架，View部分的代码完全来源于自动生成，只需在UI预设上以特定规则对想让程序控制的控件进行命名，代码生成器即会自动生成这些控件的引用，并将自动把View脚本挂接在预设上，同时控件的引用也会自动挂接到View脚本中。
3. 乱七八糟但十分实用的杂项功能，比如`根据权重随机选取数组中的某个元素`之类的。

## 模块命名规律

该仓库下所有包的包名格式均为`cc.inspoy.*`，`*`的部分为全部小写，同时这部分使用Camel命名法作为该包的命名空间，同时在前面加统一前缀`Instech.`。以包`cc.inspoy.framework.common`为例，这个包的根命名空间即为`Instech.Framework.Common`。

`framework`开头的模块属于Unity框架，需要依赖Unity引擎，其他的则不需要，可以使用在.Net Core应用程序。

## 如何获取

如果你是从GitHub上看到的该仓库，直接将其**整个**Clone下来即可，虽然仓库里的框架被细分为了很多模块，但这些模块都是相互依赖的，在大部分情况下，你应该会需要全部的模块。

## 系统要求

当前，所有模块均推荐使用**Unity2020.1**或以上版本，理论上2019甚至2018应该也能在简单修改代码后正常工作，但我并没有进行过相关的测试。

## 如何使用

首先有两个模块没有托管在本仓库，分别是

1. cc.inspoy.encrypthelper - 加密相关，AssetHelper和Data模块会用到
2. cc.inspoy.filepacker - 文件打包，AssetHelper模块会用到

这两个模块托管在GitHub，你需要打开Unity工程中的`Packages/manifest.json`文件，在其`dependencies`字段中新增两行：

```json
{
  "dependencies": {
    "cc.inspoy.encrypthelper": "https://github.com/inspoy/EncryptHelper.git",
    "cc.inspoy.filepacker": "https://github.com/inspoy/FilePacker.git",
    // other packages...
  }
}
```

然后，仓库里的这些模块同样都是符合upm(Unity Package Manager)格式的。接入到项目中后会都会出现在Project窗口的`Packages`目录中。

### 方式1，直接复制

直接把所有以cc.inspoy.framework开头的目录全部复制到项目的Packages目录中，无需对`manifest.json`做更多操作。这样做好处是方便快捷，缺点是不方便及时拿到更新。

### 方式2，使用submodule间接引用

使用git的submodule功能把本仓库嵌套到你自己的git仓库中，在你的`.gitmodules`文件中添加如下代码（其中path可以选一个自己觉得合适的地方）：

```ini
[submodule "Instech.UnityPackages"]
    path = Instech.UnityPackages
    url = https://inspoy.cc/git/inspoy/Instech.UnityPackages.git
```

然后执行`git submodule update --init`初始化并拉取本仓库的内容到本地。这时候要进入子模块中检查下当前的分支，确保当前HEAD处于`upm`分支即可（其他关于submodule的说明不在这里展开）。

然后是重点，打开`manifest.json`文件，我们要把仓库里所有的模块都加入到`dependencies`中：

```json
{
  "dependencies": {
    "cc.inspoy.encrypthelper": "https://github.com/inspoy/EncryptHelper.git",
    "cc.inspoy.filepacker": "https://github.com/inspoy/FilePacker.git",
    "cc.inspoy.framework.assethelper": "file:../../Instech.UnityPackages/cc.inspoy.framework.assethelper",
    "cc.inspoy.framework.common": "file:../../Instech.UnityPackages/cc.inspoy.framework.common",
    "cc.inspoy.framework.core": "file:../../Instech.UnityPackages/cc.inspoy.framework.core",
    "cc.inspoy.framework.data": "file:../../Instech.UnityPackages/cc.inspoy.framework.data",
    "cc.inspoy.framework.logging": "file:../../Instech.UnityPackages/cc.inspoy.framework.logging",
    "cc.inspoy.framework.myjson": "file:../../Instech.UnityPackages/cc.inspoy.framework.myjson",
    "cc.inspoy.framework.tests": "file:../../Instech.UnityPackages/cc.inspoy.framework.tests",
    "cc.inspoy.framework.ui": "file:../../Instech.UnityPackages/cc.inspoy.framework.ui",
    "cc.inspoy.framework.uiwidgets": "file:../../Instech.UnityPackages/cc.inspoy.framework.uiwidgets",
    "cc.inspoy.framework.utils": "file:../../Instech.UnityPackages/cc.inspoy.framework.utils",
    // other packages...
  },
  "testables": [
    "cc.inspoy.framework.tests"
  ]
}
```

其中各个模块的路径根据实际情况修改其相对于`manifest.json`的**相对路径**，然后后面的`testables`字段可选，如果添加了这行，`cc.inspoy.framework.tests`模块中的单元测试就会出现在Unity的TestRunner中。

## 模块列表

* [Core](cc.inspoy.framework.core/README.md) - 框架核心功能，包含单例，对象池
* Logging - 日志模块，所有需要输出日志的模块均依赖这个
* Utils - 公共方法库，有许多实用的工具函数
* Common - 框架基本功能，包含：事件，状态机，常用异步调度器（含定时器）
* AssetHelper - 资源加载相关，主要负责AssetBundle的管理
* Ui - 基于`View-Presenter`的UI框架
* UiWidgets - 扩展UGUI控件
* MyJson - 基于Utf8Json封装，包含Json相关工具方法，AOT用的代码生成
* Data - 配置数据相关，包含配置表，本地存储和本地化
* FilePacker - 将多个文件打包成一个文件，支持选择性部分加载，用于资源打包
* EncryptHelper - 加密相关的使用工具类
