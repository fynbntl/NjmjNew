﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
    public class DBCommonUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="taskId"></param>
        /// <param name="isGet"></param>
        public static async Task<TaskInfo> UpdateTask(long uid, int taskId, bool isGet = false)
        {
            DBProxyComponent proxyComponent = Game.Scene.GetComponent<DBProxyComponent>();
            TaskInfo taskInfo = new TaskInfo();
            TaskProgressInfo progress = new TaskProgressInfo();
            List<TaskProgressInfo> taskProgressInfoList = await proxyComponent.QueryJson<TaskProgressInfo>($"{{UId:{uid},TaskId:{taskId}}}");

            if (taskProgressInfoList.Count > 0)
            {
                for (int i = 0; i < taskProgressInfoList.Count; ++i)
                {
                    progress = taskProgressInfoList[i];

                    if (isGet)
                    {
                        progress.IsGet = true;
                    }
                    else
                    {
                        ++progress.CurProgress;
                        if (progress.CurProgress == progress.Target)
                        {
                            progress.IsComplete = true;
                        }
                        else
                        {
                            progress.IsComplete = false;
                        }
                    }
                    await proxyComponent.Save(progress);
                }
                taskInfo.Id = progress.TaskId;
                taskInfo.IsGet = progress.IsGet;
                taskInfo.IsComplete = progress.IsComplete;
                taskInfo.Progress = progress.CurProgress;
            }

            return taskInfo;
        }

        public static async Task<TaskInfo> UpdateChengjiu(long UId, int taskId, bool isGet = false)
        {
            DBProxyComponent proxyComponent = Game.Scene.GetComponent<DBProxyComponent>();
            TaskInfo taskInfo = new TaskInfo();
            ChengjiuInfo progress = new ChengjiuInfo();
            List<ChengjiuInfo> chengjiuInfoList = await proxyComponent.QueryJson<ChengjiuInfo>($"{{UId:{UId},TaskId:{taskId}}}");

            if (chengjiuInfoList.Count > 0)
            {
                for (int i = 0; i < chengjiuInfoList.Count; ++i)
                {
                    progress = chengjiuInfoList[i];
                    progress.TaskId = taskId;
                    if (isGet)
                    {
                        progress.IsGet = true;
                    }
                    else
                    {
                        ++progress.CurProgress;
                        if (progress.CurProgress == progress.Target)
                        {
                            progress.IsComplete = true;
                        }
                        else
                        {
                            progress.IsComplete = false;
                        }
                    }
                    await proxyComponent.Save(progress);
                }
                taskInfo.Id = progress.TaskId;
                taskInfo.IsGet = progress.IsGet;
                taskInfo.IsComplete = progress.IsComplete;
                taskInfo.Progress = progress.CurProgress;
            }
            return taskInfo;
        }

        public static async void ChangeWealth(long uid, int propId, float propNum)
        {
            DBProxyComponent proxyComponent = Game.Scene.GetComponent<DBProxyComponent>();
            switch (propId)
            {
                // 金币
                case 1:
                    {
                        List<PlayerBaseInfo> playerBaseInfos = await proxyComponent.QueryJson<PlayerBaseInfo>($"{{UId:{uid}}}");
                        playerBaseInfos[0].GoldNum += (int)propNum;
                        if (playerBaseInfos[0].GoldNum < 0)
                        {
                            playerBaseInfos[0].GoldNum = 0;
                        }
                        await proxyComponent.Save(playerBaseInfos[0]);
                    }
                    break;

                // 元宝
                case 2:
                    {
                        List<PlayerBaseInfo> playerBaseInfos = await proxyComponent.QueryJson<PlayerBaseInfo>($"{{UId:{uid}}}");
                        playerBaseInfos[0].WingNum += (int)propNum;
                        if (playerBaseInfos[0].WingNum < 0)
                        {
                            playerBaseInfos[0].WingNum = 0;
                        }
                        await proxyComponent.Save(playerBaseInfos[0]);
                    }
                    break;

                // 话费
                case 3:
                    {
                        List<PlayerBaseInfo> playerBaseInfos = await proxyComponent.QueryJson<PlayerBaseInfo>($"{{UId:{uid}}}");
                        playerBaseInfos[0].HuaFeiNum += propNum;
                        if (playerBaseInfos[0].HuaFeiNum < 0)
                        {
                            playerBaseInfos[0].HuaFeiNum = 0;
                        }
                        await proxyComponent.Save(playerBaseInfos[0]);
                    }
                    break;

                // 其他道具
                default:
                    {
                        List<UserBag> userBags = await proxyComponent.QueryJson<UserBag>($"{{UId:{uid},BagId:{propId}}}");
                        if (userBags.Count == 0)
                        {
                            UserBag itemInfo = new UserBag();
                            itemInfo.BagId = propId;
                            itemInfo.UId = uid;
                            itemInfo.Count = (int)propNum;
                            DBHelper.AddItemToDB(itemInfo);
                        }
                        else
                        {
                            userBags[0].Count += (int)propNum;
                            if (userBags[0].Count < 0)
                            {
                                userBags[0].Count = 0;
                            }
                            await proxyComponent.Save(userBags[0]);
                        }
                    }
                    break;
            }
        }
    }
}
