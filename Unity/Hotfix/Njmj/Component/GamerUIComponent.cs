﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class GamerUIComponentStartSystem : StartSystem<GamerUIComponent>
    {
        public override void Start(GamerUIComponent self)
        {
            self.Start();
        }
    }

    /// <summary>
    /// 玩家UI组件
    /// </summary>
    public class GamerUIComponent : Component
    {
        //UI面板
        public GameObject Panel { get; private set; }

        //玩家昵称
        public string NickName { get { return name.text; } }

        public Image head;
        private Text prompt;
        private Text name;

        private Image readyHead;
        private Text readyName;
        private Text readyText;
        private Text shengLvText;
        private Text jinbiText;
        private Text uidText;
        private GameObject headInfo;
        private GameObject changeMoney;
        private GameObject vip;
        private GameObject zhuang;

        public int Index { get; set; }

        public void Start()
        {
//           
        }

        /// <summary>
        /// 重置面板
        /// </summary>
        public void ResetPanel()
        {
//            ResetPrompt();
        
            this.name.text = "空位";

            this.Panel = null;
            this.name = null;
        }

        /// <summary>
        /// 设置面板
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="gameObject"></param>
        /// <param name="index"></param>
        public void SetPanel(GameObject panel, GameObject readyPanel, int index)
        {
            panel.SetActive(true);
            this.Panel = panel;
            this.Index = index;
            //绑定关联
            this.head = this.Panel.Get<GameObject>("Head").GetComponent<Image>();
            this.name = this.Panel.Get<GameObject>("Name").GetComponent<Text>();
            this.prompt = this.Panel.Get<GameObject>("Prompt").GetComponent<Text>();
            this.changeMoney = this.Panel.Get<GameObject>("ChangeMoney");
            this.zhuang = this.Panel.Get<GameObject>("Zhuang");


            if (index != 0)
            {
                this.headInfo = this.head.transform.GetChild(0).gameObject;
                this.shengLvText = this.headInfo.Get<GameObject>("Shenglv").GetComponent<Text>();
                this.jinbiText = this.headInfo.Get<GameObject>("Jinbi").GetComponent<Text>();
                this.uidText = this.headInfo.Get<GameObject>("Uid").GetComponent<Text>();
                this.headInfo.SetActive(false);
                head.GetComponent<Button>().onClick.RemoveAllListeners();
                head.GetComponent<Button>().onClick.Add(OnShowHeadInfo);
            }

//            this.readyHead = readyPanel.Get<GameObject>("Image").GetComponent<Image>();
//            this.readyName = readyPanel.Get<GameObject>("Name").GetComponent<Text>();
//            this.readyText = readyPanel.Get<GameObject>("Text").GetComponent<Text>();

            UpdatePanel();
        }

        public void SetGoldChange(int num)
        {
            GameHelp.ShowPlusGoldChange(changeMoney, num);
        }

        private void OnShowHeadInfo()
        {
            if (this.headInfo.activeSelf)
            {
                headInfo.SetActive(false);
            }
            else
            {
                headInfo.SetActive(true);
            }
        }

        public void ResetReadyPanel()
        {
            if (readyName == null)
            {
                return;
            }
            readyName.text = "";
            readyHead.sprite = CommonUtil.getSpriteByBundle("Image_Desk_Card", "icon_default");
            readyText.text = "";
            vip.transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 更新面板
        /// </summary>
        public void UpdatePanel()
        {
            if (this.Panel != null)
            {
                SetUserInfo();
            }
        }

        /// <summary>
        /// 游戏开始
        /// </summary>
        public void GameStart()
        {

//            ResetPrompt();
        }

        /// <summary>
        /// 设置用户信息
        /// </summary>
        /// <param name="id"></param>
        private async void SetUserInfo()
        {
            PlayerInfo playerInfo = this.GetParent<Gamer>().PlayerInfo;

            if (this.Panel != null || playerInfo == null)
            {
                name.text = playerInfo.Name;
                head.sprite = Game.Scene.GetComponent<UIIconComponent>().GetSprite(playerInfo.Icon);

                if (Index != 0)
                {
                    uidText.text = playerInfo.Name;
                    jinbiText.text = $"金 币:<color=#FFF089FF>{playerInfo.GoldNum}</color>";

                    float i;
                    if (playerInfo.TotalGameCount == 0)
                    {
                        i = 0;
                    }
                    else
                    {
                        i = GameUtil.GetWinRate(playerInfo.TotalGameCount, playerInfo.WinGameCount);
                    }
                    shengLvText.text = $"胜 率:<color=#FFF089FF>{i}%</color>";

                    if (GameUtil.isVIP(playerInfo))
                    {
                        head.transform.Find("vip").transform.localScale = Vector3.one;
                    }
                    else
                    {
                        head.transform.Find("vip").transform.localScale = Vector3.zero;
                    }

                }

            }
        }

        public void SetZhuang()
        {
            Gamer gamer = this.GetParent<Gamer>();
            if (gamer.IsBanker)
            {
                zhuang.SetActive(true);
            }
            else
            {
                zhuang.SetActive(false);
            }
        }

        private CancellationTokenSource tokenSource;

        public async Task GetPlayerInfo()
        {
            tokenSource = new CancellationTokenSource();
            try
            {
                Gamer gamer = this.GetParent<Gamer>();
                Log.Debug("请求gamer信息:" + gamer.UserID);
                G2C_PlayerInfo playerInfo = (G2C_PlayerInfo)await SessionWrapComponent.Instance.Session.Call(new C2G_PlayerInfo() { uid = gamer.UserID }, tokenSource.Token);
                gamer.PlayerInfo = playerInfo.PlayerInfo;
            }
            catch (Exception e)
            {
                Log.Error(e);
                tokenSource.Cancel();
            }
        }

        /// <summary>
        /// 设置准备界面
        /// </summary>
        /// <param name="gameObject"></param>
        public void SetHeadPanel(GameObject gameObject)
        {
            try
            {
                if (gameObject == null)
                {
                    return;
                }
                Gamer gamer = this.GetParent<Gamer>();
                this.readyHead = gameObject.Get<GameObject>("Image").GetComponent<Image>();
                this.readyName = gameObject.Get<GameObject>("Name").GetComponent<Text>();
                this.readyText = gameObject.Get<GameObject>("Text").GetComponent<Text>();
                this.vip = gameObject.Get<GameObject>("vip");
                PlayerInfo playerInfo = gamer.PlayerInfo;

                if (readyName == null) return;
                readyName.text = playerInfo.Name + "";
                HeadManager.setHeadSprite(readyHead, playerInfo.Icon);

                if (gamer.IsReady)
                {
                    gameObject.transform.Find("Text").GetComponent<Text>().text = "已准备";
                }
                else
                {
                    readyText.text = "";
                }

                if (GameUtil.isVIP(playerInfo))
                {
                    vip.transform.localScale = Vector3.one;
                }
                else
                {
                    vip.transform.localScale = Vector3.zero;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

        }

        public void SetReady()
        {
            readyText.text = "已准备";
        }


    }
}
