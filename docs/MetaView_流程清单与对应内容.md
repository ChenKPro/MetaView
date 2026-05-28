# MetaView 流程清单与对应内容

本文按当前软件实现整理各类流程、入口、配置内容、执行步骤、调用能力和输出结果。当前版本以 Demo 可运行链路为主，真实硬件链路已有接口和部分 Foundation 适配，但仍需要进一步完善设备会话管理和真实采集流程。

## 1. 流程总览

| 流程 | 当前入口 | 核心对象 | 当前状态 |
| --- | --- | --- | --- |
| 软件启动流程 | `MetaView.App` | Prism Shell / DI / Region | 已完成平台壳启动 |
| 设备参数加载流程 | `JsonRuntimeParameterProvider` | `metaview.devices.json` | 已完成基础配置读取 |
| 运动控制初始化流程 | `HardwarePanelViewModel` / Workflow | `IMotionControlCapability` | Demo 可运行，真实控制器创建链路已有 |
| 明场相机 Live 流程 | `LivePreviewCommand` | `IBrightfieldCameraCapability` | Demo 可显示，真实相机创建链路已有 |
| 明场相机 Capture 流程 | `SingleCaptureCommand` | `IBrightfieldCameraCapability` | Demo 可采集 |
| SRS 2D Demo 流程 | `RunDemoWorkflowCommand` | `SrsTwoDDemoWorkflow` | 可运行 Demo |
| SRS + 明场多模态流程 | `RunDemoWorkflowCommand` | `MultimodalImagingWorkflow` | 可运行 Demo 骨架 |
| 图像鼠标位移台联动流程 | `ImageViewer2D` 鼠标事件 | `IMotionControlCapability` | Stage 模式已有，中键临时模式待补齐 |
| Log 输出流程 | `WorkflowLogPublisher` | Prism EventAggregator | 已接入 UI |
| 图像/曲线刷新流程 | `RealtimeSignalImagingService` / Camera Event | Prism Events | 已接入 ImageDisplay / Signal Plot |

## 2. 软件启动流程

### 入口

```text
MetaView/App.xaml.cs
```

### 执行内容

1. Prism 创建应用。
2. `RegisterTypes()` 注册平台服务和能力。
3. `CreateShell()` 创建 `MetaView.Presentation.MainWindow`。
4. `OnInitialized()` 注册 Prism Region。
5. `TopBar / Workspace / Hardware / Acquisition / Status` 自动进入对应 Region。

### 关键注册

```text
MetaView/Composition/MetaViewContainerRegistration.cs
```

主要内容：

- 注册 `IRuntimeParameterProvider`
- 注册各 Capability
- 注册 Workflow 服务
- 注册 Presentation ViewModel
- 注册主窗口 `MetaView.Presentation.MainWindow`

### 输出

- 启动 MetaView 主界面。
- 加载 Prism Region。
- 左侧导航、中央图像、右侧设备和采集面板可见。

## 3. 参数加载流程

### 入口

```text
MetaView.Capability.ParameterManagement/Providers/JsonRuntimeParameterProvider.cs
```

### 配置文件

```text
config/metaview.devices.json
```

### 当前配置内容

| 配置节点 | 作用 |
| --- | --- |
| `motionSystem` | 运动控制器、轴绑定、Demo/真实模式 |
| `daq` | DAQ Demo/真实配置路径 |
| `brightfieldCamera` | 明场相机类型、ID、曝光、增益、ROI、触发 |
| `imageStageNavigation` | 图像像素到位移台移动的标定关系 |
| `laser` | 激光类型、设备 ID、功率、预热、发射状态 |

### 典型读取调用

```csharp
GetMotionSystemConfiguration()
GetDaqRuntimeConfiguration()
GetBrightfieldCameraSettings()
GetImageStageNavigationSettings()
GetLaserRuntimeSettings()
```

### 输出

- 返回统一 `OperationResult<T>`。
- 供 Capability 初始化和 Workflow Preflight 使用。

## 4. 运动控制流程

### 入口 1：手动控制面板

```text
MetaView.Presentation/ViewModels/HardwarePanelViewModel.cs
MetaView.Presentation/Views/HardwarePanelView.xaml
```

### UI 操作

| 按钮/控件 | 当前绑定 |
| --- | --- |
| `X+` | `MoveXPositiveCommand` |
| `X-` | `MoveXNegativeCommand` |
| `Y+` | `MoveYPositiveCommand` |
| `Y-` | `MoveYNegativeCommand` |
| `Z+` | `MoveZPositiveCommand` |
| `Z-` | `MoveZNegativeCommand` |
| `STOP` | `StopCommand` |
| `Save` | 保存当前位置 |
| `Pos` | 回到保存位置 |
| `Unload` | 移动到卸载位 |
| `XY Step` | XY 相对移动步长 |
| `Z Step` | Z 相对移动步长 |

### 调用能力

```csharp
IMotionControlCapability.InitializeAsync()
IMotionControlCapability.StartMonitoringAsync()
IMotionControlCapability.MoveRelativeAsync()
IMotionControlCapability.MoveAbsoluteAsync()
IMotionControlCapability.StopAsync()
```

### 当前位置反馈

运动 Capability 发布：

```text
MotionStatusChangedEvent
```

UI 更新：

```text
X / Y / Z / Status / StageConnected
```

### 入口 2：Workflow

```text
MetaView.Services/ExperimentCapabilityInvoker.cs
```

Workflow 通过：

```csharp
MoveRelativeAsync(MotionAxis.X, distance)
MoveRelativeAsync(MotionAxis.Y, distance)
MoveRelativeAsync(MotionAxis.Z, distance)
```

调用运动台。

### 当前状态

- Demo 运动控制可运行。
- `CompositeMotionControlCapability` 已有真实控制器创建链路。
- 当前配置 `useDemo = true`，实际默认使用 Demo。

## 5. 明场相机 Live 流程

### 入口

```text
MetaView.Presentation/ViewModels/AcquisitionWorkflowViewModel.cs
```

命令：

```text
LivePreviewCommand
```

### 执行步骤

1. UI 点击 `Live`。
2. 状态切换为 `LivePreview`。
3. 读取 `GetBrightfieldCameraSettings()`。
4. 调用 `IBrightfieldCameraCapability.InitializeAsync(settings)`。
5. 调用 `IBrightfieldCameraCapability.StartLiveAsync()`。
6. 相机 Capability 发布 `BrightfieldCameraFramePublishedEvent`。
7. `ImageWorkspaceViewModel` 接收事件，更新 `LiveImageSource`。
8. 图像显示到 `ImageViewer2D`。
9. Log 输出相机状态。

### 调用链

```text
Live Button
  -> AcquisitionWorkflowViewModel.StartLiveAsync()
  -> IRuntimeParameterProvider.GetBrightfieldCameraSettings()
  -> IBrightfieldCameraCapability.InitializeAsync()
  -> IBrightfieldCameraCapability.StartLiveAsync()
  -> BrightfieldCameraFramePublishedEvent
  -> ImageWorkspaceViewModel.ApplyCameraFrame()
  -> ImageViewer2D.Source
```

### 当前输出

- Demo 明场图像实时刷新。
- 图像显示到中央 ImageDisplay。
- 运行日志显示 Live 状态。

## 6. 明场相机单帧采集流程

### 入口

```text
AcquisitionWorkflowViewModel.SingleCaptureCommand
```

### 执行步骤

1. UI 点击 `Capture`。
2. 状态切换为 `Capturing`。
3. 读取明场相机参数。
4. 初始化相机。
5. 调用 `CaptureSingleAsync()`。
6. 发布一帧图像。
7. 更新 `LiveImageSource`。
8. 状态回到 `Ready`。

### 调用能力

```csharp
IBrightfieldCameraCapability.InitializeAsync(settings)
IBrightfieldCameraCapability.CaptureSingleAsync()
```

### 当前输出

- 一帧明场图像。
- Log 中记录 Capture 状态。

## 7. SRS 2D Demo 流程

### 入口

```text
AcquisitionWorkflowViewModel.RunDemoWorkflowCommand
```

当 `SelectedModality != Multimodal` 时选择：

```text
ExperimentRecipeCatalog.SrsTwoDTemplateId
```

### 配方来源

```text
MetaView.Services.Interfaces/DemoExperimentRecipes.cs
```

创建：

```csharp
CreateSrsTwoD(
    xRelativeDistance: 10,
    yRelativeDistance: 10,
    zRelativeDistance: 0.5,
    daqAcquireDuration: 300 ms)
```

### 配方内容

| 内容 | 当前值 |
| --- | --- |
| `RecipeId` | `demo.srs.2d` |
| `Dimension` | `TwoD` |
| `Modality` | `Srs` |
| `ScanPattern` | `Raster` |
| `ProcessingMode` | `SignalImage` |
| `InputChannels` | `AI0, AI1, AI2, AI3` |
| `PositionXChannel` | `AI0` |
| `PositionYChannel` | `AI1` |
| `SignalChannels` | `AI2,AI3` |
| `CapabilityPlan` | Motion + DataAcquisition + SignalImaging + Algorithm |

### Workflow

```text
MetaView.Services/Workflows/SrsTwoDDemoWorkflow.cs
```

### 步骤顺序

```text
1. validate.recipe
2. capabilities.initialize
3. publish.preview
4. motion.move.x
5. motion.move.y
6. motion.move.z
7. daq.start
8. daq.acquire
9. capabilities.stop
```

### 每一步内容

| Step | 内容 | 调用 |
| --- | --- | --- |
| `validate.recipe` | 检查 2D、SRS、扫描尺寸、处理模式 | 本地校验 |
| `capabilities.initialize` | 初始化 Motion / DAQ / Algorithm 等能力 | `ExperimentCapabilityInvoker.InitializeAsync()` |
| `publish.preview` | 发布一张 Demo SRS 图和四路曲线 | `PublishSignalPreview()` |
| `motion.move.x` | X 相对移动 | `MoveRelativeAsync(MotionAxis.X)` |
| `motion.move.y` | Y 相对移动 | `MoveRelativeAsync(MotionAxis.Y)` |
| `motion.move.z` | Z 相对移动 | `MoveRelativeAsync(MotionAxis.Z)` |
| `daq.start` | 启动 DAQ | `StartDaqAsync()` |
| `daq.acquire` | 模拟采集一段时间并生成图像 | `AcquireDaqAsync()` |
| `capabilities.stop` | 停止相关能力 | `StopAsync()` |

### 输出

- `ExperimentExecutionResult`
- `ExperimentStepRecord`
- `ExperimentDataProduct`
- SRS 图像
- AI0 / AI1 / AI2 / AI3 曲线
- Log 列表逐步输出当前执行到哪一步

## 8. SRS + 明场多模态流程

### 入口

```text
AcquisitionWorkflowViewModel.RunDemoWorkflowCommand
```

当 `SelectedModality == Multimodal` 时选择：

```text
ExperimentRecipeCatalog.SrsBrightfieldTwoDTemplateId
```

### 配方来源

```text
DemoExperimentRecipes.CreateSrsBrightfieldTwoD(...)
```

### 配方内容

总配方：

| 字段 | 内容 |
| --- | --- |
| `RecipeId` | `demo.srs-brightfield.2d` |
| `Dimension` | `TwoD` |
| `Modality` | `Multimodal` |
| `SavePlan` | Image + Curve |

子模态：

| ModalityId | Modality | CapabilityPlan |
| --- | --- | --- |
| `srs` | `Srs` | Motion + DataAcquisition + SignalImaging + Algorithm |
| `brightfield` | `Brightfield` | BrightfieldCamera |

### Workflow

```text
MetaView.Services/Workflows/MultimodalImagingWorkflow.cs
```

### 步骤结构

```text
1. validate.recipe
2. modality.srs
3. modality.brightfield
```

### `modality.srs` 内容

1. 初始化 SRS 所需能力。
2. X 相对移动。
3. Y 相对移动。
4. Z 相对移动。
5. 发布 SRS Preview。
6. 启动 DAQ。
7. 采集 DAQ。
8. 添加数据产品：
   - signal image
   - signal trace
9. 停止能力。

### `modality.brightfield` 内容

1. 初始化明场相机能力。
2. 开启 Live。
3. Capture 一帧。
4. 添加数据产品：
   - brightfield image
5. 停止明场 Live。

### 当前输出

- 一张 SRS Demo 图。
- 四路信号曲线。
- 一帧明场 Demo 图。
- 多个数据产品记录。
- Log 显示每个模态执行进度。

## 9. TPEF / Fluorescence / CARS / DC 流程现状

当前多模态 Workflow 里已经预留了模态映射：

| Modality | 当前执行 |
| --- | --- |
| `Tpef` | `RunPhotoDetectionAsync()` |
| `Fluorescence` | `RunPhotoDetectionAsync()` |
| `Cars` | `RunSrsAsync()` |
| `Dc` | `RunSrsAsync()` |

### 当前状态

这些流程目前是骨架或复用逻辑：

- `Tpef / Fluorescence` 当前只初始化 PhotoDetection，并产出 detector signal 占位产品。
- `Cars / Dc` 当前复用 SRS 的运动 + DAQ + signal imaging 逻辑。

### 后续应补内容

每个模态需要独立定义：

- 光路切换
- 激光参数
- 探测器配置
- DAQ 通道
- 扫描策略
- 算法处理
- 输出产品

## 10. 图像鼠标位移台联动流程

### 当前对象

```text
ImageViewer2D.Controls/ImageViewer2D.cs
MetaView.Presentation/Views/ImageWorkspaceView.xaml.cs
MetaView.Presentation/ViewModels/ImageWorkspaceViewModel.cs
```

### 当前已有能力

`ImageViewer2D` 输出：

```csharp
ImageMouseMove
ImageMouseDown
ImageMouseUp
ImageMouseDoubleClick
ImageMouseWheel
```

`ImageWorkspaceViewModel` 调用：

```csharp
MoveStageToImageCenterAsync()
MoveStageByImageDragAsync()
MoveStageByMouseWheelAsync()
```

最终调用：

```csharp
IMotionControlCapability.MoveRelativeAsync(MotionAxis.X, dx)
IMotionControlCapability.MoveRelativeAsync(MotionAxis.Y, dy)
IMotionControlCapability.MoveRelativeAsync(MotionAxis.Z, dz)
```

### 当前参数

```text
config/metaview.devices.json
```

```json
"imageStageNavigation": {
  "micronsPerPixelX": 0.43,
  "micronsPerPixelY": 0.43,
  "wheelStepMicronsZ": 0.5,
  "invertX": false,
  "invertY": false,
  "invertZ": false
}
```

### 目标交互

你最新定义的是：

```text
鼠标中键：
  第一次按下 -> 进入位移台联动模式
  3 秒内再次按下 -> 立即退出，回到图像 Pan/Zoom
  3 秒无操作 -> 自动退出，回到图像 Pan/Zoom

位移台联动模式下：
  左键拖动 -> 移动 XY
  鼠标滚轮 -> 移动 Z
  双击图像 -> 移动 XY，使点击点到中心

普通图像模式下：
  左键拖动 -> 平移图像
  鼠标滚轮 -> 缩放图像
```

### 当前注意

代码中已有 `StageNavigation` 模式，但最新“中键临时切换 + 3 秒超时 + 状态栏鼠标位置实时更新”需要继续补齐并验证。

## 11. Log 输出流程

### 入口

```text
MetaView.Services/WorkflowLogPublisher.cs
```

### 事件

```text
WorkflowLogPublishedEvent
```

### 来源

- Workflow step started
- Workflow step completed
- Workflow failed
- Preflight 信息
- Camera Live / Capture 信息
- Demo workflow 状态

### UI 显示

当前 Log 显示在界面左下角区域，接收 Prism Event 后添加到列表。

### 当前状态

- 已实现 Workflow Log 输出。
- 已优化过滚动条样式。
- 可继续增加日志级别、过滤、清空、保存。

## 12. 图像与曲线刷新流程

### SRS 图像

入口：

```text
RealtimeSignalImagingService.ProcessDemoFrame()
```

发布：

```text
SignalImageFramePublishedEvent
```

接收：

```text
ImageWorkspaceViewModel.ApplySignalImageFrame()
```

显示：

```text
ImageViewer2D.Source = LiveImageSource
```

### 四路信号曲线

发布：

```text
SignalTraceFramePublishedEvent
```

接收：

```text
ImageWorkspaceViewModel.ApplySignalTraceFrame()
```

显示：

```text
Vibronix.Presentation.Wpf.Plot.PlotView
```

曲线：

- AI0 X
- AI1 Y
- AI2 Laser
- AI3 Laser

### 明场图像

发布：

```text
BrightfieldCameraFramePublishedEvent
```

接收：

```text
ImageWorkspaceViewModel.ApplyCameraFrame()
```

显示：

```text
ImageViewer2D.Source = LiveImageSource
```

## 13. 保存流程现状

### 配方对象

```text
SavePlan
```

包含：

- `AutoSave`
- `Directory`
- `Name`
- `ProductKinds`

### 当前状态

`SavePlan` 已经进入 `ExperimentRecipe`，但真实数据落盘流程还没完整实现。

当前更多是：

- 配方中描述要保存什么。
- Workflow 中生成 `ExperimentDataProduct`。
- UI 中显示保存路径、名称。

后续需要增加：

- `IDataProductWriter`
- 图像保存
- 曲线保存
- 原始 DAQ 数据保存
- 配方快照保存
- 元数据保存

## 14. 真实设备流程现状

### 运动控制器

真实创建链路：

```text
CompositeMotionControlCapability.CreateController()
```

支持：

- Pusi
- Kaifull
- Prior
- ZMotionEthernet
- HeidStarGclib
- E53XMT

当前默认：

```json
"useDemo": true
```

所以实际运行是 Demo。

### 相机

真实创建链路：

```text
FoundationBrightfieldCameraCapability.EnsureFoundationCamera()
CameraFactory.CreateCamera(cameraType)
```

当前默认：

```json
"cameraType": "Demo"
```

所以实际运行是 Demo。

### DAQ

当前有 `IDataAcquisitionCapability` 和 Foundation 适配入口，但 SRS 图像 Demo 仍主要通过模拟帧生成。

## 15. 建议后续流程优先级

### 第一优先级：交互安全

补齐并验证：

- 中键临时位移台联动。
- 3 秒无操作自动退出。
- 再按中键立即退出。
- 状态栏鼠标位置实时刷新。
- Stage 模式明显状态提示。

### 第二优先级：设备会话管理

新增：

```text
IDeviceRuntimeManager
DeviceRuntimeManager
```

统一管理：

- Motion connect/disconnect/reconnect
- Camera connect/disconnect/reconnect
- DAQ configure/start/stop
- Device status event
- Device error log

### 第三优先级：配方文件化

新增：

```text
recipes/*.json
IExperimentRecipeRepository
JsonExperimentRecipeRepository
```

让流程配置从代码移到文件。

### 第四优先级：真实 SRS 采集闭环

实现：

- DAQ 真实采样
- AI0/AI1 坐标映射
- AI2/AI3 分格聚合
- 归一化成图
- 曲线实时显示
- 原始数据保存

### 第五优先级：多模态流程细化

分别细化：

- SRS
- CARS
- TPEF
- Fluorescence
- DC Brightfield
- 3D
- Large Area
- Time Lapse

每个模态都应有独立的：

- CapabilityPlan
- ScanPlan
- ProcessingPlan
- SavePlan
- Workflow steps
