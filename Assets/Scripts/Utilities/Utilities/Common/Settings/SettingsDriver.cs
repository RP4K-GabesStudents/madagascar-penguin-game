using UnityEngine;

namespace Utilities.Utilities.Common.Settings
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
      
      
      private void Awake()
      {
         _settingsMenus = GetComponentsInChildren<ISettingsMenu>(true);
         if (_settingsLoaded) return;
         _settingsLoaded = true;
         
         Debug.Log("Initializing Settings");
         foreach (var menu in _settingsMenus)
         {
            menu.Load();
         }
      }

      public void Save()
      {
         _settingsMenus ??= GetComponentsInChildren<ISettingsMenu>(true);
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
