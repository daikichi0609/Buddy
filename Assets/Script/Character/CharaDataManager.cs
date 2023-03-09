using UnityEngine;
using System.IO;
using UnityEditor;

public static class CharaDataManager
{
    private static string m_Datapath = Application.dataPath + "/Resources/TestJson.json";
    public static string Datapath
    {
        get { return m_Datapath; }
    }

    //セーブのメソッド
    public static void SaveTest(PlayerStatus data)
    {
        string jsonstr = JsonUtility.ToJson(data);//受け取ったPlayerDataをJSONに変換
        StreamWriter writer = new StreamWriter(m_Datapath, false);//初めに指定したデータの保存先を開く
        writer.WriteLine(jsonstr);//JSONデータを書き込み
        writer.Flush();//バッファをクリアする
        writer.Close();//ファイルをクローズする
    }

    public static string LoadTest(string dataPath)
    {
        StreamReader reader = new StreamReader(dataPath); //受け取ったパスのファイルを読み込む
        string datastr = reader.ReadToEnd();//ファイルの中身をすべて読み込む
        reader.Close();//ファイルを閉じる

        return datastr;//読み込んだJSONファイルをstring型に変換して返す
    }

    public static PlayerStatus.PlayerParameter LoadPlayerScriptableObject(CHARA_NAME name)
    {
        PlayerStatus.PlayerParameter param = LoadCharaParameter(name) as PlayerStatus.PlayerParameter;
        return new PlayerStatus.PlayerParameter(param);
    }

    public static EnemyStatus.EnemyParameter LoadEnemyScriptableObject(CHARA_NAME name)
    {
        EnemyStatus.EnemyParameter param = LoadCharaParameter(name) as EnemyStatus.EnemyParameter;
        return new EnemyStatus.EnemyParameter(param);
    }

    public static void SaveScriptableObject(BattleStatus status)
    {
        EditorUtility.SetDirty(status);
        AssetDatabase.SaveAssets();
    }

    public static BattleStatus.Parameter LoadCharaParameter(CHARA_NAME name)
    {
        PlayerStatus playerParam = Resources.Load<PlayerStatus>("Character/" + name.ToString());
        if (playerParam != null)
        {
            return playerParam.Param;
        }

        EnemyStatus enemyParam = Resources.Load<EnemyStatus>("Character/" + name.ToString());
        if (enemyParam != null)
        {
            return enemyParam.Param;
        }

        Debug.LogError("キャラクターステータスの読み込みに失敗しました");
        return null;
    }
}