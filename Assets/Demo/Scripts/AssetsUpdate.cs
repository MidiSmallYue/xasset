//
// AssetsUpdate.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace Plugins.XAsset
{
    public class AssetsUpdate : MonoBehaviour
    {
        public enum State
        {
            Wait,
            Checking,
            Downloading,
            Completed,
            Error,
        }

        public State state;

        public Action completed;

        public Action<string, float> progress;

        public Action<string> onError;

        //本地版本文件内容
        private Dictionary<string, string> _versions = new Dictionary<string, string>();

        //远程版本文件内容
        private Dictionary<string, string> _serverVersions = new Dictionary<string, string>();
        private readonly List<Download> _downloads = new List<Download>();
        private int _downloadIndex;

        [SerializeField] string versionsTxt = "versions.txt";

        private string fguiPath = "Demo/FUI/UI/Model";
        private List<FairyGUI.GObject> fguiObjs = new List<FairyGUI.GObject>();
        private ETModel.FUIPackageComponent fuiPackageComponent;
        private ETModel.FUIComponent fuiComponent;


        private void OnError(string e)
        {
            if (onError != null)
            {
                onError(e);
            }

            message = e;
            state = State.Error;
        }

        string message = "click Check to start.";

        void OnProgress(string arg1, float arg2)
        {
            message = string.Format("下载进度 {0:F0}%:{1}({2}/{3})", arg2 * 100, arg1, _downloadIndex, _downloads.Count);
            Log(message);
        }

        void Clear()
        {
            var dir = Path.GetDirectoryName(Utility.updatePath);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            _downloads.Clear();
            _downloadIndex = 0;
            _versions.Clear();
            _serverVersions.Clear();
            message = "click Check to start.";
            state = State.Wait;

            Versions.Clear();

            var path = Utility.updatePath + Versions.versionFile;
            if (File.Exists(path))
                File.Delete(path);
        }

        [Conditional("LOG_ENABLE")]
        private static void Log(string s)
        {
            UnityEngine.Debug.Log(string.Format("[Test]{0}", s));
        }
        void Check()
        {
            fuiComponent = new ETModel.FUIComponent();
            fuiPackageComponent = new ETModel.FUIPackageComponent();
            Assets.Initialize(delegate
            {
                var path = Utility.GetRelativePath4Update(versionsTxt);
                Log(string.Format("Init Success->persistent version path {0}", path));
                //检测可更新目录 是否存在 版本文件
                if (!File.Exists(path))
                {
                    //可更新目录不存在版本文件，从 StreamingAssets 目录中找
                    var asset = Assets.LoadAsync(Utility.GetWebUrlFromDataPath(versionsTxt), typeof(TextAsset));
                    asset.completed += delegate
                    {
                        if (asset.error != null)
                        {
                            OnError(asset.error);
                            return;
                        }

                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        //将 StreamingAssets 中的版本文件 写入可更新目录中，并加载
                        File.WriteAllText(path, asset.text);
                        LoadVersions(asset.text);

                        asset.Release();
                    };
                }
                else
                {
                    LoadVersions(File.ReadAllText(path));
                }
            }, OnError);
            progress += OnProgress;
            state = State.Checking;
        }

        private void Start()
        {
            state = State.Wait;
            Versions.Load();
        }

        private void Update()
        {
            if (state == State.Downloading)
            {
                if (_downloadIndex < _downloads.Count)
                {
                    var download = _downloads[_downloadIndex];
                    download.Update();
                    if (download.isDone)
                    {
                        _downloadIndex = _downloadIndex + 1;
                        if (_downloadIndex == _downloads.Count)
                        {
                            Complete();
                        }
                        else
                        {
                            _downloads[_downloadIndex].Start();
                        }
                    }
                    else
                    {
                        if (progress != null)
                        {
                            progress.Invoke(download.url, download.progress);
                        }
                    }
                }
            }
        }

        string assetPath = "";

        List<Asset> loadedAssets = new List<Asset>();

        void OnAssetLoaded(Asset asset)
        {
            if (asset.name.EndsWith(".prefab", StringComparison.CurrentCulture))
            {
                var go = Instantiate(asset.asset);
                go.name = asset.asset.name;
                asset.Require(go);
                Destroy(go, 3);
                //asset.Release();
            }
            else if (asset.name.Contains("Model"))
            {
                //FairyGUI.UIPackage.AddPackage(asset.bund);
                var obj = FairyGUI.UIPackage.CreateObject("Model", "Loading");
                FairyGUI.GRoot.inst.AddChild(obj);
            }

            loadedAssets.Add(asset);
        }

        private void OnGUI()
        {
            using (var v = new GUILayout.VerticalScope("AssetsUpdate Demo", "window"))
            {
                switch (state)
                {
                    case State.Wait:
                        if (GUILayout.Button("Check"))
                        {
                            Check();
                        }

                        break;

                    case State.Completed:
                        if (GUILayout.Button("Clear"))
                        {
                            Clear();
                        }
                        break;
                    default:
                        break;
                }

                GUILayout.Label(string.Format("{0}:{1}", state, message));
                if (state == State.Completed)
                {
                    GUILayout.Label("AllBundleAssets:");
                    foreach (var item in Assets.bundleAssets)
                    {
                        if (GUILayout.Button(item.Key))
                        {
                            assetPath = item.Key;
                        }
                    }

                    using (var h = new GUILayout.HorizontalScope())
                    {
                        assetPath = GUILayout.TextField(assetPath, GUILayout.Width(256));
                        if (assetPath.Contains("FUI"))
                        {
                            if (GUILayout.Button("AddPackage"))
                            {
                                fuiPackageComponent.AddPackage(assetPath);
                                ETModel.FUI fui = new ETModel.FUI();
                                fui.Awake(FairyGUI.UIPackage.CreateObject("Model", "Loading"));
                                fui.Name = assetPath;
                                fuiComponent.Add(fui);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Load"))
                            {
                                var asset = Assets.Load(assetPath, typeof(UnityEngine.Object));
                                asset.completed += OnAssetLoaded;

                            }
                        }

                        if (GUILayout.Button("LoadAsync"))
                        {
                            var asset = Assets.LoadAsync(assetPath, typeof(UnityEngine.Object));
                            asset.completed += OnAssetLoaded;
                        }

                        if (GUILayout.Button("LoadScene"))
                        {
                            var asset = Assets.LoadScene(assetPath, true, true);
                            asset.completed += OnAssetLoaded;
                        }
                    }

                    if (loadedAssets.Count > 0)
                    {
                        if (GUILayout.Button("UnloadAll"))
                        {
                            for (int i = 0; i < loadedAssets.Count; i++)
                            {
                                var item = loadedAssets[i];
                                item.Release();
                            }

                            loadedAssets.Clear();
                        }

                        for (int i = 0; i < loadedAssets.Count; i++)
                        {
                            var item = loadedAssets[i];
                            using (var hor = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label(item.name);
                                if (GUILayout.Button("Unload"))
                                {
                                    item.Release();
                                    loadedAssets.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                    foreach (var child in fuiComponent.Root.Children.Values)
                    {
                        using (var hor = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(child.Name);
                            if (GUILayout.Button("Remove"))
                            {
                                //.RemovePackage(child.Name);
                                fuiComponent.Remove(child.Name);
                            }
                        }
                    }

                }
            }
        }

        private void Complete()
        {
            Versions.Save();

            //替换本地文件列表中的 文件
            if (_downloads.Count > 0)
            {
                for (int i = 0; i < _downloads.Count; i++)
                {
                    var item = _downloads[i];
                    if (!item.isDone)
                    {
                        break;
                    }
                    else
                    {
                        if (_serverVersions.ContainsKey(item.path))
                        {
                            _versions[item.path] = _serverVersions[item.path];
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                foreach (var item in _versions)
                {
                    sb.AppendLine(string.Format("{0}:{1}", item.Key, item.Value));
                }

                var path = Utility.GetRelativePath4Update(versionsTxt);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                //重新写入可读写目录中的版本文件
                File.WriteAllText(path, sb.ToString());
                Assets.Initialize(delegate
                {
                    if (completed != null)
                    {
                        completed();
                    }
                }, OnError);
                state = State.Completed;

                message = string.Format("{0} files has update.", _downloads.Count);
                return;
            }

            if (completed != null)
            {
                completed();
            }

            message = "nothing to update.";
            state = State.Completed;
        }

        private void Download()
        {
            _downloadIndex = 0;
            _downloads[_downloadIndex].Start();
            state = State.Downloading;
        }

        private void LoadVersions(string text)
        {
            LoadText2Map(text, ref _versions);
            var path = Utility.GetDownloadURL(versionsTxt);
            Log(string.Format("LoadVersions->downloadUrl {0}", path));
            //加载远程服务器 版本文件
            var asset = Assets.LoadAsync(path, typeof(TextAsset));
            asset.completed += delegate
            {
                if (asset.error != null)
                {
                    OnError(asset.error);
                    return;
                }

                LoadText2Map(asset.text, ref _serverVersions);

                //对比两个版本文件，将需要下载的加入列表
                foreach (var item in _serverVersions)
                {
                    string ver;
                    if (!_versions.TryGetValue(item.Key, out ver) || !ver.Equals(item.Value))
                    {
                        var downloader = new Download();
                        downloader.url = Utility.GetDownloadURL(item.Key);
                        downloader.path = item.Key;
                        downloader.version = item.Value;
                        //下载文件的保存路径，直接存到可读写路径中区
                        downloader.savePath = Utility.GetRelativePath4Update(item.Key);
                        _downloads.Add(downloader);
                    }
                }

                if (_downloads.Count == 0)
                {
                    Complete();
                }
                else
                {
                    //平台bundle 文件
                    var downloader = new Download();
                    downloader.url = Utility.GetDownloadURL(Utility.GetPlatform());
                    downloader.path = Utility.GetPlatform();
                    downloader.savePath = Utility.GetRelativePath4Update(Utility.GetPlatform());
                    _downloads.Add(downloader);
                    Download();
                }

                asset.Release();
            };
        }

        //将版本文件中的信息 存在map 中
        private static void LoadText2Map(string text, ref Dictionary<string, string> map)
        {
            map.Clear();
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split(':');
                    if (fields.Length > 1)
                    {
                        map.Add(fields[0], fields[1]);
                    }
                }
            }
        }
    }
}