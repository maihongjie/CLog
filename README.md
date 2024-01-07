# CLog SDK



### 方法
#### Init(String, Level, Level)

初始化 CLogger，传入对应的项目ID以及需要收集的日志等级
```c#
public void Init(string appKey, Level fileLogLevel, Level uploadLogLevel);
```
**示例：**

```c#
 CLogger.Instance.Init("appKey", Level.Info, Level.Info);
```
**参数列表：**

- appKey：项目ID
- fileLogLevel：写入本地文件的日志等级
- uploadLogLevel：上报服务器的日志等级



#### INFO(String)

将普通消息保存到本地以及上报服务器
```c#
public void INFO(string context);
```
**示例：**
```c#
 CLogger.Instance.INFO("CLoggerTest");
```

**参数列表：**

- context：消息正文




#### WARN(String)

将警告消息保存到本地以及上报服务器
```c#
public void WARN(string context);
```
**示例：**
```c#
 CLogger.Instance.WARN("CLoggerTest", "TestLog");
```

**参数列表：**

- context：消息正文



#### ERROR(String)

将错误消息保存到本地以及上报服务器
```c#
public void ERROR(string context);
```
**示例：**
```c#
 CLogger.Instance.ERROR("CLoggerTest", "TestLog");
```

**参数列表：**

- context：消息正文



#### EXCEPTION(String)

将异常消息保存到本地以及上报服务器
```c#
public void EXCEPTION(string context);
```
**示例：**
```c#
 CLogger.Instance.EXCEPTION("CLoggerTest", "TestLog");
```

**参数列表：**

- context：消息正文



### 属性

##### EnableSave 

将调用CLogger接口发送的消息保存到本地，设为false时所有调用CLogger的接口不会做保存操作，默认为true
```c#
public bool EnableSave { set; }
```
**示例：**
```c#
 CLogger.Instance.EnableSave = false;
```



##### EnableUpload 

将调用CLogger接口发送的消息上报到服务端，设为false时所有调用CLogger的接口不会做上报操作，默认为false
```c#
public bool EnableUpload { set; }
```
**示例：**
```c#
 CLogger.Instance.EnableUpload = false;
```



##### SetFileLogLevel

设置写入本地文件的日志等级，共有五个级别Info、Warn、Error、Exception、None，默认为Info
```c#
public Level SetFileLogLevel { get; set; }
```
**示例：**

```c#
 CLogger.Instance.SetFileLogLevel =  CLogger.Instance.Level.Error;
```



##### SetUploadLogLevel

设置上报服务器的日志等级，共有五个级别Info、Warn、Error、Exception、None，默认为Info

```c#
public Level SetUploadLogLevel { get; set; }
```

**示例：**

```c#
 CLogger.Instance.SetUploadLogLevel =  CLogger.Instance.Level.Error;
```





##### ProjectVersion

项目版本
```c#
public string ProjectVersion { get; set; }
```
**示例：**

```c#
 CLogger.Instance.ProjectVersion = "1.0.0";
```



##### ChanneId

渠道id
```c#
public string ChanneId { get; set; }
```
**示例：**

```c#
 CLogger.Instance.ChanneId = "null";
```



##### BranchTag

主干/分支
```c#
public string BranchTag { get; set; }
```
**示例：**

```c#
 CLogger.Instance.BranchTag = "Trunk";
```



##### PlayerAccount

游戏账号
```c#
public string PlayerAccount { get; set; }
```
**示例：**

```c#
 CLogger.Instance.PlayerAccount = "123";
```

##### MapName

游戏场景
```c#
public string MapName { get; set; }
```
**示例：**

```c#
 CLogger.Instance.MapName = "Login";
```



