using Microsoft.Toolkit.Mvvm.ComponentModel;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskTopTimer.AudioWaves
{
    public class AudioWaveViewModels:ObservableObject
    {
        public WasapiLoopbackCapture cap = new WasapiLoopbackCapture();

        public delegate void WaveDataChangedHandler(float[] samples);
        public event WaveDataChangedHandler WaveDataChanged;

        public AudioWaveViewModels()
        {
            cap.DataAvailable += WaveDataIn;
        }


        private void WaveDataIn(object? sender, WaveInEventArgs e)
        {
          
                float[] allSamples = Enumerable      // 提取数据中的采样
                    .Range(0, e.BytesRecorded / 4)   // 除以四是因为, 缓冲区内每 4 个字节构成一个浮点数, 一个浮点数是一个采样
                    .Select(i => BitConverter.ToSingle(e.Buffer, i * 4))  // 转换为 float
                    .ToArray();    // 转换为数组
                                   // 获取采样后, 在这里进行详细处理

                int channelCount = cap.WaveFormat.Channels;   // WasapiLoopbackCapture 的 WaveFormat 指定了当前声音的波形格式, 其中包含就通道数

                float[][] channelSamples = Enumerable
                  .Range(0, channelCount)
                  .Select(channel => Enumerable
                      .Range(0, allSamples.Length / channelCount)
                      .Select(i => allSamples[channel + i * channelCount])
                      .ToArray())
                  .ToArray();

                float[] samples = Enumerable.Range(0, allSamples.Length / channelCount)
                    .Select(index => Enumerable
                    .Range(0, channelCount)
                    .Select(channel => channelSamples[channel][index])
                    .Average())
                    .ToArray();


                float log = (float)Math.Ceiling(Math.Log(samples.Length, 2));   // 取对数并向上取整
                int newLen = (int)Math.Pow(2, log);                             // 计算新长度
                float[] filledSamples = new float[newLen];
                Array.Copy(samples, filledSamples, samples.Length);   // 拷贝到新数组
                Complex[] complexSrc = filledSamples
                  .Select(v => new Complex() { X = v })        // 将采样转换为复数
                  .ToArray();
                FastFourierTransform.FFT(false, (int)log, complexSrc);   // 进行傅里叶变换

                Complex[] halfData = complexSrc.Take(complexSrc.Length / 2).ToArray();    // 一半的数据
                float[] dftData = halfData
                  .Select(v => (float)Math.Sqrt(v.X * v.X + v.Y * v.Y))  // 取复数的模
                  .ToArray();    // 将复数结果转换为我们所需要的频率幅度
                int count = dftData.Length / (cap.WaveFormat.SampleRate / (filledSamples.Length == 0 ? 1 : filledSamples.Length));

                WaveDataChanged?.Invoke(dftData.Take(count).ToArray());
            
        }

        public bool StartRecord()
        {
            try
            {
                cap.StartRecording();
                return true;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }
        public bool StopRecord()
        {
            try
            {
                cap.StopRecording();
                WaveDataChanged?.Invoke(new float[0]);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }
    }
}
