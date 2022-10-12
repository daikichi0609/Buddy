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

	public static PlayerStatus.PlayerParameter LoadPlayerScriptableObject(Define.CHARA_NAME name)
	{
		PlayerStatus.PlayerParameter param = new PlayerStatus.PlayerParameter();
		PlayerStatus.PlayerParameter constParam = LoadPlayerParameter(name) as PlayerStatus.PlayerParameter;

		param.Name = constParam.Name;
		param.Hp = constParam.Hp;
		param.Atk = constParam.Atk;
		param.Def = constParam.Def;
		param.Agi = constParam.Agi;
		param.Dex = constParam.Dex;
		param.Eva = constParam.Eva;
		param.CriticalRate = constParam.CriticalRate;
		param.Res = constParam.Res;

		param.Luk = constParam.Luk;

		return param;
    }

	public static EnemyStatus.EnemyParameter LoadEnemyScriptableObject(Define.CHARA_NAME name)
	{
		EnemyStatus.EnemyParameter param = new EnemyStatus.EnemyParameter();
		EnemyStatus.EnemyParameter constParam = LoadPlayerParameter(name) as EnemyStatus.EnemyParameter;

		param.Name = constParam.Name;
		param.Hp = constParam.Hp;
		param.Atk = constParam.Atk;
		param.Def = constParam.Def;
		param.Agi = constParam.Agi;
		param.Dex = constParam.Dex;
		param.Eva = constParam.Eva;
		param.CriticalRate = constParam.CriticalRate;
		param.Res = constParam.Res;

		param.Ex = constParam.Ex;

		return param;
	}

	public static void SaveScriptableObject(BattleStatus status)
    {
		EditorUtility.SetDirty(status);
		AssetDatabase.SaveAssets();
	}

	public static BattleStatus.Parameter LoadPlayerParameter(Define.CHARA_NAME name)
    {
		PlayerStatus playerStatus = Resources.Load<PlayerStatus>(name.ToString());
		if(playerStatus != null)
        {
			return playerStatus.Param;
        }

		EnemyStatus enemyStatus = Resources.Load<EnemyStatus>(name.ToString());
		if (enemyStatus != null)
        {
			return enemyStatus.Param;
        }

		Debug.LogError("キャラクターステータスの読み込みに失敗しました");
		return null;
	}
}