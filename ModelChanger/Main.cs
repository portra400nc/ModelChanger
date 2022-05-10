using System;
using System.Collections.Generic;
using System.IO;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace ModelChanger
{
    public class Main : MonoBehaviour
    {
        public Main(IntPtr ptr) : base(ptr)
        {
        }

        public Main() : base(ClassInjector.DerivedConstructorPointer<Main>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        #region Properties

        private GameObject _avatarRoot;
        private GameObject _activeAvatar;
        private GameObject _activeAvatarBody;
        private GameObject _activeAvatarModelParent;
        private GameObject _weaponRoot;
        private GameObject _weaponRootParent;
        private GameObject _weaponL;
        private GameObject _weaponLParent;
        private GameObject _weaponR;
        private GameObject _weaponRParent;
        private List<GameObject> _bodyParts = new List<GameObject>();
        private string _filePath;
        private string _avatarTexName = "texture.png";
        private byte[] _fileData;
        private Texture2D _tex;
        private bool _showPanel;

        private Rect _windowRect = new Rect((Screen.height - 100) / 2, (Screen.height - 100) / 2, 150, 100);
        #endregion
        
        public void OnGUI()
        {
            if (!_showPanel) return;
            _windowRect = GUILayout.Window(4, _windowRect, (GUI.WindowFunction) TexWindow, "Model Changer",
                new GUILayoutOption[0]);
        }

        public void TexWindow(int id)
        {
            if (id != 4) return;
            GUILayout.Space(10);

            GUILayout.Label("Character Texture", new GUILayoutOption[0]);
            _avatarTexName = GUILayout.TextField(_avatarTexName, new GUILayoutOption[0]);
            if (GUILayout.Button("Apply", new GUILayoutOption[0]))
                ApplyAvatarTexture();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Cut", new GUILayoutOption[0]))
                CutAvatarBody();
            if (GUILayout.Button("Paste", new GUILayoutOption[0]))
                PasteAvatarBody();
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                _showPanel = !_showPanel;
            if (_showPanel)
                Focused = false;

            if (_avatarRoot == null)
                _avatarRoot = GameObject.Find("/EntityRoot/AvatarRoot");

            if (!_avatarRoot) return;
            try
            {
                if (_activeAvatar == null)
                    FindActiveAvatar();
                if (!_activeAvatar.activeInHierarchy)
                    FindActiveAvatar();
            }
            catch
            {
            }
        }

        #region MainFunctions

        private void CutAvatarBody()
        {
            _bodyParts.Clear();
            foreach (var o in _activeAvatarModelParent.transform)
            {
                var bodypart = o.Cast<Transform>();
                _bodyParts.Add(bodypart.gameObject);
                Loader.Msg($"Added {bodypart.name} of {_activeAvatarModelParent.transform.name} to the list.");
            }

            foreach (var o in _activeAvatarModelParent.GetComponentsInChildren<Transform>())
            {
                switch (o.name)
                {
                    case "Bip001 Spine1":
                        _weaponRootParent = o.gameObject;
                        Loader.Msg($"Found {_weaponRootParent.name}.");
                        break;
                    case "Bip001 L Hand":
                        _weaponLParent = o.gameObject;
                        Loader.Msg($"Found {_weaponLParent.name}");
                        break;
                    case "Bip001 R Hand":
                        _weaponRParent = o.gameObject;
                        Loader.Msg($"Found {_weaponRParent.name}");
                        break;
                }
            }
        }

        private void PasteAvatarBody()
        {
            HideObjects();
            foreach (var bodypart in _bodyParts)
            {
                bodypart.transform.parent = _activeAvatarModelParent.transform;
                bodypart.transform.parentInternal = _activeAvatarModelParent.transform;
                bodypart.transform.SetSiblingIndex(0);
                Loader.Msg($"{bodypart.name} moved to {_activeAvatarModelParent.name}");
            }
            _weaponRoot.transform.parent = _weaponRootParent.transform;
            _weaponL.transform.parent = _weaponLParent.transform;
            _weaponR.transform.parent = _weaponRParent.transform;
        }

        private void HideObjects()
        {
            foreach (var a in _activeAvatarModelParent.transform)
            {
                var bodypart = a.Cast<Transform>();
                bodypart.gameObject.SetActive(false);
            }
        }

        private void ApplyAvatarTexture()
        {
            if (_activeAvatarBody == null) return;

            _filePath = Path.Combine(Application.dataPath, "tex_test", _avatarTexName);
            _fileData = File.ReadAllBytes(_filePath);
            _tex = new Texture2D(1024, 1024);
            ImageConversion.LoadImage(_tex, _fileData);

            _activeAvatarBody.GetComponent<SkinnedMeshRenderer>().materials[1].mainTexture = _tex;
        }

        #endregion

        #region HelperFunctions

        private void FindActiveAvatar()
        {
            if (_avatarRoot.transform.childCount == 0) return;
            foreach (var a in _avatarRoot.transform)
            {
                var active = a.Cast<Transform>();
                if (!active.gameObject.activeInHierarchy) continue;
                _activeAvatar = active.gameObject;
                FindBody();
            }
        }

        private void FindBody()
        {
            foreach (var a in _activeAvatar.GetComponentsInChildren<Transform>())
            {
                switch (a.name)
                {
                    case "Body":
                        _activeAvatarBody = a.gameObject;
                        break;
                    case "OffsetDummy":
                        _activeAvatarModelParent = a.GetChild(0).gameObject;
                        Loader.Msg($"{_activeAvatarModelParent.transform.name}");
                        break;
                    case "WeaponL":
                        _weaponL = a.gameObject;
                        Loader.Msg($"Found {_weaponL.name}");
                        break;
                    case "WeaponR":
                        _weaponR = a.gameObject;
                        Loader.Msg($"Found {_weaponR.name}");
                        break;
                }

                if (a.name.Contains("WeaponRoot"))
                {
                    _weaponRoot = a.gameObject;
                    Loader.Msg($"Found {_weaponRoot.name}");
                }
            }
        }

        private static bool Focused
        {
            get => Cursor.lockState == CursorLockMode.Locked;
            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = value == false;
            }
        }

        #endregion
    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}