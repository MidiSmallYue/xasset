using FairyGUI;
using Plugins.XAsset;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    /// <summary>
    /// 管理所有UI Package
    /// </summary>
    public class FUIPackageComponent
    {
#if UNITY_EDITOR
        public const string FUI_PACKAGE_DIR = "Assets/Bundles/FUI";
#endif

        private readonly Dictionary<string, UIPackage> packages = new Dictionary<string, UIPackage>();
        private readonly Dictionary<string, Bundle> packageBundles = new Dictionary<string, Bundle>();


        public void AddPackage(string path)
        {
            if (packages.ContainsKey(path))
            {
                return;
            }
            if (Utility.assetBundleMode)
            {
                if(Assets.GetAssetBundleName(path, out  var assetBundleName))
                {
                    var bundle = Bundles.Load(assetBundleName);
                    UIPackage uiPackage = UIPackage.AddPackage(bundle.assetBundle);
                    packages.Add(path, uiPackage);
                    packageBundles.Add(path, bundle);
                }
                
            }
            else
            {
                var index = path.LastIndexOf("_fui.bytes");
                if (index > 0)
                {
                    path = path.Substring(0, path.Length - index);
                }
                UIPackage uiPackage = UIPackage.AddPackage(path);
                packages.Add(path, uiPackage);
            }

        }

        public void AddPackageAsync(string path)
        {
            if (Utility.assetBundleMode)
            {
                var bundle = Bundles.LoadAsync(path);
                bundle.completed += delegate (Asset a)
                 {
                     UIPackage uiPackage = UIPackage.AddPackage(bundle.assetBundle);
                     packages.Add(path, uiPackage);
                 };
            }
            else
            {
                var index = path.LastIndexOf("_fui.bytes");
                if (index > 0)
                {
                    path = path.Substring(0, path.Length - index);
                }
                UIPackage uiPackage = UIPackage.AddPackage(path);
                packages.Add(path, uiPackage);
            }
        }

        public void RemovePackage(string type)
        {
            UIPackage package;

            if (Utility.assetBundleMode)
            {
                if (packages.TryGetValue(type, out package))
                {
                    var p = UIPackage.GetByName(package.name);

                    if (p != null)
                    {
                        UIPackage.RemovePackage(package.name);
                    }

                    packages.Remove(package.name);
                }
                if (packageBundles.TryGetValue(type, out var bundle))
                {
                    Bundles.Unload(bundle);

                    packageBundles.Remove(type);
                }


            }
            else
            {
                var index = type.LastIndexOf("_fui.bytes");
                if (index > 0)
                {
                    type = type.Substring(0, type.Length - index);
                }
                if (packages.TryGetValue(type, out package))
                {
                    var p = UIPackage.GetByName(package.name);

                    if (p != null)
                    {
                        UIPackage.RemovePackage(package.name);
                    }

                    packages.Remove(package.name);
                }
            }

        }
    }
}