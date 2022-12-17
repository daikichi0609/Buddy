using UnityEngine;

public interface ICharacterComponent
{
    void Initialize();
}

public abstract class CharaComponentBase : MonoBehaviour, ICharacterComponent
{
    /// <summary>
    /// コレクター
    /// </summary>
    protected ICollector Collector { get; set; }

    /// <summary>
    /// 登録処理
    /// </summary>
    private void Awake()
    {
        Collector = GetComponent<CharacterComponentCollector>();
        Collector.Register(this);
    }

    private void Start()
    {
        Initialize();
    }

    protected virtual void Initialize() { }
    void ICharacterComponent.Initialize() => Initialize();
}