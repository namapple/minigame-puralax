using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "All Levels", menuName = "Level Creation/All Levels")]
public class LevelDictionary : ScriptableObject
{
   [SerializeField] private List<Level> allLevels;
   private Dictionary<string, Level> levels = new Dictionary<string, Level>();

   private void OnEnable()
   {
      foreach (Level item in allLevels)
      {
         levels[item.levelName] = item;
      }
   }

   public Level GetLevel(string name)
   {
      levels.TryGetValue(name, out Level result);
      return result;
   }
}
