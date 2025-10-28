# MusicTracker - 实时显示当前播放歌曲的小工具

这是一个用 C# 编写的轻量级工具，能自动获取 Windows 系统当前播放的媒体信息，并实时写入 `current_track.txt` 文件。配合 B站直播姬 的「文字素材 → 从文件导入」功能，即可在直播画面中显示“正在播放的歌曲”。

---

## 快速开始

### 1）下载运行
1. 打开Releases页：https://github.com/mcss6699/MusicTracker/releases
2. 下载解压，双击 MusicTracker.exe 运行
3. 此时会生成一个`current_track.txt` 文件，其中就是歌曲信息
4. 不要关闭命令行，如果嫌碍眼可以按 Win+Tab 将命令行窗口放到其他桌面

### 2）配置直播姬
1. 打开 **直播姬**
2. 添加一个 **“文字素材”**
3. 在文字设置窗口右上角，找到 **“文件导入”** 选项
4. 选择你刚刚生成的 `current_track.txt` 文件（就在项目根目录）

---

## 高级设置

### 开机自启
1. 右键 MusicTracker.exe，选择发送快捷方式到桌面
2. 右键这个快捷方式 → **复制**
3. 按 `Win + R`，输入 `shell:startup` 回车
4. 在启动文件夹里 **右键 → 粘贴**