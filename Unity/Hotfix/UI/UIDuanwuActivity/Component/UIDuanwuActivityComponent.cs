﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIDuanwuActivitySystem : StartSystem<UIDuanwuActivityComponent>
    {
        public override void Start(UIDuanwuActivityComponent self)
        {
            self.Start();
        }
    }

    public class UIDuanwuActivityComponent : Component
    {
        public static UIDuanwuActivityComponent Instance;
        private Text Des;
        private Text Reward;
        private Text Progress;
        private Text ActivityFinishTime;
        private Text OwnZongziCount;
        private GameObject TaskGrid;
        private Text ShowZongzi;

        private Button RefreshTaskBtn;
        private Button TreasureBtn;
        private Text RefreshLeftCount;//
        private GameObject Tip;
        private Text TipTxt;
        private Button CancelBtn;
        private Button SureBtn;


        #region duanwu
        private GameObject taskItem = null;
        private List<GameObject> taskItemList = new List<GameObject>();
        private List<UI> uilist = new List<UI>();
        private List<DuanwuActivity> duanwuActInfoList = new List<DuanwuActivity>();
        private string activityType = "";
        public DuanwuData duanwuData;
        private CancellationTokenSource tokenSource;
        #endregion

        public void Start()
        {
            Instance = this;
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            Des = rc.Get<GameObject>("Des").GetComponent<Text>();
            Reward = rc.Get<GameObject>("Reward").GetComponent<Text>();
            Progress = rc.Get<GameObject>("Progress").GetComponent<Text>();
            ShowZongzi = rc.Get<GameObject>("ShowZongzi").GetComponent<Text>();
            ActivityFinishTime = rc.Get<GameObject>("ActivityFinishTime").GetComponent<Text>();
            OwnZongziCount = rc.Get<GameObject>("OwnZongziCount").GetComponent<Text>();
            RefreshTaskBtn = rc.Get<GameObject>("RefreshTaskBtn").GetComponent<Button>();
            TreasureBtn = rc.Get<GameObject>("TreasureBtn").GetComponent<Button>();
            TaskGrid = rc.Get<GameObject>("TaskGrid");
            RefreshLeftCount = rc.Get<GameObject>("RefreshLeftCount").GetComponent<Text>();
            taskItem = CommonUtil.getGameObjByBundle(UIType.UIDuanwuItem);

            Tip = rc.Get<GameObject>("Tip");
            TipTxt = rc.Get<GameObject>("TipTxt").GetComponent<Text>();
            SureBtn = rc.Get<GameObject>("SureBtn").GetComponent<Button>();
            CancelBtn = rc.Get<GameObject>("CancelBtn").GetComponent<Button>();

            //刷新任务按钮
            RefreshTaskBtn.onClick.Add(() =>
            {
                try
                {
                    string curTime = CommonUtil.getCurTimeNormalFormat();
                    if (string.CompareOrdinal(curTime, duanwuData.StartTime) >= 0
                        && string.CompareOrdinal(curTime, duanwuData.EndTime) < 0)
                    {
                        if (duanwuData != null && duanwuData.RefreshCount > 0)
                        {
                            Tip.SetActive(true);
                            TipTxt.text = $"点击刷新后,任务进度全部清零\r\n本次需要花费{RefreshCost()}金币!";
                        }
                        else
                        {
                            ToastScript.createToast("今日刷新次数已用完");
                        }
                    }
                    else
                    {
                        ToastScript.createToast("未到活动时间");
                    }
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }
            });

            TreasureBtn.onClick.Add(() =>
            {
                //显示宝箱界面
                Game.Scene.GetComponent<UIComponent>().Create(UIType.UIDuanwuTreasure);
            });

            SureBtn.onClick.Add(() =>
            {
                RefreshActivityType();
            });

            CancelBtn.onClick.Add(() =>
            {
                Tip.SetActive(false);
            });

            GetActivityTypeRequest();
        }

        /// <summary>
        /// 向服务器请求活动类型（）
        /// </summary>
        public async void GetActivityTypeRequest()
        {
            UINetLoadingComponent.showNetLoading();
            //type = 1 则为单纯请求数据
            G2C_DuanwuDataBase g2cDuanwu = (G2C_DuanwuDataBase)await Game.Scene.GetComponent<SessionComponent>().Session.Call(new C2G_DuanwuDataBase { UId = PlayerInfoComponent.Instance.uid, Type = 1 });
            UINetLoadingComponent.closeNetLoading();
            try
            {
                duanwuData = g2cDuanwu.DuanwuData;
                activityType = g2cDuanwu.DuanwuData.ActivityType;
                GetDuanwuTaskInfoList();
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        int notGetcount = 0;
        private int GetNoGetCount()
        {
            for (int i = 0; i < duanwuActInfoList.Count; ++i)
            {
                if (duanwuActInfoList[i].IsComplete && !duanwuActInfoList[i].IsGet)
                {
                    notGetcount++;
                }
            }
            return notGetcount;
        }

        public void DeCount()
        {
            --notGetcount;
            if (notGetcount <= 0)
            {
                Game.Scene.GetComponent<UIComponent>().Get(UIType.UIMain).GetComponent<UIMainComponent>().SetRedTip(2, false, notGetcount);
            }
        }

        private long RefreshCost()
        {
            int goldCost = 0;
            switch (duanwuData.RefreshCount)
            {
                case 0:
                    ToastScript.createToast("今日刷新次数已用完");
                    break;
                case 1:
                    goldCost = 50000;
                    break;
                case 2:
                    goldCost = 30000;
                    break;
                case 3:
                    goldCost = 10000;
                    break;
                default:
                    Log.Debug("刷新次数超出,检查代码逻辑");
                    break;
            }
            return goldCost;
        }

        /// <summary>
        /// 刷新活动类型
        /// </summary>
        private async void RefreshActivityType()
        {
            try
            {
                long goldCost = RefreshCost();

                if (PlayerInfoComponent.Instance.GetPlayerInfo().GoldNum >= goldCost)
                {
                    activityType = GetRandomIndex();
                    //type = 3 刷新活动
                    G2C_RefreshDuanwuActivity g2cDuanwu = (G2C_RefreshDuanwuActivity)await Game.Scene.GetComponent<SessionComponent>().Session.Call(new C2G_RefreshDuanwuActivity { UId = PlayerInfoComponent.Instance.uid, ActivityType = activityType, GoldCost = goldCost });

                    if(g2cDuanwu.Error != ErrorCode.ERR_Success)
                    {
                        ToastScript.createToast(g2cDuanwu.Message);
                        return;
                    }

                    duanwuData = g2cDuanwu.DuanwuData;
                    duanwuActInfoList = g2cDuanwu.DuanwuActivityList;
                    Init();
                    CreateDuanwuList();
                    PlayerInfoComponent.Instance.GetPlayerInfo().GoldNum = g2cDuanwu.GoldNum;
                    Game.Scene.GetComponent<UIComponent>().Get(UIType.UIMain).GetComponent<UIMainComponent>().refreshUI();
                }
                else
                {
                    ToastScript.createToast("金币不足");
                }
                Tip.SetActive(false);
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        /// <param name="zongzi"></param>
        public void RefreshUI(int zongzi)
        {
            duanwuData.ZongziCount = zongzi;
            OwnZongziCount.text = duanwuData.ZongziCount.ToString();
        }

        /// <summary>
        /// 向服务器请求端午任务列表
        /// </summary>
        private async void GetDuanwuTaskInfoList()
        {
            UINetLoadingComponent.showNetLoading();
            G2C_DuanwuActivity g2cduanwu = (G2C_DuanwuActivity)await SessionComponent.Instance.Session.Call(new C2G_DuanwuActivity() { UId = PlayerInfoComponent.Instance.uid });
            UINetLoadingComponent.closeNetLoading();

            if (g2cduanwu.Error != ErrorCode.ERR_Success)
            {
                ToastScript.createToast("活动还未开始");
                return;
            }

            duanwuActInfoList = g2cduanwu.DuanwuActivityList;

            GetNoGetCount();
            Init();
            CreateDuanwuList();
        }

        /// <summary>
        /// 界面初始化数据
        /// </summary>
        private void Init()
        {
            try
            {
                Log.Debug(duanwuData + "======");
                //活动结束时间
                ActivityFinishTime.text = CommonUtil.splitStr_Start_str(duanwuData.StartTime, ' ') + "~" +
                    CommonUtil.splitStr_Start_str(duanwuData.EndTime, ' ');
                OwnZongziCount.text = duanwuData.ZongziCount.ToString();
                RefreshLeftCount.text = duanwuData.RefreshCount.ToString();
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// 创建端午活动任务列表
        /// </summary>
        private void CreateDuanwuList()
        {
            GameObject obj = null;
            try
            {
                for (int i = 0; i < GetActivityType().Count; ++i)
                {
                    int index = GetActivityType()[i];
                    DuanwuActivity activity = duanwuActInfoList[index];
                    if (i < taskItemList.Count)
                    {
                        obj = taskItemList[i];
                    }
                    else
                    {
                        obj = GameObject.Instantiate(taskItem);
                        obj.transform.SetParent(TaskGrid.transform);
                        obj.transform.localScale = Vector3.one;
                        obj.transform.localPosition = Vector3.zero;
                        UI ui = ComponentFactory.Create<UI, GameObject>(obj);
                        ui.AddComponent<UIDuanwuActivityItemComponent>();
                        uilist.Add(ui);
                        taskItemList.Add(obj);
                    }
                    if(i == 0)
                    {
                        SetCurActivityInfo(activity);
                        uilist[i].GetComponent<UIDuanwuActivityItemComponent>().SetSelectState(true);
                    }
                    uilist[i].GetComponent<UIDuanwuActivityItemComponent>().SetItemInfo(activity);
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// 获得
        /// </summary>
        /// <returns></returns>
        private List<int> GetActivityType()
        {
            List<int> activityList = new List<int>();
            string[] strArray = activityType.Split(';');
            for (int i = 0; i < strArray.Length; ++i)
            {
                int index = int.Parse(strArray[i]);
                activityList.Add(index);
            }
            return activityList;
        }

        /// <summary>
        /// 显示领取粽子的动画
        /// </summary>
        /// <param name="count"></param>
        public void ShowAddZongziCount(int count)
        {
            //显示领取了多少粽子，暂不处理
            ShowZongzi.text = "+" + count;
            SetTimeOut(2);
        }

        /// <summary>
        /// 设置定时器
        /// </summary>
        /// <param name="time"></param>
        private async void SetTimeOut(int time)
        {
            try
            {
                tokenSource = new CancellationTokenSource();
                while (time > 0)
                {
                    await ETModel.Game.Scene.GetComponent<TimerComponent>().WaitAsync(500, tokenSource.Token);
                    time--;
                }
                ShowZongzi.text = "";
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// 设置当前点击的活动具体信息
        /// </summary>
        /// <param name="activity"></param>
        public void SetCurActivityInfo(DuanwuActivity activity)
        {
            try
            {
                for (int i = 0; i < uilist.Count; ++i)
                {
                    if (uilist[i].GetComponent<UIDuanwuActivityItemComponent>().info != activity)
                    {
                        uilist[i].GetComponent<UIDuanwuActivityItemComponent>().SetSelectState(false);
                    }
                }
                Des.text = activity.Desc;
                Reward.text = activity.Reward.ToString();
                if (activity.IsComplete)
                {
                    Progress.text = activity.Target + "/" + activity.Target;
                }
                else
                {
                    Progress.text = activity.CurProgress + "/" + activity.Target;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// 从12个活动里面随机获取六个活动
        /// </summary>
        /// <returns></returns>
        private string GetRandomIndex()
        {
            List<int> randomIndexList = new List<int>();
            int i = 6;
            StringBuilder result = new StringBuilder();
            while (i > 0)
            {
                int index = Common_Random.getRandom(0, 11);
                if (!randomIndexList.Contains(index))
                {
                    randomIndexList.Add(index);
                    if(randomIndexList.Count == 1)
                    {
                        result.Append(index);
                    }
                    else
                    {
                        result.Append(";").Append(index);
                    }
                    --i;
                }
            }
            return result.ToString();
        }

        //清理内存
        public override void Dispose()
        {
            base.Dispose();
            taskItemList.Clear();
            uilist.Clear();
            duanwuActInfoList.Clear();
            tokenSource.Cancel();
            notGetcount = 0;
        }
    }
}
