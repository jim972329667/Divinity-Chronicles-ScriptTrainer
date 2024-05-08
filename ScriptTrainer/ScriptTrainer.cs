using DV;
using HarmonyLib;
using I2.Loc;
using JTW;
using ScriptTrainer.Cards;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptTrainer
{
    public class ScriptTrainer: ModComponent
    {
        public static string ModCardImgPath
        {
            get
            {
                var dir = new DirectoryInfo(Path.GetDirectoryName(typeof(ScriptTrainer).Assembly.Location));
                var card = Path.Combine(dir.Parent.FullName, "Cards");
                if(!Directory.Exists(card))
                    Directory.CreateDirectory(card);
                return card;
            }
        }
        public static string SettingFilePath
        {
            get
            {
                var dir = new DirectoryInfo(Path.GetDirectoryName(typeof(ScriptTrainer).Assembly.Location));
                var card = Path.Combine(dir.Parent.FullName, "Setting.ini");
                return card;
            }
        }

        public static ScriptTrainer Instance;
        public static AssetBundle Asset;
        public static GameObject CardUI = null;
        public static KeyCode ShowTrainer = KeyCode.F9;
        
        // 窗口相关
        public GameObject YourTrainer;
        // 启动按
        public void Awake()
        {
            Instance = this;
            #region[注入游戏补丁]
            LoadSetting();
            var harmony = new Harmony("ScriptTrainer");
            harmony.PatchAll();
            #endregion
            Debug.Log("Harmony Success!");
            #region 注入游戏修改器UI
            YourTrainer = GameObject.Find("ZG_Trainer");
            if (YourTrainer == null)
            {
                YourTrainer = new GameObject("ZG_Trainer");
                GameObject.DontDestroyOnLoad(YourTrainer);
                YourTrainer.hideFlags = HideFlags.HideAndDontSave;
                YourTrainer.AddComponent<ZGGameObject>();
            }
            else YourTrainer.AddComponent<ZGGameObject>();
            #endregion

            Debug.Log("脚本已启动");
        }

        public void Start()
        {
            Asset = AssetBundle.LoadFromStream(Assembly.GetAssembly(typeof(ScriptTrainer)).GetManifestResourceStream("ScriptTrainer.dynamiccardui"));
            Debug.Log($"载入Asset成功！{Asset}");
            CardUI  = Asset.LoadAsset<GameObject>("assets/dynamiccardui.prefab").transform.Find("ModCardsCanvas").gameObject;
            Debug.Log($"载入GameObject成功！{CardUI}");

            foreach (string file in Directory.GetFiles(ModCardImgPath, "*.card"))
            {
                DynamicCardCreator.AppendCardToGame(file);
            }
        }
        public byte[] StreamToByteArray(string input)
        {
            var x = Assembly.GetAssembly(typeof(ScriptTrainer)).GetManifestResourceStream(input);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                x.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public void Update()
        {
            
        }
        public void FixedUpdate()
        {
        }
        public void UpdateKeyCode()
        {
            var obj = YourTrainer.GetComponent<ZGGameObject>();
            if (obj.SettingButton != null)
            {
                obj.SettingButton.GetComponentInChildren<TextMeshProUGUI>().text = $"等待按键输入";
                obj.CheckShowButton = true;
            }
        }
        public void SetSettingButton(GameObject obj)
        {
            YourTrainer.GetComponent<ZGGameObject>().SettingButton = obj;
        }
        public void SaveSettings()
        {
            File.WriteAllText(SettingFilePath, ShowTrainer.ToString());
        }
        public void LoadSetting()
        {
            if(File.Exists(SettingFilePath))
            {
                string t = File.ReadAllText(SettingFilePath).Trim();
                Enum.TryParse<KeyCode>(t, out ShowTrainer);
            }
        }
        public void OnDestroy()
        {
            // 移除 MainWindow.testAssetBundle 加载时的资源
            //AssetBundle.UnloadAllAssetBundles(true);

        }
    }
    public class ZGGameObject : MonoBehaviour
    {
        public MainWindow mw;
        public bool LoadEntranceSceneController = false;
        public GameObject SettingButton = null;
        public bool CheckShowButton = false;
        public void Start()
        {
            mw = new MainWindow();
        }
        public void Update()
        {
            if (!MainWindow.initialized)
            {
                MainWindow.Initialize();
            }
            if (Input.GetKeyDown(ScriptTrainer.ShowTrainer))
            {
                if (!MainWindow.initialized)
                {
                    return;
                }
                
                MainWindow.optionToggle = !MainWindow.optionToggle;
                MainWindow.canvas.SetActive(MainWindow.optionToggle);
                Event.current.Use();
            }
            if (SettingButton != null && CheckShowButton)
            {
                foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
                {
                    //Debug.Log($"检测KeyCode : {keycode}");
                    if (Input.GetKeyDown(keycode))
                    {
                        if (keycode != ScriptTrainer.ShowTrainer)
                        {
                            ScriptTrainer.ShowTrainer = keycode;
                            ScriptTrainer.Instance.SaveSettings();
                            SettingButton.GetComponentInChildren<TextMeshProUGUI>().text = $"修改器快捷键：{ScriptTrainer.ShowTrainer}";
                        }
                        CheckShowButton = false;
                        break;
                    }
                }
            }
        }
    }
  
}
