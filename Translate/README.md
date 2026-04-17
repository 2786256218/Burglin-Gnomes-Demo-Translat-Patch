# Burglin' Gnomes Demo 汉化工程

此文件夹包含所有的汉化核心代码、注入器和字体文件，可在任何 Windows 电脑上直接编译和使用。

## 文件说明
- `GnomiumTranslationCore.cs`: 核心汉化代码，包含所有的文本翻译字典和字体注入逻辑。
- `Patcher.cs`: 注入器代码，用于将汉化核心挂载到游戏原生的 `TextMeshPro` 和 `Text` 组件的生命周期上。
- `Mono.Cecil.dll`: 注入器所需的依赖库。
- `宋体.ttf`: 游戏中使用的中文字体文件。
- `build.bat`: 一键编译脚本，使用 Windows 系统自带的 C# 编译器即可编译，无需安装 Visual Studio。
- `install.bat`: 一键安装脚本，自动将编译好的文件和字体复制到游戏目录并执行注入。

## 如何在其他设备上编译和使用
1. 确保本 `Translate` 文件夹放置在游戏的根目录下（即与 `Burglin' Gnomes Demo.exe` 同级）。
2. 双击运行 `build.bat`。如果提示成功，文件夹内会生成 `GnomiumTranslationCore.dll` 和 `Patcher.exe`。
3. 双击运行 `install.bat`。它会自动将 `GnomiumTranslationCore.dll` 复制到 `Gnomium_Data\Managed` 目录，将 `宋体.ttf` 复制到游戏根目录，并运行 `Patcher.exe` 完成注入。
4. 启动游戏即可体验汉化。

## 如何修改翻译内容
1. 使用任意文本编辑器（推荐 VS Code 或 Notepad++，保存为 **UTF-8 with BOM** 编码）打开 `GnomiumTranslationCore.cs`。
2. 在 `InitDict()` 方法中添加或修改 `Dict["英文原文"] = "中文翻译";`。
3. 重新运行 `build.bat` 和 `install.bat`。
