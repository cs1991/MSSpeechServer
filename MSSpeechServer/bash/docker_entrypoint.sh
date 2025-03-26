#!/bin/bash
set -euo pipefail

source auto_xvfb

# 判断目标文件夹是否已经包含了指定的文件
if ! diff -q /wine32/drive_c/Program\ Files/Common\ Files/Microsoft\ Shared/Speech/TTS/v11.0/ /wine32/drive_c/libs/chsbrkr_chtbrkr_dlls/ >/dev/null 2>&1; then
    # 如果目标文件夹不包含指定的文件或diff命令报错，则执行安装步骤
	echo "开始安装语言包..."
    wine msiexec /qn /i /wine32/drive_c/libs/speech_platform_runtime_v11/SpeechPlatformRuntimeX86.msi
    wine msiexec /qn /i /wine32/drive_c/libs/speech_platform_runtime_v11/x86_MicrosoftSpeechPlatformSDK.msi
    wine msiexec /qn /i /wine32/drive_c/libs/language_packs_for_v11/MSSpeech_TTS_en-US_ZiraPro.msi
    wine msiexec /qn /i /wine32/drive_c/libs/language_packs_for_v11/MSSpeech_TTS_zh-CN_HuiHui.msi
    wine msiexec /qn /i /wine32/drive_c/libs/language_packs_for_v11/MSSpeech_TTS_zh-HK_HunYee.msi
	wine msiexec /qn /i /wine32/drive_c/libs/language_packs_for_v11/MSSpeech_SR_zh-CN_TELE.msi
	wine msiexec /qn /i /wine32/drive_c/libs/language_packs_for_v11/MSSpeech_SR_zh-HK_TELE.msi
    cp /wine32/drive_c/libs/chsbrkr_chtbrkr_dlls/* /wine32/drive_c/Program\ Files/Common\ Files/Microsoft\ Shared/Speech/TTS/v11.0/
    cp /wine32/drive_c/libs/chsbrkr_chtbrkr_dlls/NaturalLanguage6.dll /wine32/drive_c/windows/system32/
fi

# 运行应用程序
if [ -d "/wine32/drive_c/debug" ] && [ "$(ls -A /wine32/drive_c/debug 2>/dev/null)" ]; then
	echo "进入wine32/drive_c/debug..."
    cd /wine32/drive_c/debug
else
    cd /wine32/drive_c/bin
fi
wine MSSpeechServer.exe

