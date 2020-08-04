# Instech.UnityPackages

把自己之前整理的公共代码整合为UnityPackage，不再把所有代码全部放到工程目录里，方便自由选取需要的模块，也方便为公共代码做单独的版本管理。

该仓库会不定期同步到Github，开放给需要的同学~

## 命名规律

该仓库下所有包的包名格式均为`cc.inspoy.*`，`*`的部分为全部小写，同时这部分使用Camel命名法作为该包的命名空间，同时在前面加统一前缀`Instech.`。以包`cc.inspoy.framework.common`为例，这个包的根命名空间即为`Instech.Framework.Common`。

`framework`开头的模块属于Unity框架，需要依赖Unity引擎，其他的则不需要，可以使用在linux服务端。

# 模块列表

详细说明请见每个子目录中的README文档（施工中）

## Instech.Framework.Core

框架核心功能，包含单例，对象池

## Instech.Framework.Logging

日志模块，所有需要输出日志的模块均依赖这个

## Instech.Framework.Utils

公共方法库，有许多实用的方法，一般会被所有其他包引用

## Instech.Framework.Common

框架基本功能，包含：
* 事件
* 状态机
* 常用异步调度器

## Instech.Framework.AssetHelper

资源加载相关，主要负责AssetBundle的管理

## Instech.Framework.Ui

UI框架

## Instech.Framework.UiWidgets

扩展UGUI控件

## Instech.Framework.MyJson

基于Utf8Json封装，包含Json相关工具方法，AOT用的代码生成

## Instech.Framework.Data

配置数据相关，包含配置表和本地化

# 其他模块

除了本仓库，还有其他一些不依赖Unity的模块，如下

## Instech.FilePacker

将多个文件打包成一个文件，支持选择性部分加载，用于资源打包

地址：[FilePacker on Github](https://github.com/inspoy/FilePacker)

## Instech.EncryptHelper

加密相关的使用工具类

地址：[EncryptHelper on Github](https://github.com/inspoy/EncryptHelper)
