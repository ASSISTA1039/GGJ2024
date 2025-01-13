using QxFramework.Core;

public abstract class LogicModuleBase
{
    public virtual void Init()
    {
    }

    protected bool RegisterData<T>(out T data, string key = "Default") where T : GameDataBase, new()
    {
        return QXData.Instance.InitData<T>(out data, key);
    }

    protected void SetModify<T>(T data, string key = "Default") where T : GameDataBase, new()
    {
        QXData.Instance.SetModify<T>(data, this, key);
    }

    public virtual void Update()
    {
    }

    public virtual void LateUpdate()
    {
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void OnDestroy()
    {
    }
}