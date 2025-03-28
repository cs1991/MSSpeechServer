﻿using SimpleHttpServer;
using System;
using System.Collections.Generic;
using System.Net;
using SimpleHttpServer.Models;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Speech.Synthesis;
namespace MSSpeechServer
{
    internal class Program
    {
        static class Routes
        {
            public static List<Route> GET
            {
                get
                {
                    return new List<Route>()
                {
                    new Route()
                    {
                        Callable = GetVoicesHandler,
                        UrlRegex = "^\\/GetVoices$",
                        Method = "GET"
                    },
                    new Route()
                    {
                        Callable = SetTTSHandler,
                        UrlRegex = "^\\/GetTts(?:\\?.*)?$",
                        Method = "GET"
                    }
                };

                }
            }

            private static HttpResponse GetVoicesHandler(HttpRequest request)
            {
                // 获取可用的语音列表
                List<string> availableVoices = new List<string>();
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    foreach (InstalledVoice voice in synth.GetInstalledVoices())
                    {
                        VoiceInfo info = voice.VoiceInfo;
                        availableVoices.Add(info.Name);
                    }
                }

                // 构造 JSON 响应
                string jsonResponse = JsonConvert.SerializeObject(new { errcode = 0, errmsg = "", rtval = availableVoices });

                return new HttpResponse()
                {
                    ContentAsUTF8 = jsonResponse,
                    ReasonPhrase = "OK",
                    StatusCode = "200",
                    Headers = { ["Content-Type"] = "application/json",
                                ["Access-Control-Allow-Origin"] = "*",
                                ["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS",
                                ["Access-Control-Allow-Headers"] = "Content-Type, Authorization", }
                };
            }

            private static HttpResponse SetTTSHandler(HttpRequest request)
            {
                // 获取 URL 中的参数
                string queryString = request.Url;
                string decodedQueryString = WebUtility.UrlDecode(queryString);

                // 初始化变量
                string text = null;
                string voiceName = null;
                float rate = 0.0f;
                float pitch = 1.0f;
                string cantonese = "0";
                // 手动分割参数和值
                string[] parts = decodedQueryString.Split('?');
                if (parts.Length > 1)
                {
                    string[] paramParts = parts[1].Split('&');
                    foreach (string paramPart in paramParts)
                    {
                        string[] keyValue = paramPart.Split('=');
                        if (keyValue.Length == 2)
                        {
                            string key = keyValue[0];
                            string value = keyValue[1];
                            Console.WriteLine($"{key}: {value}");

                            // 检查键值对的键，分别赋值给 text 和 voiceName
                            if (key.Equals("text"))
                            {
                                text = value;
                            }
                            else if (key.Equals("voiceName"))
                            {
                                voiceName = value;
                            }
                            else if (key.Equals("rate")) 
                            {
                                if (float.TryParse(value, out float num)) {
                                    rate = num;
                                }
                            }
                            else if (key.Equals("pitch"))
                            {
                                if (float.TryParse(value, out float num))
                                {
                                    pitch = num;
                                }
                            }
                            else if (key.Equals("cantonese"))
                            {
                                cantonese = value;
                            }
                        }
                    }
                }

                // 检查参数是否为空
                if (string.IsNullOrWhiteSpace(text))
                {
                    // 构造 JSON 响应
                    string jsonResponse = JsonConvert.SerializeObject(new { errcode = 4001, errmsg = "Error: 'text' parameter cannot be empty", rtval = (string)null });

                    return new HttpResponse()
                    {
                        ContentAsUTF8 = jsonResponse,
                        ReasonPhrase = "Bad Request",
                        StatusCode = "400",
                        Headers = { ["Content-Type"] = "application/json",
                                ["Access-Control-Allow-Origin"] = "*",
                                ["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS",
                                ["Access-Control-Allow-Headers"] = "Content-Type, Authorization", }
                    };
                }

                // Process the request to set TTS (Text-to-Speech) with text and voiceName
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    var voices = synth.GetInstalledVoices();
                    if (voices.Count == 0)
                    {
                        // 构造 JSON 响应
                        string jsonResponse = JsonConvert.SerializeObject(new { errcode = 4001, errmsg = "No available voices found.", rtval = (string)null });

                        return new HttpResponse()
                        {
                            ContentAsUTF8 = jsonResponse,
                            ReasonPhrase = "Bad Request",
                            StatusCode = "400",
                            Headers = { ["Content-Type"] = "application/json",
                                ["Access-Control-Allow-Origin"] = "*",
                                ["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS",
                                ["Access-Control-Allow-Headers"] = "Content-Type, Authorization", }
                        };
                    }
                    else
                    {

                        // 如果设置语音库
                        bool makeSpeak = true;
                        string finalVoiceName = "";
                        string lan = "zh-CN";
                        if (!string.IsNullOrWhiteSpace(cantonese) && cantonese == "1")
                        {
                            voiceName = "Microsoft Server Speech Text to Speech Voice (zh-HK, HunYee)";
                            finalVoiceName = voiceName;
                            synth.SelectVoice(voiceName);
                            lan = "zh-HK";
                        }
                        else if (!string.IsNullOrWhiteSpace(voiceName))
                        {
                            bool voiceFound = false;
                            foreach (InstalledVoice voice in voices)
                            {
                                VoiceInfo info = voice.VoiceInfo;
                                if (info.Name == voiceName)
                                {
                                    synth.SelectVoice(info.Name);
                                    voiceFound = true;
                                    finalVoiceName = info.Name;
                                    break;
                                }
                            }
                            if (!voiceFound)
                            {
                                makeSpeak = false;
                                // 构造 JSON 响应
                                string jsonResponse = JsonConvert.SerializeObject(new { errcode = 4001, errmsg = $"Voice '{voiceName}' not found.", rtval = (string)null });

                                return new HttpResponse()
                                {
                                    ContentAsUTF8 = jsonResponse,
                                    ReasonPhrase = "Bad Request",
                                    StatusCode = "400",
                                    Headers = { ["Content-Type"] = "application/json" ,
                                                ["Access-Control-Allow-Origin"] = "*",
                                                ["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS",
                                                ["Access-Control-Allow-Headers"] = "Content-Type, Authorization", }
                                };
                            }
                        }
                        else
                        {
                            // 默认选择第一个语音
                            VoiceInfo info = voices[0].VoiceInfo;
                            synth.SelectVoice(info.Name);
                            finalVoiceName = info.Name;
                        }
                        if (!string.IsNullOrWhiteSpace(cantonese) && cantonese == "1")
                        {
                            lan = "zh-HK";
                        }
                        else {
                            if (finalVoiceName.Contains("zh-HK"))
                            {
                                lan = "zh-HK";
                            }
                            else {
                                lan = "zh-CN";
                            }
                        }
                        if (makeSpeak)
                        {
                            // 将文本转换为语音并保存为 WAV 格式的字节数组
                            var memoryStream = new MemoryStream();
                            synth.SetOutputToWaveStream(memoryStream);

                            synth.Rate = (int)(rate * 10);

                            string ssmlTemplate = @"
<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xml:lang=""{4}"">
    <voice name=""{0}"">
        <prosody rate=""{1}%"" pitch=""{2}%"" volume=""loud"">
            {3}
        </prosody>
    </voice>
</speak>";
                            string srate = rate > 0 ? "+" + ((int)(rate * 100)).ToString():((int)(rate * 10)).ToString();
                            string spitch = pitch > 0 ? "+" + ((int)(pitch * 100)).ToString(): ((int)(pitch * 10)).ToString();
                            //string text = "你好，这里是微软语音合成。";

                            string ssml = string.Format(ssmlTemplate, finalVoiceName, srate, spitch, text, lan);

                            //synth.Speak(text);
                            synth.SpeakSsml(ssml);
                            memoryStream.Position = 0;

                            // 构造响应
                            return new HttpResponse
                            {
                                Content = memoryStream.ToArray(),
                                Headers = { ["Content-Type"] = "audio/wav" ,
                                        ["Access-Control-Allow-Origin"] = "*",
                                        ["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS",
                                        ["Access-Control-Allow-Headers"] = "Content-Type, Authorization", },
                                StatusCode = "200",
                                ReasonPhrase = "OK"
                            };
                        }

                    }
                }

                // Default response if no data is returned
                return new HttpResponse()
                {
                    ContentAsUTF8 = "",
                    ReasonPhrase = "OK",
                    StatusCode = "200"
                };
            }
        }

        static void Main(string[] args)
        {
            HttpServer httpServer = new HttpServer(8080, Routes.GET);
            Console.WriteLine("HTTP server is started and waiting for connections...");
            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
        }
    }
}
