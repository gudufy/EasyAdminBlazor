# EasyAdminBlazor 入门

## 一、项目概述
### 项目简介
`EasyAdminBlazor` 是一个基于 .NET 9.0 的项目，用于开发管理后台相关功能。此项目运用 `BootstrapBlazor` 组件库搭建用户界面，借助 `FreeSql` 进行数据访问操作，前台使用Razor pages，可快速完成中小型项目，个人接单利器。

### 技术栈
#### 后端
- .NET 9.0
- FreeSql

#### 前端
- BootstrapBlazor

## 二、依赖管理
### NuGet 包依赖
项目依赖众多 NuGet 包，部分核心依赖如下：

| 包名                          | 版本  | 描述                                                         |
|-------------------------------|-------|--------------------------------------------------------------|
| `BootstrapBlazor`             | 9.6.1 | 基于 Bootstrap 的 Blazor 组件库，用于构建用户界面             |
| `FreeSql.Extensions.AggregateRoot` | 3.5.205 | FreeSql 的聚合根扩展，方便进行领域驱动设计的数据操作         |
| `NETCore.MailKit`             | 2.1.0 | 用于在 .NET Core 项目中发送邮件的库                           |
| `Rougamo.Fody`                | 5.0.0 | 基于 Fody 的 AOP 框架，用于实现面向切面编程                   |

### 前台依赖
项目在 `wwwroot/lib` 目录下包含一些前端库：
- **jQuery 3.7.1**：快速、简洁的 JavaScript 库，简化 HTML 文档遍历、事件处理、动画和 Ajax 操作。
- **jQuery Validation**：用于表单验证的 jQuery 插件。
- **jQuery Validation Unobtrusive**：为 jQuery Validation 提供非侵入式验证支持。

## 三、运行说明
### 运行环境
- **.NET SDK**：9.0

### 运行步骤
1. **恢复 NuGet 包**：在项目根目录下执行以下命令恢复 NuGet 包。
```bash
dotnet restore
```

2. **启动项目**：在EasyAdminBlazor.Test目录下执行以下命令启动项目。
```bash
dotnet run
```

3. **访问应用**：在浏览器中访问 `http://localhost:5207` 即可查看应用。

### 或者vs2022打开EasyAdminBlazor.sln，然后在EasyAdminBlazor.Test项目下的wwwroot文件夹右键“在浏览器中查看”