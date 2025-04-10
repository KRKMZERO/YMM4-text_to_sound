using Ymm4.Plugin.Extension;
using Ymm4.Plugin.Types;
using NAudio.Wave;
using System;
using System.IO;

public class TalkSyncSound : OutputExtensionBase
{
    private AudioFileReader audioFile;
    private WaveOutEvent outputDevice;
    private string previousText = "";

    public override string Name => "テキスト音声同期プラグイン（音量調整付き）";
    public override string Description => "テキストが表示されるたびに音を鳴らします。value0でWAVファイル指定、value1で音量調整ができます。";
    public override ExtensionCategory Category => ExtensionCategory.Character;

    public override void OnStart(IExtensionContext context)
    {
        string soundPath = context.GetStringParameter("value0") ?? "";
        double volumePercent = context.GetDoubleParameter("value1", 100); // 0〜100
        float volume = (float)(Math.Clamp(volumePercent, 0, 100) / 100.0);

        if (File.Exists(soundPath))
        {
            audioFile = new AudioFileReader(soundPath)
            {
                Volume = volume
            };

            outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
        }
    }

    public override void OnUpdate(IExtensionContext context)
    {
        var text = context.Text ?? "";
        if (text.Length > previousText.Length)
        {
            // 新しい文字が追加された → 音を鳴らす
            if (outputDevice != null && audioFile != null)
            {
                audioFile.Position = 0; // 再生位置をリセット
                outputDevice.Play();
            }
        }
        previousText = text;
    }

    public override void OnEnd(IExtensionContext context)
    {
        outputDevice?.Stop();
        outputDevice?.Dispose();
        audioFile?.Dispose();
    }
}
