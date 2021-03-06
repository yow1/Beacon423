﻿using UnityEngine;
using System.Collections;

public static class MapManager
{
	public static int _height = 18;
	public static int _width = 10;
	public static string _curMap; // 不能让外部直接访问
	public static GameObject[] _gos; 

	public static void LoadMap()
	{
		TextAsset ta = (TextAsset)Resources.Load("Map/Map_" + GameData._CurLevel); 
		if (ta == null)
		{
			return; 
		}

		Debug.Log("Origin data: " + ta.text); 
		string s = ""; 
		int row = 0; 
		int column = 0; 
		bool isComment = false; 
		for (int i = 0; i < ta.text.Length; ++i)
		{
			if (!isComment && ta.text[i] == '/' && i + 1 < ta.text.Length && ta.text[i + 1] == '/')
			{
				isComment = true; 
			}
			else if (isComment && ta.text[i] == '\n')
			{
				isComment = false; 
				continue; 
			}
			if (isComment)
			{
				continue; 
			}

			if (IsValid(ta.text[i]))
			{
				s += ta.text[i]; 
			}
			else if ((ta.text[i] == '\r' || ta.text[i] == '\n') && IsValid(ta.text[i - 1]))
			{
//                Debug.LogError("isValid: " + ta.text[i - 1] + ", " + ta.text[i + 1]); 
				if (column == 0)
				{
					column = s.Length; 
				}
				++row; 
			}
		}
		Debug.Log("row: " + row + ", column: " + column); 
		Debug.Log("Obtain data: " + s); 
		_width = column; 
		_height = row; 
		MapManager._curMap = s; 
	}

	public static string GetCurMap()
	{
		return _curMap; 
	}

	public static bool IsValid(char character)
	{
		return character >= '0' && character <= '9'; 
	}

	public static void GenerateWall() // 应该与ResetMap一致
	{
		string map = _curMap;
		int width = _width;
		int height = _height; 

		GameData._Instance._wallParent = (new GameObject("WallParent")).transform; 
		GameData._Instance._wallParent.position = Vector3.zero; 
		GameData._Instance._wallPrefab.gameObject.SetActive(false); 
		_gos = new GameObject[map.Length]; 
		for (int i = 0; i < map.Length; ++i)
		{
			char destCode = map[i]; 
			Transform tf = null; 
			if (destCode == MapCode.WALL)
			{
				tf = GameObject.Instantiate(GameData._Instance._wallPrefab); 
				tf.GetComponentInChildren<TextMesh>().text = string.Format("{0}, {1}", 
					i % width, i / width); 
			}
			else if (destCode == MapCode.PIT)
			{
				tf = GameObject.Instantiate(GameData._Instance._pitPrefab); 
			}
			else if (destCode == MapCode.ENEMY)
			{
				tf = GameObject.Instantiate(GameData._Instance._enemyPrefab); 
				tf.GetComponent<Enemy>().Init(new Pos((int)(i % width - width / 2), (int)(i / width - height / 2))); 
			}
			else if (destCode == MapCode.NPC)
			{
				tf = GameObject.Instantiate(GameData._Instance._npcPrefab); 
				NPC npc = tf.GetComponent<NPC>();
				if (npc != null)
				{
					npc.Init(ERole.GrandDaughter, null); 
				}
			}
			else if (destCode == MapCode.NPC_DARK_PRINCE)
			{
				tf = GameObject.Instantiate(GameData._Instance._npcPrefab); 
				NPC npc = tf.GetComponent<NPC>();
				if (npc != null)
				{
					npc.Init(ERole.DarkPrince, null); 
				}
			}
			else
			{
				_gos[i] = null; 
			}
			if (tf != null)
			{
				tf.SetParent(GameData._Instance._wallParent); 
				tf.gameObject.SetActive(true); 
				tf.position = new Vector3(i % _width - _width / 2 + 0.5f, i / _width - _height / 2 + 0.5f, 0); 
				_gos[i] = tf.gameObject; 
			}
		}
	}

	public static void DestroyWall()
	{
		_gos = null; 
		if (GameData._Instance._wallParent != null)
		{
			GameObject.Destroy(GameData._Instance._wallParent.gameObject); 
		}
	}


	#region Change Map
	public static void ResetMap(int destIndex, char destCode, int srcIndex, char srcCode = MapCode.NONE, bool isDestroy = false)
	{
		ExchangePos(destIndex, destCode, srcIndex, srcCode); 
		ExchangeObj(destIndex, srcIndex, isDestroy); 
	}

	public static void ResetMap(int destIndex, int srcIndex, bool isDestroy = false)
	{
		ExchangePos(destIndex, srcIndex); 
		ExchangeObj(destIndex, srcIndex, isDestroy); 
	}

	public static void ResetMap(int destIndex, char destCode = MapCode.NONE, bool isDestroy = true)
	{
		if (destIndex >= _gos.Length)
		{
			return; 
		}
		char[] chars = _curMap.ToCharArray(); 
		chars[destIndex] = destCode; 
		SetMap(chars); 

		Transform tf = null; 
		if (destCode == MapCode.WALL)
		{
			tf = GameObject.Instantiate(GameData._Instance._wallPrefab); 
		}
		else if (destCode == MapCode.PIT)
		{
			tf = GameObject.Instantiate(GameData._Instance._pitPrefab); 
		}
		else if (destCode == MapCode.ENEMY)
		{
			tf = GameObject.Instantiate(GameData._Instance._enemyPrefab); 
		}
		else if (destCode == MapCode.NPC)
		{
			tf = GameObject.Instantiate(GameData._Instance._npcPrefab); 
		}
		if (tf != null)
		{
			int i = destIndex; 
			tf.SetParent(GameData._Instance._wallParent); 
			tf.gameObject.SetActive(true); 
			tf.position = new Vector3(i % _width - _width / 2 + 0.5f, i / _width - _height / 2 + 0.5f, 0); 
			_gos[i] = tf.gameObject; 
		}
		SetObj(destIndex, tf == null ? null : tf.gameObject, isDestroy); 
	}

	static void ExchangePos(int destIndex, char destCode, int srcIndex, char srcCode = MapCode.NONE)
	{
		if (destIndex >= _gos.Length || srcIndex >= _gos.Length)
		{
			return; 
		}
		char[] chars = _curMap.ToCharArray(); 
		chars[srcIndex] = srcCode; 
		chars[destIndex] = destCode; 
		SetMap(chars); 
	}

	static void ExchangePos(int destIndex, int srcIndex)
	{
		if (destIndex >= _gos.Length || srcIndex >= _gos.Length)
		{
			return; 
		}
		char[] chars = _curMap.ToCharArray(); 
		Debug.Log("srcCode: " + chars[srcIndex] + ", " + "destCode: " + chars[destIndex]); 
		char c = chars[srcIndex]; 
		chars[srcIndex] = chars[destIndex]; 
		chars[destIndex] = c; 
		SetMap(chars); 
	}

	static void SetPos(int destIndex, char destCode = MapCode.NONE)
	{
		if (destIndex >= _gos.Length)
		{
			return; 
		}
		char[] chars = _curMap.ToCharArray(); 
		chars[destIndex] = destCode; 
		SetMap(chars); 
	}

	static void SetMap(char[] chars)
	{
		if (chars == null)
		{
			return; 
		}
		string s = ""; 
		for (int i = 0, count = chars.Length; i < count; ++i)
		{
			s += chars[i]; 
		}
		_curMap = s; 
	}

	static void ExchangeObj(int destIndex, int srcIndex, bool isDestroy = false)
	{
		if (destIndex >= _gos.Length || srcIndex >= _gos.Length)
		{
			return; 
		}
		GameObject go = _gos[srcIndex]; 
		_gos[destIndex] = _gos[destIndex]; 
		_gos[destIndex] = go; 
		if (isDestroy)
		{
			GameObject.Destroy(_gos[srcIndex]); 
		}
	}
	static void SetObj(int destIndex, GameObject obj = null, bool isDestroy = true)
	{
		if (destIndex >= _gos.Length)
		{
			return; 
		} 
		Debug.Log("SetObj: " + _gos[destIndex]); 
		if (isDestroy)
		{
			Debug.Log("destroy: " + _gos[destIndex].name); 
			GameObject.Destroy(_gos[destIndex]); 
		}
		_gos[destIndex] = obj; 
	}
	#endregion




	#region Map Info

	public static GameObject GetObj(int index)
	{
		if (index >= _gos.Length)
		{
			return null; 
		}
		return _gos[index]; 
	}

	#endregion


	#region Position

	public static int CurIndex(int x, int y)
	{
		int newX = x + MapManager._width / 2; 
		int newY = y + MapManager._height / 2; 
		if (newX >= 0 && newX < MapManager._width
			&& newY >= 0 && newY < MapManager._height)
		{
			return newX + newY * MapManager._width; 
		}
		return -1; 
	}

	public static char GetCode(int x, int y)
	{
		var index = MapManager.CurIndex(x, y); 
		if (index < 0)
		{
			return MapCode.NOT_EXIST; 
		}
		return _curMap[index];  
	}

	public static bool IsExistCodeInRange(int x, int y, out int enemyX, out int enemyY, char code)
	{
		enemyX = x + 0;
		enemyY = y + 1; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		enemyX = x + 0;
		enemyY = y - 1; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		enemyX = x + 1;
		enemyY = y + 0; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		enemyX = x - 1;
		enemyY = y + 0; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		enemyX = x + 1;
		enemyY = y + 1; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		enemyX = x + 1;
		enemyY = y - 1; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		enemyX = x - 1;
		enemyY = y + 1; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		enemyX = x - 1;
		enemyY = y - 1; 
		if(MapManager.GetCode(enemyX, enemyY) == code)
		{
			return true; 
		}
		return false; 
	}
	#endregion
	public static byte[] _constDirs = new byte[4]{ MoveUtil._DIR_EAST, MoveUtil._DIR_WEST, MoveUtil._DIR_SOUTH, MoveUtil._DIR_NORTH };
	public static int Check(ref byte dir, byte constDir, Vector2 pos, char checkCode)
	{
		int curIndex = MapManager.CurIndex((int)pos.x, (int)pos.y); 
		if (curIndex < 0 || MapManager._curMap[curIndex] == checkCode)
		{
			dir |= constDir; 
			if (curIndex < 0) // 超出边界
			{
				dir = 0; 
				dir |= constDir; // 直接返回它本身，不改变constDir的值
				return 0; // 摸到门
			}
			return 1; // 墙壁
		}
		return -1; // 空气
	}
}
