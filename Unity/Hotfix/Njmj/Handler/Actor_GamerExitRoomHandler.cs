﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    [MessageHandler]
    public class Actor_GamerExitRoomHandler : AMHandler<Actor_GamerExitRoom>
    {
        protected override async void Run(Session session, Actor_GamerExitRoom message)
        {
            try
            {
                Log.Info($"收到退出:{JsonHelper.ToJson(message)}");
                UI uiRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.UIRoom);

                if (uiRoom == null) return;
                UI uiReady = Game.Scene.GetComponent<UIComponent>().Get(UIType.UIReady);

                GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
                UIRoomComponent uiRoomComponent = uiRoom.GetComponent<UIRoomComponent>();

                UIReadyComponent uiReadyComponent = uiReady.GetComponent<UIReadyComponent>();

                if (gamerComponent.LocalGamer.UserID == message.Uid)
                {
                    CommonUtil.ShowUI(UIType.UIMain);
                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.UIRoom);
                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.UIReady);
                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.UIChatShow);
                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.UIChat);
                }
                else
                {
                    Gamer gamer = gamerComponent.Get(message.Uid);
                    gamer?.GetComponent<GamerUIComponent>()?.ResetReadyPanel();
                    uiRoomComponent.RemoveGamer(message.Uid);
                }

                SoundsHelp.Instance.playSound_LiKai();

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
