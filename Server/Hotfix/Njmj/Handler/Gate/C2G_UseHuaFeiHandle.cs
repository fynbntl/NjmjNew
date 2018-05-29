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
                if (message.HuaFei == 5)
                {
                    List<PlayerBaseInfo> playerBaseInfos = await proxyComponent.QueryJson<PlayerBaseInfo>($"{{Uid:{message.Uid}}}");
                    if (playerBaseInfos[0].HuaFeiNum >= 5)
                    {
                        List<UseHuaFei> useHuaFeis = await proxyComponent.QueryJson<UseHuaFei>($"{{CreateTime:/^{DateTime.Now.GetCurrentDay()}/,Uid:{message.Uid},HuaFei:{message.HuaFei}}}");
                        if (useHuaFeis.Count > 0)
                        {
                            response.Error = ErrorCode.TodayHasSign;
                            response.Message = "您今天的兑换机会已用完";
                            reply(response);

                            return;
                        }
                        else
                        {
                            // 充值话费
                            {
                                string str = HttpUtil.PhoneFeeRecharge(message.Uid.ToString(), "话费", "1", message.Phone, "3", "1");

                                SortedDictionary<string, string> dic = CommonUtil.XmlToDictionary(str);
                                string Code;
                                dic.TryGetValue("Code", out Code);

                                string Message;
                                dic.TryGetValue("Message", out Message);
                                
                                if (Code.CompareTo("0") != 0)
                                {
                                    Log.Debug("充值失败：" + Message);

                                    response.Message = "充值失败";
                                    response.Error = ErrorCode.ERR_PhoneCodeError;
                                    reply(response);
                                    return;
                                }
                                // 充值成功
                                else
                                {
                                    // 充值话费
                                    UseHuaFei useHuaFei = ComponentFactory.CreateWithId<UseHuaFei>(IdGenerater.GenerateId());
                                    useHuaFei.Uid = message.Uid;
                                    useHuaFei.HuaFei = message.HuaFei;
                                    useHuaFei.Phone = message.Phone;
                                    await proxyComponent.Save(useHuaFei);

                                    playerBaseInfos[0].HuaFeiNum -= 5;
                                    await proxyComponent.Save(playerBaseInfos[0]);
                                }
                            }

                            reply(response);
                        }
                    }
                    else
                    {
                        response.Error = ErrorCode.TodayHasSign;
                        response.Message = "您的话费余额不足";
                        reply(response);

                        return;
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
