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

        private GameObject _avatarRoot;
        private GameObject _activeAvatar;
        private GameObject _activeAvatarBody;
        private GameObject _activeAvatarModelParent;
        private List<GameObject> _bodyParts = new List<GameObject>();
        private string _filePath;
        private string _avatarTexName = "texture.png";
        private byte[] _fileData;
        private Texture2D _tex;
        private bool _showPanel;

        private Rect _windowRect = new Rect((Screen.height - 100) / 2, (Screen.height - 100) / 2, 150, 100);

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
                CopyAvatarBody();
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

        private void CopyAvatarBody()
        {
            _bodyParts.Clear();
            foreach (var o in _activeAvatarModelParent.transform)
            {
                var bodypart = o.Cast<Transform>();
                _bodyParts.Add(bodypart.gameObject);
                Loader.Msg($"Added {bodypart.name} of {_activeAvatarModelParent.transform.name} to the list.");
            }
        }

        private void PasteAvatarBody()
        {
            MigrateObjects();
            
            foreach (var bodypart in _bodyParts)
            {
                bodypart.transform.parent = _activeAvatarModelParent.transform;
            }
        }

        private void MigrateObjects()
        {
            var newObj = new GameObject();
            foreach (var a in _activeAvatarModelParent.transform)
            {
                var bodypart = a.Cast<Transform>();
                bodypart.parent = newObj.transform;
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
    }
}