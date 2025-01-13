using System.Collections;
using System.Collections.Generic;
using System.IO;
using QxFramework.Utilities;
using UnityEngine;

namespace QxFramework.Core
{
    /// <summary>
    /// 资源管理，提供各项本地资源，包括物体对象和UI等的解码和加载功能。
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>, ISystemModule
    {
        private Dictionary<string, UnityEngine.Object> _cache = new Dictionary<string, Object>();
        private Dictionary<string, UnityEngine.Object[]> _arrayCache = new Dictionary<string, Object[]>();

        /// <summary>
        /// 持久化的文件存储URL，直接读取
        /// </summary>
        /// <returns></returns>
        public string PersistentDataUrl
        {
            get
            {
                return "file:///" + Application.persistentDataPath + "/";
            }
        }

        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path.</param>
        /// <param name="instance">if set to <c>true</c> [instance].</param>
        /// <returns></returns>
        public T Load<T>(string path, bool instance = false) where T : UnityEngine.Object
        {
            T asset;
            try
            {
                _cache.ContainsKey(path);
            }
            catch
            {
                Debug.LogError("WrongPath:"+ path);
            }
            if (_cache.ContainsKey(path))
            {
                asset = _cache[path] as T;
            }
            else
            {
                asset = Resources.Load<T>(path) as T;
                _cache[path] = asset;
            }

            if (asset != null && instance)
            {
                return Object.Instantiate(asset);
            }
            return asset;
        }

        /// <summary>
        /// 从文件中实例化预设到游戏物体。
        /// </summary>
        /// <param name="path">预设目录。</param>
        /// <param name="parent">设置的父对象。</param>
        /// <returns></returns>
        public GameObject Instantiate(string path, Transform parent = null)
        {
            GameObject asset = Load<GameObject>(path);
            GameObject go = null;
            if (asset != null)
            {
                //go = GameObject.Instantiate(asset, parent) as GameObject;
                //使用对象池的方式.
                go = ObjectPool.Spawn(asset, parent);
            }
            return go;
        }

        public void Update(float deltaTime)
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// 测试目录是否有写权限
        /// </summary>
        /// <param name="folderPath">路径</param>
        /// <returns>有写权限</returns>
        public static bool HasWriteAccessToFolder(string folderPath)
        {
            try
            {
                string tmpFilePath = Path.Combine(folderPath, Path.GetRandomFileName());
                using (
                    FileStream fs = new FileStream(tmpFilePath, FileMode.CreateNew, FileAccess.ReadWrite,
                        FileShare.ReadWrite))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write("1");
                }
                File.Delete(tmpFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void FixedUpdate(float deltaTime)
        {
        }

        public T[] LoadAll<T>(string path) where T : Object
        {
            if (_arrayCache.ContainsKey(path))
            {
                return _arrayCache[path] as T[]; 
            }
            else
            {
                var t = Resources.LoadAll<T>(path);
                _arrayCache[path] = t;
                for (int i = 0; i < t.Length; i++)
                {
                    _cache[path + "/" + t[i].name] = t[i];
                }
                return t;
            }
        }

        public ResourceRequest LoadAsync<T>(string path) where T : Object
        {
            return Resources.LoadAsync<T>(path);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool InCache(string path)
        {
            return (_cache.ContainsKey(path));
        }

        public void AddCache(string path,Object obj)
        {
            try
            {
                if (InCache(path))
                {
                    Debug.LogWarning("AddCache Repeat :" + path);
                }
                _cache[path]= obj;
            }
            catch (System.Exception)
            {
                Debug.LogError("AddCache Error :" + path);
                throw;
            }
          
        }
    }
}