# ImageWatchCSharp

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

