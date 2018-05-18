﻿using ETModel;
using System;
using System.Collections.Generic;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_BagHandler : AMRpcHandler<C2G_BagOperation, G2C_BagOperation>
    {
        protected override async void Run(Session session, C2G_BagOperation message, Action<G2C_BagOperation> reply)
        {
            G2C_BagOperation response = new G2C_BagOperation();
            try
            {
                DBProxyComponent proxyComponent = Game.Scene.GetComponent<DBProxyComponent>();
                List<UserBag> bagInfoList = await proxyComponent.QueryJson<UserBag>($"{{UId:{message.UId}}}");
                response.ItemList = new List<Bag>();
                List<Bag> itemList = new List<Bag>();
                for(int i = 0;i< bagInfoList.Count; ++i)
                {
                    Bag item = new Bag();
                    item.ItemId = bagInfoList[i].BagId;
                    item.Count = bagInfoList[i].Count;
                    itemList.Add(item);
                }
                response.ItemList = itemList;
                reply(response);
            }
            catch(Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
