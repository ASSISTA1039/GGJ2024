using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ObjectPool : MonoBehaviour
{
    public enum StartupPoolMode
    {
        Awake,
        Start,
        CallManually
    }

    [Serializable]
    public class StartupPool
    {
        public int Size;

        public GameObject Prefab;
    }

    private static ObjectPool _instance;

    private static readonly List<GameObject> _tempList = new List<GameObject>();

    private readonly Dictionary<GameObject, List<GameObject>> _pooledObjects = new Dictionary<GameObject, List<GameObject>>();

    private readonly Dictionary<GameObject, GameObject> _spawnedObjects = new Dictionary<GameObject, GameObject>();

    public ObjectPool.StartupPoolMode startupPoolMode;

    public ObjectPool.StartupPool[] StartupPools;

    private bool _startupPoolsCreated;

    public static ObjectPool Instance
    {
        get
        {
            if (ObjectPool._instance != null)
            {
                return ObjectPool._instance;
            }
            //FindObjectOfType(typeof(Type))返回Type类型第一个激活的加载的物体。
            ObjectPool._instance = UnityEngine.Object.FindObjectOfType<ObjectPool>();
            if (ObjectPool._instance != null)
            {
                return ObjectPool._instance;
            }
            ObjectPool._instance = new GameObject("ObjectPool")
            {
                transform =
                {
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            }.AddComponent<ObjectPool>();
            return ObjectPool._instance;
        }
    }

    private void Awake()
    {
        ObjectPool._instance = this;
        if (this.startupPoolMode == ObjectPool.StartupPoolMode.Awake)
        {
            ObjectPool.CreateStartupPools();
        }
    }

    private void Start()
    {
        if (this.startupPoolMode == ObjectPool.StartupPoolMode.Start)
        {
            ObjectPool.CreateStartupPools();
        }
    }

    public static void CreateStartupPools()
    {
        if (!ObjectPool.Instance._startupPoolsCreated)
        {
            ObjectPool.Instance._startupPoolsCreated = true;
            ObjectPool.StartupPool[] array = ObjectPool.Instance.StartupPools;
            if (array != null && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    ObjectPool.CreatePool(array[i].Prefab, array[i].Size);
                }
            }
        }
    }

    public static void CreatePool<T>(T prefab, int initialPoolSize) where T : Component
    {
        ObjectPool.CreatePool(prefab.gameObject, initialPoolSize);
    }

    public static void CreatePool(GameObject prefab, int initialPoolSize)
    {
        if (prefab != null && !ObjectPool.Instance._pooledObjects.ContainsKey(prefab))
        {
            List<GameObject> list = new List<GameObject>();
            ObjectPool.Instance._pooledObjects.Add(prefab, list);
            if (initialPoolSize > 0)
            {
                bool activeSelf = prefab.activeSelf;
                prefab.SetActive(false);
                Transform transform = ObjectPool.Instance.transform;
                while (list.Count < initialPoolSize)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
                    gameObject.transform.parent = transform;
                    list.Add(gameObject);
                }
                prefab.SetActive(activeSelf);
            }
        }
    }

    public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
    {
        return ObjectPool.Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
    }

    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return ObjectPool.Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
    }

    public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
    {
        return ObjectPool.Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
    }

    public static T Spawn<T>(T prefab, Vector3 position) where T : Component
    {
        return ObjectPool.Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
    }

    public static T Spawn<T>(T prefab, Transform parent) where T : Component
    {
        return ObjectPool.Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
    }

    public static T Spawn<T>(T prefab) where T : Component
    {
        return ObjectPool.Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
    }

    public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        List<GameObject> list;
        GameObject gameObject;
        Transform transform;
        if (ObjectPool.Instance._pooledObjects.TryGetValue(prefab, out list))
        {
            gameObject = null;
            if (list.Count > 0)
            {
                while (gameObject == null && list.Count > 0)
                {
                    gameObject = list[0];
                    list.RemoveAt(0);
                }
                if (gameObject != null)
                {
                    transform = gameObject.transform;
                    transform.parent = parent;
                    transform.localPosition = position;
                    transform.localRotation = rotation;
                    gameObject.SetActive(true);
                    ObjectPool.Instance._spawnedObjects.Add(gameObject, prefab);
                    return gameObject;
                }
            }
            gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
            transform = gameObject.transform;
            transform.parent = parent;
            transform.localPosition = position;
            transform.localRotation = rotation;
            ObjectPool.Instance._spawnedObjects.Add(gameObject, prefab);
            return gameObject;
        }
        gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
        transform = gameObject.GetComponent<Transform>();
        transform.parent = parent;
        transform.localPosition = position;
        transform.localRotation = rotation;
        return gameObject;
    }

    public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
    {
        return ObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return ObjectPool.Spawn(prefab, null, position, rotation);
    }

    /// <summary>
    /// 针对ResourceManager的Instantiate函数写的Spawn（保证不错位）
    /// </summary>
    public static GameObject Spawn(GameObject prefab, Transform parent)
    {
        //return ObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        List<GameObject> list;
        GameObject gameObject;
        Transform transform;
        if (ObjectPool.Instance._pooledObjects.TryGetValue(prefab, out list))
        {
            gameObject = null;
            if (list.Count > 0)
            {
                while (gameObject == null && list.Count > 0)
                {
                    gameObject = list[0];
                    list.RemoveAt(0);
                }
                if (gameObject != null)
                {
                    transform = gameObject.transform;
                    transform.SetParent(parent);
                    gameObject.SetActive(true);
                    ObjectPool.Instance._spawnedObjects.Add(gameObject, prefab);
                    return gameObject;
                }
            }
            gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, parent);
            ObjectPool.Instance._spawnedObjects.Add(gameObject, prefab);
            return gameObject;
        }
        //创建一个对应的对象池.
        ObjectPool.CreatePool(prefab, 0);
        gameObject = Spawn(prefab, parent);
        return gameObject;
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position)
    {
        return ObjectPool.Spawn(prefab, null, position, Quaternion.identity);
    }

    public static GameObject Spawn(GameObject prefab)
    {
        return ObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
    }

    public static void Recycle<T>(T obj) where T : Component
    {
        ObjectPool.Recycle(obj.gameObject);
    }

    public static void Recycle(GameObject obj)
    {
        GameObject prefab;
        if (ObjectPool.Instance._spawnedObjects.TryGetValue(obj, out prefab))
        {
            ObjectPool.Recycle(obj, prefab);
        }
        else
        {
            UnityEngine.Object.Destroy(obj);
        }
    }

    private static void Recycle(GameObject obj, GameObject prefab)
    {
        ObjectPool.Instance._pooledObjects[prefab].Add(obj);
        ObjectPool.Instance._spawnedObjects.Remove(obj);
        obj.transform.SetParent(ObjectPool.Instance.transform);
        obj.SetActive(false);
    }

    public static void RecycleAll<T>(T prefab) where T : Component
    {
        ObjectPool.RecycleAll(prefab.gameObject);
    }

    public static void RecycleAll(GameObject prefab)
    {
        foreach (KeyValuePair<GameObject, GameObject> current in ObjectPool.Instance._spawnedObjects)
        {
            if (current.Value == prefab)
            {
                ObjectPool._tempList.Add(current.Key);
            }
        }
        for (int i = 0; i < ObjectPool._tempList.Count; i++)
        {
            ObjectPool.Recycle(ObjectPool._tempList[i]);
        }
        ObjectPool._tempList.Clear();
    }

    public static void RecycleAll()
    {
        ObjectPool._tempList.AddRange(ObjectPool.Instance._spawnedObjects.Keys);
        for (int i = 0; i < ObjectPool._tempList.Count; i++)
        {
            ObjectPool.Recycle(ObjectPool._tempList[i]);
        }
        ObjectPool._tempList.Clear();
    }

    public static bool IsSpawned(GameObject obj)
    {
        return ObjectPool.Instance._spawnedObjects.ContainsKey(obj);
    }

    public static int CountPooled<T>(T prefab) where T : Component
    {
        return ObjectPool.CountPooled(prefab.gameObject);
    }

    public static int CountPooled(GameObject prefab)
    {
        List<GameObject> list;
        if (ObjectPool.Instance._pooledObjects.TryGetValue(prefab, out list))
        {
            return list.Count;
        }
        return 0;
    }

    public static int CountSpawned<T>(T prefab) where T : Component
    {
        return ObjectPool.CountSpawned(prefab.gameObject);
    }

    public static int CountSpawned(GameObject prefab)
    {
        int num = 0;
        foreach (GameObject current in ObjectPool.Instance._spawnedObjects.Values)
        {
            if (prefab == current)
            {
                num++;
            }
        }
        return num;
    }

    public static int CountAllPooled()
    {
        int num = 0;
        foreach (List<GameObject> current in ObjectPool.Instance._pooledObjects.Values)
        {
            num += current.Count;
        }
        return num;
    }

    public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
    {
        if (list == null)
        {
            list = new List<GameObject>();
        }
        if (!appendList)
        {
            list.Clear();
        }
        List<GameObject> collection;
        if (ObjectPool.Instance._pooledObjects.TryGetValue(prefab, out collection))
        {
            list.AddRange(collection);
        }
        return list;
    }

    public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
    {
        if (list == null)
        {
            list = new List<T>();
        }
        if (!appendList)
        {
            list.Clear();
        }
        List<GameObject> list2;
        if (ObjectPool.Instance._pooledObjects.TryGetValue(prefab.gameObject, out list2))
        {
            for (int i = 0; i < list2.Count; i++)
            {
                list.Add(list2[i].GetComponent<T>());
            }
        }
        return list;
    }

    public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
    {
        if (list == null)
        {
            list = new List<GameObject>();
        }
        if (!appendList)
        {
            list.Clear();
        }
        foreach (KeyValuePair<GameObject, GameObject> current in ObjectPool.Instance._spawnedObjects)
        {
            if (current.Value == prefab)
            {
                list.Add(current.Key);
            }
        }
        return list;
    }

    public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
    {
        if (list == null)
        {
            list = new List<T>();
        }
        if (!appendList)
        {
            list.Clear();
        }
        GameObject gameObject = prefab.gameObject;
        foreach (KeyValuePair<GameObject, GameObject> current in ObjectPool.Instance._spawnedObjects)
        {
            if (current.Value == gameObject)
            {
                list.Add(current.Key.GetComponent<T>());
            }
        }
        return list;
    }

    public static void DestroyPooled(GameObject prefab)
    {
        List<GameObject> list;
        if (ObjectPool.Instance._pooledObjects.TryGetValue(prefab, out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                UnityEngine.Object.Destroy(list[i]);
            }
            list.Clear();
        }
    }

    public static void DestroyAllPooled()
    {

        List<List<GameObject>> gos = new List<List<GameObject>>();
        foreach (var go in ObjectPool.Instance._pooledObjects)
        {
            gos.Add(go.Value);
        }

        foreach (var prefab in gos)
        {
            //if (ObjectPool.Instance._pooledObjects.TryGetValue(prefab, out list))
            {
                for (int i = 0; i < prefab.Count; i++)
                {
                    UnityEngine.Object.Destroy(prefab[i]);
                }
                prefab.Clear();
            }
        }

    }

    public static void DestroyPooled<T>(T prefab) where T : Component
    {
        ObjectPool.DestroyPooled(prefab.gameObject);
    }

    public static void DestroyAll(GameObject prefab)
    {
        ObjectPool.RecycleAll(prefab);
        ObjectPool.DestroyPooled(prefab);
    }

    public static void DestroyAll<T>(T prefab) where T : Component
    {
        ObjectPool.DestroyAll(prefab.gameObject);
    }
}