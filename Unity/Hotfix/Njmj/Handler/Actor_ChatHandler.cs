﻿using ETModel;
using System;
using UnityEngine;

namespace ETHotfix
{
    [MessageHandler]
    public class Actor_ChatHandler : AMHandler<Actor_Chat>
    {
        protected override void Run(Session session, Actor_Chat message)
        {
            Debug.Log("===");
            try
            {
                if (message.ChatType == 1)
                {
                    GameObject item = CommonUtil.getGameObjByBundle(message.Value);
                    GameObject obj = GameObject.Instantiate(item);
                    obj.transform.SetParent(GameObject.Find("CommonWorld").transform);
                }
                else if(message.ChatType == 2)
                {
                    Debug.Log("33333333333");
                    Game.Scene.GetComponent<UIComponent>().Get(UIType.UIReady).GetComponent<UIReadyComponent>().ShowChatContent(message.Value,message.UId);
                }
                Debug.Log(Game.Scene.GetComponent<UIComponent>().Get(UIType.UIChat).GetComponent<UIChatComponent>());
                Game.Scene.GetComponent<UIComponent>().Get(UIType.UIChat).GetComponent<UIChatComponent>().CloseOrOpenChatUI(false);
            }
            catch (Exception e)
            {

            }
            
        }
    }
}
