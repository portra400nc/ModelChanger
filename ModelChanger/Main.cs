using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private GameObject _npcRoot;
        private GameObject _monsterRoot;
        private GameObject _activeAvatar;
        private GameObject _activeAvatarBody;
        private GameObject _activeAvatarModelParent;
        private GameObject _weaponRoot;
        private GameObject _weaponRootParent;
        private GameObject _weaponL;
        private GameObject _weaponLParent;
        private GameObject _weaponR;
        private GameObject _weaponRParent;
        private GameObject _npcAvatarModelParent;
        private GameObject _npcWeaponL;
        private GameObject _npcWeaponR;
        private GameObject _npcWeaponRoot;
        private List<GameObject> _bodyParts = new List<GameObject>();
        private List<GameObject> _npcBodyParts = new List<GameObject>();
        private List<GameObject> _searchResults = new List<GameObject>();
        private string _filePath;
        private string _avatarTexName = "texture.png";
        private string _avatarSearch;
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

            GUILayout.Label("Search", new GUILayoutOption[0]);
            _avatarSearch = GUILayout.TextField(_avatarSearch, new GUILayoutOption[0]);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Search", new GUILayoutOption[0]))
                SearchObjects();
            if (GUILayout.Button("Clear", new GUILayoutOption[0]))
                _searchResults.Clear();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (_searchResults.Count > 0)
            {
                foreach (var result in _searchResults)
                {
                    if (!GUILayout.Button($"{result.transform.name}", new GUILayoutOption[0])) continue;
                    NpcAvatarChanger(result.gameObject);
                }
            }

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
            if (_npcRoot == null)
                _npcRoot = GameObject.Find("/EntityRoot/NPCRoot");
            if (_monsterRoot == null)
                _monsterRoot = GameObject.Find("/EntityRoot/MonsterRoot");

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

            _searchResults = _searchResults.Where(item => item != null).ToList();
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

        private void SearchObjects()
        {
            _searchResults.Clear();
            if (_monsterRoot)
            {
                foreach (var a in _monsterRoot.transform)
                {
                    var monster = a.Cast<Transform>();
                    if (monster.name.Contains(_avatarSearch, StringComparison.OrdinalIgnoreCase))
                        _searchResults.Add(monster.gameObject);
                }
            }

            if (_npcRoot)
            {
                foreach (var a in _npcRoot.transform)
                {
                    var npc = a.Cast<Transform>();
                    if (npc.name.Contains(_avatarSearch, StringComparison.OrdinalIgnoreCase))
                        _searchResults.Add(npc.gameObject);
                }
            }
        }

        //by Rin
        private void NpcAvatarChanger(GameObject searchResult)
        {
            foreach (var a in searchResult.GetComponentsInChildren<Transform>())
            {
                if (a.name == "OffsetDummy")
                {
                    _npcAvatarModelParent = a.GetChild(0).gameObject;
                    Loader.Msg($"{_npcAvatarModelParent.transform.name}");
                }

                if (a.name.Contains("Body"))
                {
                    _npcAvatarModelParent = a.gameObject.transform.parent.gameObject;
                }
            }

            _npcBodyParts.Clear(); //清空_npcBodyParts数组
            foreach (var o in _npcAvatarModelParent.transform) //遍历NPC的父对象的transform组件并存于o
            {
                var npcBodypart = o.Cast<Transform>(); //将o铸型并存与bodypart
                _npcBodyParts.Add(npcBodypart.gameObject); //将找到的gameObject保存在_bodyParts数组里
                Loader.Msg($"Added {npcBodypart.name} of {_npcAvatarModelParent.transform.name} to the list."); //打印
            }

            foreach (var o in _npcAvatarModelParent.GetComponentsInChildren<Transform>()) //遍历活动角色的父项的子集并存于o
            {
                switch (o.name) //判断 o名称 查找武器
                {
                    case "Bip001 Spine1": //case Bip001 Spine1
                        _npcWeaponRoot = o.gameObject; //武器路劲o.gameObject
                        Loader.Msg($"Found {_npcWeaponRoot.name}."); //找到Bip001 Spine1
                        break;
                    case "Bip001 L Hand":
                        _npcWeaponL = o.gameObject;
                        Loader.Msg($"Found {_npcWeaponL.name}");
                        break;
                    case "Bip001 R Hand":
                        _npcWeaponR = o.gameObject;
                        Loader.Msg($"Found {_npcWeaponR.name}");
                        break;
                }
            }

            HideObjects(); //隐藏当前角色parts
            foreach (var npcBodypart in _npcBodyParts) //遍历_bodyparts存放于bodypart
            {
                npcBodypart.transform.parent = _activeAvatarModelParent.transform;
                npcBodypart.transform.parentInternal = _activeAvatarModelParent.transform;
                npcBodypart.transform.SetSiblingIndex(0);
                Loader.Msg($"{npcBodypart.name} moved to {_activeAvatarModelParent.name}");
            }
        }
        //by Rin

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