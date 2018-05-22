﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETHotfix
{
    [MessageHandler(AppType.Gate)]
    public class C2G_UseHuaFeiHandler : AMRpcHandler<C2G_UseHuaFei, G2C_UseHuaFei>
    {
        protected override async void Run(Session session, C2G_UseHuaFei message, Action<G2C_UseHuaFei> reply)
        {
            G2C_UseHuaFei response = new G2C_UseHuaFei();
            try
            {
                DBProxyComponent proxyComponent = Game.Scene.GetComponent<DBProxyComponent>();
                if (message.HuaFei == 1)
                {
                    List<UseHuaFei> useHuaFeis = await proxyComponent.QueryJson<UseHuaFei>($"{{Uid:{message.Uid},HuaFei:{message.HuaFei}}}");
                    if (useHuaFeis.Count > 0)
                    {
                        // 不合法的金额
                        response.Error = ErrorCode.TodayHasSign;
                        response.Message = "您的1元兑换机会已用完";
                        reply(response);

                        return;
                    }
                    else
                    {
                        // 充值1元话费
                        UseHuaFei useHuaFei = ComponentFactory.CreateWithId<UseHuaFei>(IdGenerater.GenerateId());
                        useHuaFei.Uid = message.Uid;
                        useHuaFei.HuaFei = message.HuaFei;
                        useHuaFei.Phone = message.Phone;
                        await proxyComponent.Save(useHuaFei);

                        reply(response);
                    }
                }
                else if ((message.HuaFei == 5) || (message.HuaFei == 10) || (message.HuaFei == 20))
                {
                    List<UseHuaFei> useHuaFeis = await proxyComponent.QueryJson<UseHuaFei>($"{{CreateTime:/^{DateTime.Now.GetCurrentDay()}/,Uid:{message.Uid},HuaFei:{message.HuaFei}}}");
                    if (useHuaFeis.Count > 0)
                    {
                        // 不合法的金额
                        response.Error = ErrorCode.TodayHasSign;
                        response.Message = "您今天的兑换机会已用完";
                        reply(response);

                        return;
                    }
                    else
                    {
                        // 充值话费
                        UseHuaFei useHuaFei = ComponentFactory.CreateWithId<UseHuaFei>(IdGenerater.GenerateId());
                        useHuaFei.Uid = message.Uid;
                        useHuaFei.HuaFei = message.HuaFei;
                        useHuaFei.Phone = message.Phone;
                        await proxyComponent.Save(useHuaFei);

                        reply(response);
                    }
                }
                else
                {
                    // 不合法的金额
                    response.Error = ErrorCode.TodayHasSign;
                    response.Message = "您的充值金额不存在";
                    reply(response);

                    return;
                }
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}