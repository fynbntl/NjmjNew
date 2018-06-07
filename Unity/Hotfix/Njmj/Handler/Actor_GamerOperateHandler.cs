﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
    [MessageHandler]
    public class Actor_GamerOperateHandler : AMHandler<Actor_GamerOperation>
    {
        protected override async void Run(Session session, Actor_GamerOperation message)
        {
            GamerOperation(message);
        }

        public static void GamerOperation(Actor_GamerOperation message)
        {
            try
            {
                Log.Info($"收到有人碰杠胡:{JsonHelper.ToJson(message)}");
                UI uiRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.UIRoom);
                GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
                UIRoomComponent uiRoomComponent = uiRoom.GetComponent<UIRoomComponent>();
                Gamer gamer = gamerComponent.Get(message.Uid);
                HandCardsComponent handCardsComponent = gamer.GetComponent<HandCardsComponent>();
                uiRoomComponent.ClosePropmtBtn();
                uiRoomComponent.ShowTurn(message.Uid);
                MahjongInfo mahjongInfo = new MahjongInfo() { weight = (byte) message.weight, m_weight = (Consts.MahjongWeight) message.weight };

                if (PlayerInfoComponent.Instance.uid == message.Uid)
                {
                    if (message.OperationType == 0)
                    {
                        gamerComponent.CurrentPlayUid = message.Uid;
                        gamerComponent.IsPlayed = false;
                    }

                    //碰刚
                    if (message.OperationType == 5)
                    {
                        handCardsComponent.SetPengGang(message.OperationType, mahjongInfo);
                        SoundsHelp.Instance.PlayGang(PlayerInfoComponent.Instance.GetPlayerInfo().PlayerSound);
                    }
                    else
                    {
                        handCardsComponent.SetPeng(message.OperationType, mahjongInfo);
                        SoundsHelp.Instance.PlayPeng(PlayerInfoComponent.Instance.GetPlayerInfo().PlayerSound);
                    }
                }
                else
                {
                    //碰刚
                    if (message.OperationType == 5)
                    {
                        handCardsComponent.SetOtherPengGang(message.OperationType, mahjongInfo);
                        SoundsHelp.Instance.PlayGang(gamer.PlayerInfo.PlayerSound);
                    }
                    else
                    {
                        handCardsComponent.SetOtherPeng(message.OperationType, mahjongInfo);
                        SoundsHelp.Instance.PlayPeng(gamer.PlayerInfo.PlayerSound);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
