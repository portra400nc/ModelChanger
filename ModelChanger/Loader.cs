using System;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace ModelChanger
{
    public static class BuildInfo
    {
        public const string Name = "Model Changer";
        public const string Description = null;
        public const string Author = "portra";
        public const string Company = null;
        public const string Version = "0.0.1";
        public const string DownloadLink = null;
    }
    public class Loader : MelonMod
    {
        public static Action<string> Msg;
        public static Action<string> Warning;
        public static Action<string> Error;
        
        private GameObject _texChangerObj;

        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<Main>();
            Msg = LoggerInstance.Msg;
            Warning = LoggerInstance.Warning;
            Error = LoggerInstance.Error;
        }
        
        public override void OnApplicationLateStart()
        {
            Load();
        }
        
        private void Load()
        {
            if (_texChangerObj != null) return;
            _texChangerObj = new GameObject();
            _texChangerObj.AddComponent<Main>();
            GameObject.DontDestroyOnLoad(_texChangerObj);
        }
    }
}