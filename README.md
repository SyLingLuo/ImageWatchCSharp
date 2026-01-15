# ImageWatchCSharp

ImageWatchCSharp Engligh
ImageWatchCSharp is a Visual Studio extension designed for .NET developers using OpenCvSharp. It provides an intuitive tool window that allows you to easily view, analyze, and verify image data during debugging, moving beyond the era of Cv2.ImShow pop-up windows.

💡 Inspired by the C++ ImageWatch, now built for a native-level debugging experience with OpenCvSharp.

Features
Automatic Variable Discovery: Automatically scans and displays Mat variables in the current scope when a breakpoint is hit.
Detailed Property Display: View image width, height, number of channels, and type (e.g., CV_8UC3).
Pixel-Level Inspection: Hover the mouse to preview pixel coordinates and RGB/grayscale values.
Resource Status Monitoring: Flags disposed image resources to help avoid null reference errors.
Flexible Zoom Preview: Supports mouse wheel zooming and drag-and-drop panning for detailed observation.
Dual-Panel Layout: Clear and intuitive interface with a variable list on the left and image preview on the right.
Installation
Download the extension from the Visual Studio Marketplace.
In Visual Studio, go to Extensions → Manage Extensions, search for ImageWatchCSharp, and install it.
Restart Visual Studio after installation is complete.
Usage Guide
Start debugging and pause at a breakpoint.
Open the menu: View → Other Windows → ImageWatch For OpenCvSharp.
The tool window will automatically load all Mat objects in the current context.
Click on an image name in the left-hand list to preview it instantly on the right.
Use the mouse wheel to zoom the image and hover to inspect pixel values.
System Requirements
Visual Studio 2022 or later (other versions not tested).
.NET Framework 4.7.2 or above.
Intended for C#/.NET projects using OpenCvSharp.
License
This project is licensed under the MIT License.


**ImageWatchCSharp** 是一个 Visual Studio 扩展，专为使用 [OpenCvSharp](https://github.com/shimat/opencvsharp) 的 .NET 开发者设计。  
它提供了一个直观的工具窗口，让你在调试过程中轻松查看、分析和验证图像数据，告别 `Cv2.ImShow` 弹窗时代。

> 💡 灵感源自 C++ 的 ImageWatch，现为 OpenCvSharp 打造原生级调试体验。

## 功能特点

- **自动变量发现**：断点命中时，自动扫描并显示当前作用域内的 `Mat` 变量
- **详细属性展示**：查看图像的宽度、高度、通道数和类型（如 `CV_8UC3`）
- **像素级检查**：鼠标悬停预览像素坐标与 RGB/灰度值
- **资源状态监控**：标记已释放（Disposed）的图像资源，避免空引用误判
- **自由缩放预览**：支持鼠标滚轮缩放、拖拽平移，便于细节观察
- **双面板布局**：左侧变量列表 + 右侧图像预览，界面清晰直观

## 安装方法

1. 从 [Visual Studio Marketplace](https://marketplace.visualstudio.com/) 下载扩展
2. 在 Visual Studio 中打开 **扩展 → 管理扩展**，搜索 `ImageWatchCSharp` 并安装
3. 安装完成后重启 Visual Studio

## 使用指南

1. 启动调试，在断点处暂停
2. 打开菜单：**视图 → 其他窗口 → ImageWatch For OpenCvSharp**
3. 工具窗口将自动加载当前上下文中所有 `Mat` 对象
4. 点击左侧列表中的图像名称，右侧即时预览

5. 使用鼠标滚轮缩放图像，悬停查看像素值

## 系统要求

- **Visual Studio 2022** 或更高版本（其他版本未测试）
- **.NET Framework 4.7.2** 或以上
- 适用于使用 [OpenCvSharp](https://github.com/shimat/opencvsharp) 的 C#/.NET 项目

## 许可证

本项目采用 [MIT 许可证](LICENSE)。

## 贡献

欢迎提交 Issue 报告问题或提出功能建议！  
我们也非常欢迎 Pull Request，共同完善这个工具 🙌

