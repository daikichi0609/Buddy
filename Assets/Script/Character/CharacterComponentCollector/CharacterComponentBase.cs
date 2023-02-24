using UnityEngine;

public interface ICharacterComponent
{
    /// <summary>
    /// コンポーネント初期化
    /// </summary>
    void Initialize();
}

public abstract class CharaComponentBase : MonoBehaviour, ICharacterComponent
{
    /// <summary>
    /// コレクター
    /// </summary>
    protected ICollector Owner { get; set; }

    /// <summary>
    /// Owner取得
    /// </summary>
    private void Awake()
    {
        Owner = GetComponent<CharacterComponentCollector>();
        Register(Owner);
    }

    protected virtual void Register(ICollector owner)
    {
        // コンポーネント登録
    }

    protected virtual void Initialize()
    {
        // コンポーネント初期化
    }

    void ICharacterComponent.Initialize() => Initialize();
}