using System;
using UnityEngine;

namespace Utilities.Common.Settings
{
   public class SettingsDriver : MonoBehaviour
   {
      
      private ISettingsMenu[] _settingsMenus;
      private static bool _settingsLoaded;

      [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
      private static void InitializeValues()
      {
         _settingsLoaded = false;
         
      }
      
      
      private void Start()
      {
         _settingsMenus = GetComponentsInChildren<ISettingsMenu>();
         if (_settingsLoaded) return;
         _settingsLoaded = true;
         
         foreach (var menu in _settingsMenus)
         {
            menu.Load();
         }
      }

      public void Save()
      {
         _settingsMenus ??= GetComponentsInChildren<ISettingsMenu>();
         foreach (var menu in _settingsMenus)
         {
            menu.Save();
         }
      }

      private void OnDestroy()
      {
         Save();
      }
   }
}
