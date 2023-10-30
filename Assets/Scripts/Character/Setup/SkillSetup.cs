using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using UnityEditor;

[CreateAssetMenu(menuName = "MyScriptable/Skill/Setup")]
public class SkillSetup : ScriptableObject
{
    /// <summary>
    /// スキル
    /// </summary>
    [SerializeField, Header("スキル"), ReadOnly]
    private List<Skill> m_Skills = new List<Skill>();
    public ISkill[] SkillEffects => m_Skills.ToArray();

    /// <summary>
    /// 生成するスキル
    /// </summary>
    [SerializeField, Header("作成するスキルアセット")]
    [Dropdown("GetEffectType")]
    private string m_Type;

#if UNITY_EDITOR
    /// <summary>
    /// トラップタイプ取得
    /// </summary>
    /// <returns></returns>
    private DropdownList<string> GetEffectType()
    {
        return new DropdownList<string>()
        {
            { "れんぞくぎり", typeof(ContinuousSlash).FullName },
            { "かいてんぎり", typeof(SpinningSlash).FullName },
            { "しんくうぎり", typeof(VaccumSlash).FullName },
            { "ラゴン・スピア", typeof(RagonSpear).FullName },
            { "ぶちかまし", typeof(Buchikamashi).FullName},
            { "ベリベリヒール", typeof(HealAround).FullName},
            { "眷属召喚", typeof(SummonKin).FullName},
            { "アイススパイク", typeof(IceSpike).FullName},
            { "エクスプロージョン", typeof(Explosion).FullName},
            { "トルネード", typeof(Tornade).FullName},
            { "ロスト・ワン", typeof(LostOne).FullName},
            { "ジャッジメント", typeof(Judgement).FullName},
            { "ホーリーインパクト", typeof(HolyImpact).FullName},
        };
    }

    /// <summary>
    /// 効果アセットがすでにあるなら消して新しく作り直す
    /// </summary>
    /// <returns></returns>
    [Button]
    private void CreateEffectAsset()
    {
        var t = Type.GetType(m_Type);
        var skill = CreateAssetInternal(t);

        AssetDatabase.AddObjectToAsset(skill, this);
        AssetDatabase.SaveAssets();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(skill);
    }

    /// <summary>
    /// 効果アセットがすでにあるなら消す
    /// </summary>
    /// <returns></returns>
    [Button]
    private void DestroyEffectAsset()
    {
        foreach (var skill in m_Skills)
            DestroyImmediate(skill, true);

        m_Skills.Clear();
    }

    /// <summary>
    /// 効果アセット作成
    /// </summary>
    /// <param name="type"></param>
    private Skill CreateAssetInternal(Type type)
    {
        var skill = (Skill)ScriptableObject.CreateInstance(type);
        m_Skills.Add(skill);
        skill.name = type.ToString();
        return skill;
    }
#endif
}
