using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class treasureHuntScript : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	public KMSelectable[] btns;

	public Material water;
	public Material[] normal;
	public Material[] normal_solved;
	public Material[] cw;
	public Material[] cw_solved;
	public Material[] r_180;
	public Material[] r_180_solved;
	public Material[] ccw;
	public Material[] ccw_solved;

	private String[][] map = {new String[]{"Sea", "Sea", "Sea", "The Spine", "Sea", "Parrot's Sandbar", "Sea"},
							  new String[]{"Sea", "Safe Haven", "Gathering Point", "Sea", "Sea", "Sea", "Eagle Cliffs"},
							  new String[]{"Port Gloria", "Sea", "Sea", "Sea", "Pirate's Bay", "Sea", "Sea"},
							  new String[]{"Sea", "Sea", "The Maelstrom", "Sea", "Sea", "Sea", "Sea"},
							  new String[]{"Sea", "Snake Island", "Sea", "Sea", "The Shoal", "Sea", "Shipwreck Passage"},
							  new String[]{"Dead Man's Grave", "Sea", "The Three Sisters", "Sea", "Sea", "Hell's Peak", "Sea"},
							  new String[]{"Sea", "Sea", "Sea", "Siren's Lake", "Sea", "Sea", "Sea"},
							  new String[]{"El Tiburón", "Sea", "Sea", "Sea", "The Twins", "Sea", "Sea"},
							  new String[]{"Sea", "Kraken's Lair", "Sea", "Skull Desert", "Sea", "Sea", "World's End"}};

	private int[][] map_ref = {new int[]{-1, -1, -1, 16, -1, 6, -1},
							   new int[]{-1, 9, 3, -1, -1, -1, 1},
							   new int[]{8, -1, -1, -1, 7, -1, -1},
							   new int[]{-1, -1, 14, -1, -1, -1, -1},
							   new int[]{-1, 13, -1, -1, 15, -1, 10},
							   new int[]{0, -1, 18, -1, -1, 4, -1},
							   new int[]{-1, -1, -1, 11, -1, -1, -1},
							   new int[]{2, -1, -1, -1, 17, -1, -1},
							   new int[]{-1, 5, -1, 12, -1, -1, 19}};

	int current_row, current_col, trgt_row, trgt_col;
	int rotation;
	int keyModules, keyModulesSolved = 0, modulesSolved = 0; 

	private KeyValuePair<int, int>[] btn_actions = new KeyValuePair<int, int>[4];

	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = false;

	void Awake()
	{
		moduleId = moduleIdCounter++;

		btns[0].OnInteract += delegate () { PressArrowButton(0); return false; };
		btns[1].OnInteract += delegate () { PressArrowButton(1); return false; };
		btns[2].OnInteract += delegate () { PressArrowButton(2); return false; };
		btns[3].OnInteract += delegate () { PressArrowButton(3); return false; };
		btns[4].OnInteract += delegate () { PressScreen(); return false; };
	}

	void Start () 
	{
		CalcKeyModules();
		CalcRotation();
		CalcButtonAction();
		CalcStartingPos();
	}
	
	void Update () 
	{
		if(!moduleSolved && modulesSolved != bomb.GetSolvedModuleNames().Count)
		{
			List<string> solved_modules = bomb.GetSolvedModuleNames();
			modulesSolved = solved_modules.Count;

			keyModulesSolved = 0;

			foreach(string module in solved_modules)
			{
				if(module == "Coordinates" || 
				module == "Maritime Flags" ||
				module == "Semaphore"  ||
				module == "Battleship" || 
				module == "The Jewel Vault" || 
				module == "Splitting The Loot" ||
				module == "Combination Lock" ||
				module == "Safety Safe" ||
				module == "Constellations")
				{
					keyModulesSolved++;
				}
			}

			KeyValuePair<int, int> treasureIsland = GetTreasureIslandCoordinates();
			trgt_row = treasureIsland.Key;
			trgt_col = treasureIsland.Value;

			Debug.LogFormat("[Treasure Hunt #{0}] Module solved in the bomb. Number of Key Modules solved is {1}. Current Treasure Island is {2}.", moduleId, keyModulesSolved, map[trgt_row][trgt_col]);
		}
	}

	void CalcKeyModules()
	{
		List<string> modules = bomb.GetSolvableModuleNames();

		foreach(string module in modules)
		{
			if(module == "Coordinates" || 
			   module == "Maritime Flags" ||
			   module == "Semaphore"  ||
			   module == "Battleship" || 
			   module == "The Jewel Vault" || 
			   module == "Splitting The Loot" ||
			   module == "Combination Lock" ||
			   module == "Safety Safe" ||
			   module == "Constellations")
			{
				keyModules++;
			}
		}

		KeyValuePair<int, int> treasureIsland = GetTreasureIslandCoordinates();
		trgt_row = treasureIsland.Key;
		trgt_col = treasureIsland.Value;

		Debug.LogFormat("[Treasure Hunt #{0}] {1} Key Module(s) in the bomb. Starting Treasure Island is {2}.", moduleId, keyModules, map[trgt_row][trgt_col]);
	}

	void CalcRotation()
	{
		rotation = rnd.Range(0, 4);

		Debug.LogFormat("[Treasure Hunt #{0}] The map is rotated {1}º clockwise.", moduleId, rotation * 90);
	}

	void CalcButtonAction()
	{
		switch(rotation)
		{
			case 0:
			{
				btn_actions[0] = new KeyValuePair<int, int>(-1, 0);
				btn_actions[1] = new KeyValuePair<int, int>(1, 0);
				btn_actions[2] = new KeyValuePair<int, int>(0, 1);
				btn_actions[3] = new KeyValuePair<int, int>(0, -1);
				break;
			}
			case 1:
			{
				btn_actions[0] = new KeyValuePair<int, int>(0, -1);
				btn_actions[1] = new KeyValuePair<int, int>(0, 1);
				btn_actions[2] = new KeyValuePair<int, int>(-1, 0);
				btn_actions[3] = new KeyValuePair<int, int>(1, 0);
				break;
			}
			case 2:
			{
				btn_actions[0] = new KeyValuePair<int, int>(1, 0);
				btn_actions[1] = new KeyValuePair<int, int>(-1, 0);
				btn_actions[2] = new KeyValuePair<int, int>(0, -1);
				btn_actions[3] = new KeyValuePair<int, int>(0, 1);
				break;
			}
			case 3:
			{
				btn_actions[0] = new KeyValuePair<int, int>(0, 1);
				btn_actions[1] = new KeyValuePair<int, int>(0, -1);
				btn_actions[2] = new KeyValuePair<int, int>(1, 0);
				btn_actions[3] = new KeyValuePair<int, int>(-1, 0);
				break;
			}
		}
	}

	void CalcStartingPos()
	{
		current_row = rnd.Range(0, 9);
		current_col = rnd.Range(0, 7);

		while(map_ref[current_row][current_col] == -1)
		{
			current_row = rnd.Range(0, 9);
			current_col = rnd.Range(0, 7);
		}

		Debug.LogFormat("[Treasure Hunt #{0}] The starting position is {1} at [{2}, {3}] ([row, column], non-rotated map).", moduleId, map[current_row][current_col], current_row + 1, current_col + 1);

		btns[4].GetComponentInChildren<Renderer>().material = GetCurrentPosImage();
	}

	Material GetCurrentPosImage()
	{
		if(map_ref[current_row][current_col] == -1)
		{
			return water;
		}
		else
		{
			switch(rotation)
			{
				case 0:
				{
					if(moduleSolved)
						return normal_solved[map_ref[current_row][current_col]];
					else
						return normal[map_ref[current_row][current_col]];
				} 
				case 1:
				{
					if(moduleSolved)
						return cw_solved[map_ref[current_row][current_col]];
					else
						return cw[map_ref[current_row][current_col]];
				} 
				case 2:
				{
					if(moduleSolved)
						return r_180_solved[map_ref[current_row][current_col]];
					else
						return r_180[map_ref[current_row][current_col]];
				} 
				case 3:
				{
					if(moduleSolved)
						return ccw_solved[map_ref[current_row][current_col]];
					else
						return ccw[map_ref[current_row][current_col]];
				} 
				default:
				{
					return null;
				}
			}
		}
	}

	void PressArrowButton(int btn)
	{
		btns[btn].AddInteractionPunch(.5f);
		if(!moduleSolved)
		{
			int temp_row_pos = current_row + btn_actions[btn].Key;
			int temp_col_pos = current_col + btn_actions[btn].Value;

			if(temp_row_pos < 0 || temp_row_pos > 8 || temp_col_pos < 0 || temp_col_pos > 6)
			{
				Debug.LogFormat("[Treasure Hunt #{0}] Strike! Tried to sail out of bounds at {1} - [{2}, {3}] ([row, column], non-rotated map).", moduleId, map[current_row][current_col], current_row + 1, current_col + 1);
				GetComponent<KMBombModule>().HandleStrike();
			}
			else
			{
				Audio.PlaySoundAtTransform("water", transform);
				Debug.LogFormat("[Treasure Hunt #{0}] Sailed from {1} - [{2}, {3}] to {4} - [{5}, {6}] ([row, column], non-rotated map).", moduleId, map[current_row][current_col], current_row + 1, current_col + 1, map[temp_row_pos][temp_col_pos], temp_row_pos + 1, temp_col_pos + 1);
				current_row = temp_row_pos;
				current_col = temp_col_pos;
				btns[4].GetComponentInChildren<Renderer>().material = GetCurrentPosImage();
			}
		}
		else
		{
			GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		}
	}

	void PressScreen()
	{
		if(moduleSolved)
			return;

		if(current_row == trgt_row && current_col == trgt_col)
		{
			Audio.PlaySoundAtTransform("writing", transform);
			moduleSolved = true;
			btns[4].GetComponentInChildren<Renderer>().material = GetCurrentPosImage();
			Debug.LogFormat("[Treasure Hunt #{0}] Solved! Selected {1} at [{2}, {3}] as Treasure Island. ([row, column], non-rotated map).", moduleId, map[current_row][current_col], current_row + 1, current_col + 1);
			GetComponent<KMBombModule>().HandlePass();
		}
		else
		{
			Debug.LogFormat("[Treasure Hunt #{0}] Strike! Selected {1} at [{2}, {3}] as Treasure Island. Actual Treasure Island is {4} at [{5}, {6}] ([row, column], non-rotated map).", moduleId, map[current_row][current_col], current_row + 1, current_col + 1, map[trgt_row][trgt_col], trgt_row + 1, trgt_col + 1);
			GetComponent<KMBombModule>().HandleStrike();
		}
	}

	KeyValuePair<int, int> GetTreasureIslandCoordinates()
	{
		switch(keyModules)
		{
			case 0:
			{
				return new KeyValuePair<int, int>(2, 4);
			}
			case 1:
			{
				switch(keyModulesSolved)
				{
					case 0:
					{
						return new KeyValuePair<int, int>(8, 1);
					}
					case 1:
					{
						return new KeyValuePair<int, int>(5, 2);						
					}
				}
				break;
			}
			case 2:
			{
				switch(keyModulesSolved)
				{
					case 0:
					{
						return new KeyValuePair<int, int>(7, 0);
					}
					case 1:
					{
						return new KeyValuePair<int, int>(8, 6);						
					}
					case 2:
					{
						return new KeyValuePair<int, int>(4, 6);						
					}
				}
				break;

			}
			case 3:
			{
				switch(keyModulesSolved)
				{
					case 0:
					{
						return new KeyValuePair<int, int>(5, 5);
					}
					case 1:
					{
						return new KeyValuePair<int, int>(3, 2);						
					}
					case 2:
					{
						return new KeyValuePair<int, int>(0, 5);						
					}
					case 3:
					{
						return new KeyValuePair<int, int>(1, 2);						
					}
				}
				break;
			}
			case 4:
			{
				switch(keyModulesSolved)
				{
					case 0:
					{
						return new KeyValuePair<int, int>(7, 4);
					}
					case 1:
					{
						return new KeyValuePair<int, int>(4, 1);						
					}
					case 2:
					{
						return new KeyValuePair<int, int>(4, 4);						
					}
					case 3:
					{
						return new KeyValuePair<int, int>(1, 6);						
					}
					case 4:
					{
						return new KeyValuePair<int, int>(8, 3);						
					}
				}
				break;
			}
			default:
			{
				switch(keyModulesSolved)
				{
					case 0:
					{
						return new KeyValuePair<int, int>(2, 0);
					}
					case 1:
					{
						return new KeyValuePair<int, int>(0, 3);						
					}
					case 2:
					{
						return new KeyValuePair<int, int>(5, 0);						
					}
					case 3:
					{
						return new KeyValuePair<int, int>(1, 1);						
					}
					default:
					{
						return new KeyValuePair<int, int>(6, 3);						
					}
				}
			}
		}

		return new KeyValuePair<int, int>();
	}

    //twitch plays
    private bool charsAreValid(string s)
    {
        char[] valids = { 'u', 'd', 'l', 'r' };
        for(int i = 0; i < s.Length; i++)
        {
            if (!valids.Contains(s.ElementAt(i)))
            {
                return false;
            }
        }
        return true;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} udlr [Move the specified directions in order; u = up, r = right, d = down, l = left] | !{0} screen [Presses the screen on the module]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*screen\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            btns[4].OnInteract();
            yield break;
        }
        command = command.Trim();
        if (charsAreValid(command))
        {
            for(int i = 0; i < command.Length; i++)
            {
                if (command.ElementAt(i).Equals('u'))
                {
                    btns[0].OnInteract();
                }
                else if (command.ElementAt(i).Equals('d'))
                {
                    btns[1].OnInteract();
                }
                else if (command.ElementAt(i).Equals('r'))
                {
                    btns[2].OnInteract();
                }
                else if (command.ElementAt(i).Equals('l'))
                {
                    btns[3].OnInteract();
                }
                yield return new WaitForSeconds(0.2f);
            }
            yield break;
        }
    }
}
