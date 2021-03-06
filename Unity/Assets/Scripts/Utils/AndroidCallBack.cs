﻿using LitJson;
using System;
using ETModel;
using UnityEngine;

public class AndroidCallBack : MonoBehaviour {

    public static String BASE_URL = "http://mapi.javgame.com:14123";
    public static String WECHAT_LOGIN_URL = BASE_URL + "/api/mlogin/WechatLogin";
    public static String WECHAT_PAY_URL = BASE_URL + "/api/mpay/wechatPay";
    public static String ALI_PAY_URL = BASE_URL + "/api/mpay/aplipay";

    public delegate void onPauseCallBack();
    public static onPauseCallBack s_onPauseCallBack = null;

    public delegate void onResumeCallBack();
    public static onResumeCallBack s_onResumeCallBack = null;
    private static AndroidCallBack Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    public void WeChatLogin_IOS(string result)
    {

//        if (OtherData.s_isTest)
//        {
//            WECHAT_LOGIN_URL = "http://mapi.javgame.com:14123/mlogin/WechatLogin";
//        }
//        else
//        {
//            WECHAT_LOGIN_URL = "http://fkmpay.51v.cn/mlogin/WechatLogin";
//        }
//        var wwwForm = new WWWForm();
//        wwwForm.AddField("gameId", "210");
//        wwwForm.AddField("appId", "wxa2c2802e8fedd592");
//        wwwForm.AddField("code", result);
//        wwwForm.AddField("openId", "");
//        UnityWebReqUtil.Instance.Post(WECHAT_LOGIN_URL, wwwForm, (tag,data) =>
//        {
//            LogUtil.Log(data);
//         
//            var jsonData = JsonMapper.ToObject(data);
//            var code = (int)jsonData["code"];
//            var name = (string)jsonData["name"];
//            var expand = (string)jsonData["expand"]["unionid"];
//            if (code != 1)
//            {
//                LogUtil.Log("微信登录web返回失败");
//                return;
//            }
//            JsonData jd = new JsonData();
//            jd["tag"] = Consts.Tag_Third_Login;
//            jd["nickname"] = name;
//            jd["third_id"] = expand;
//            jd["channelname"] = "ios";
//
//            NetLoading.getInstance().Show();
//            LoginServiceSocket.s_instance.sendMessage(jd.ToJson());
//          
//        });
    }

    public void qqLogin_IOS(string result)
    {
//        // 优先使用热更新的代码
//        if (ILRuntimeUtil.getInstance().checkDllClassHasFunc("AndroidCallBack_hotfix", "qqLogin_IOS"))
//        {
//            ILRuntimeUtil.getInstance().getAppDomain().Invoke("HotFix_Project.AndroidCallBack_hotfix", "qqLogin_IOS", null, result);
//            return;
//        }
//
//        LogUtil.Log("收到iosqq登录回调:" + result);
//        var jsonData = JsonMapper.ToObject(result);
//        var accessToken = jsonData["accessToken"].ToString();
//        var openId = jsonData["openId"].ToString();
//        LogUtil.Log(accessToken);
//        LogUtil.Log(openId);
//
//        var url = string.Format("https://graph.qq.com/user/get_user_info?openid={0}&access_token={1}&appid=101436232",
//            openId, accessToken);
//        UnityWebReqUtil.Instance.Get(url, (tag, data) =>
//        {
//            LogUtil.Log(data);
//            var nickname = JsonMapper.ToObject(data)["nickname"].ToString();
//            JsonData jd = new JsonData();
//            jd["tag"] = Consts.Tag_Third_Login;
//            jd["nickname"] = nickname;
//            jd["third_id"] = openId;
//            jd["channelname"] = "ios";
//            LoginServiceSocket.s_instance.sendMessage(jd.ToJson());
//        });
    }

    // apk版本号
    public void SetVersionCode(string apkVersion)
    {
       
    }

    // log开关
    public void SetLogIsShow(string isShow)
    {
       
       
    }

    // 身是否是测试包
    public void SetIsTest(string isTest)
    {
       
    }

    // 回到后台
    public void OnPauseCallBack(string data)
    {
       
    }

    // 回到前台
    public void OnResumeCallBack(string data)
    {
       
    }

    // 分享成功
    public void OnShareSuccess(string data)
    {
        Game.EventSystem.Run(EventIdType.WeChatShare);
    }

    // 登录结果回调
    public void GetLoginResult(string data)
    {
        Log.Debug("Unity收到:" + data);
        try
        {
            JsonData jsonData = JsonMapper.ToObject(data);
            var openId = (string)jsonData["code"];
//            var icon = (string)jsonData["icon"];
            var nickname = (string)jsonData["nickname"];
            var channelname = (string)jsonData["channelname"];
            var response = (string)jsonData["response"];
            Game.EventSystem.Run(EventIdType.WeChatLogin, openId, nickname, response);

        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public void GetPayResult(string data)
    {
//        // 优先使用热更新的代码
//        if (ILRuntimeUtil.getInstance().checkDllClassHasFunc("AndroidCallBack_hotfix", "GetPayResult"))
//        {
//            ILRuntimeUtil.getInstance().getAppDomain().Invoke("HotFix_Project.AndroidCallBack_hotfix", "GetPayResult", null, data);
//            return;
//        }
//
//        LogUtil.Log("Unity收到支付回调:" + data);
    }

    public void Back2Login(string data)
    {
        //        // 优先使用热更新的代码
        //        if (ILRuntimeUtil.getInstance().checkDllClassHasFunc("AndroidCallBack_hotfix", "Back2Login"))
        //        {
        //            ILRuntimeUtil.getInstance().getAppDomain().Invoke("HotFix_Project.AndroidCallBack_hotfix", "Back2Login", null, data);
        //            return;
        //        }
        //
        //        SetScript.Instance.OnClickChangeAccount();
        Game.EventSystem.Run(EventIdType.ChangeAccount);
    }

    public void OnIOSPaySuccess(string data)
    {
        // 优先使用热更新的代码
//        if (ILRuntimeUtil.getInstance().checkDllClassHasFunc("AndroidCallBack_hotfix", "OnIOSPaySuccess"))
//        {
//            ILRuntimeUtil.getInstance().getAppDomain().Invoke("HotFix_Project.AndroidCallBack_hotfix", "OnIOSPaySuccess", null, data);
//            return;
//        }
//
//        LogUtil.Log("Unity收到IOS支付回调:" + data);
//        var strings = data.Split('|');
//        string productId = strings[0];
//        string receiptdata = strings[1];
//
//        JsonData jd = new JsonData();
//        jd["receipt-data"] = receiptdata;
//        string receipt = jd.ToJson();
//
//        var jsonData = new JsonData();
//        jsonData["tag"] = Consts.Tag_IOS_Pay;
//        jsonData["uid"] = UserData.uid;
//        jsonData["data"] = receipt;
//        jsonData["productId"] = productId;
//
//        LogUtil.Log("消息:" + jsonData.ToJson());
//        LogicEnginerScript.Instance.SendMyMessage(jsonData.ToJson());
    }

    public void ShowLoad(string data)
    {
//        // 优先使用热更新的代码
//        if (ILRuntimeUtil.getInstance().checkDllClassHasFunc("AndroidCallBack_hotfix", "ShowLoad"))
//        {
//            ILRuntimeUtil.getInstance().getAppDomain().Invoke("HotFix_Project.AndroidCallBack_hotfix", "ShowLoad", null, data);
//            return;
//        }
//
//        NetLoading.getInstance().Show();
    }

    public void CloseLoad(string data)
    {
//        // 优先使用热更新的代码
//        if (ILRuntimeUtil.getInstance().checkDllClassHasFunc("AndroidCallBack_hotfix", "CloseLoad"))
//        {
//            ILRuntimeUtil.getInstance().getAppDomain().Invoke("HotFix_Project.AndroidCallBack_hotfix", "CloseLoad", null, data);
//            return;
//        }
//
//        NetLoading.getInstance().Close();
    }


}
