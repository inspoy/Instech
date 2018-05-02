/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: AssetBundleManager.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using System;
using Object = UnityEngine.Object;

namespace Instech.Framework
{
    /// <summary>
    /// AssetBundle加载管理
    /// </summary>
    public class AssetBundleManager : MonoSingleton<AssetBundleManager>
    {
        protected override void Init()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据类型获取资产
        /// </summary>
        /// <typeparam name="T">资产类型</typeparam>
        /// <param name="path">资产路径</param>
        /// <returns></returns>
        public T LoadAsset<T>(string path) where T : Object
        {
            throw new NotImplementedException();
        }
    }
}
