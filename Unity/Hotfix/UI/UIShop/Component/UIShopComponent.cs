﻿using ETModel;
using ProtoBuf;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ProtoContract]
    public enum ShopType
    {
        Wing = 1,
        Gold = 2,
        Prop = 5,
        Vip = 6
    }

    [ObjectSystem]
    public class UIShopSystem : AwakeSystem<UIShopComponent>
    {
        public override void Awake(UIShopComponent self)
        {
            self.Awake();
        }
    }

    public class UIShopComponent : Component
    {
        #region UI
        private Button wingBtn;
        private Button goldBtn;
        private Button proBtn;
        private Button vipBtn;
        private GameObject Wing;
        private GameObject Gold;
        private GameObject Prop;
        private GameObject Vip;
        private Button returnBtn;
        private GameObject wingGrid;
        private GameObject goldGrid;
        private GameObject propGrid;
        private GameObject vipGrid;
        private GameObject buyTip;
        private GameObject BuyTipS;
        private Button sureBtn;
        private Button cancelBtn;
        private Text buyTxt;
        private GameObject PaySelectTip;
        private Button WeipayBtn;
        private Button AlipayBtn;
        private Button maskClick;
        #endregion

        #region field
        //通过不同的商店类型保存相应的点击按钮 1:商店类型 2:点击按钮
        private Dictionary<ShopType, GameObject> buttonDic = new Dictionary<ShopType, GameObject>();
        //商品信息列表
        private List<ShopInfo> shopInfoList = new List<ShopInfo>();
        //全部的商品信息字典 int:商店类型
        private Dictionary<int, List<ShopInfo>> shopInfoDic = new Dictionary<int, List<ShopInfo>>();
        //物品obj缓存列表
        private List<GameObject> itemList = new List<GameObject>();
        //全部商品obj缓存列表 
        private Dictionary<ShopType, List<GameObject>> itemDic = new Dictionary<ShopType, List<GameObject>>();
        //ui缓存列表（之后可能会优化，只是刷新设置数据）
        private List<UI> uiList = new List<UI>();
        //全部ui缓存字典
        private Dictionary<ShopType, List<UI>> uiDic = new Dictionary<ShopType, List<UI>>();
        private bool isCanBuy = false;
        private ShopInfo shopInfo;
        #endregion

        public void Awake()
        {
            #region get
            ReferenceCollector rc = GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            PlayerInfoComponent playerInfoCom = Game.Scene.GetComponent<PlayerInfoComponent>();
            wingBtn = rc.Get<GameObject>("WingToggle").GetComponent<Button>();
            goldBtn = rc.Get<GameObject>("GoldToggle").GetComponent<Button>();
            proBtn = rc.Get<GameObject>("ProToggle").GetComponent<Button>();
            vipBtn = rc.Get<GameObject>("VipToggle").GetComponent<Button>();

            Wing = rc.Get<GameObject>("Wing");
            Gold = rc.Get<GameObject>("Gold");
            Prop = rc.Get<GameObject>("Prop");
            Vip = rc.Get<GameObject>("Vip");

            returnBtn = rc.Get<GameObject>("ReturnBtn").GetComponent<Button>();
            wingGrid = rc.Get<GameObject>("WingGrid");
            goldGrid = rc.Get<GameObject>("GoldGrid");
            propGrid = rc.Get<GameObject>("PropGrid");
            vipGrid = rc.Get<GameObject>("VipGrid");
            buyTip = rc.Get<GameObject>("BuyTip");
            sureBtn = rc.Get<GameObject>("SureBtn").GetComponent<Button>();
            cancelBtn = rc.Get<GameObject>("CancelBtn").GetComponent<Button>();
            maskClick = rc.Get<GameObject>("maskClick").GetComponent<Button>();
            buyTxt = rc.Get<GameObject>("BuyTxt").GetComponent<Text>();
            BuyTipS = rc.Get<GameObject>("BuyTipS");

            WeipayBtn = rc.Get<GameObject>("WeipayBtn").GetComponent<Button>();
            AlipayBtn = rc.Get<GameObject>("AlipayBtn").GetComponent<Button>();
            PaySelectTip = rc.Get<GameObject>("PaySelectTip");

            shopInfoList = playerInfoCom.GetShopInfoList();
            #endregion

            CommonUtil.SetTextFont(this.GetParent<UI>().GameObject);

            #region AddButton
            buttonDic.Add(ShopType.Wing, Wing);
            buttonDic.Add(ShopType.Gold, Gold);
            buttonDic.Add(ShopType.Vip, Vip);
            buttonDic.Add(ShopType.Prop, Prop);
            #endregion

            AddShopInfoByType();

            #region buttonClickEvt
            //元宝商城
            wingBtn.onClick.Add(() =>
            {
                ButtonClick(ShopType.Wing, UIType.UIWingItem, wingGrid.transform);
            });

            //金币商城
            goldBtn.onClick.Add(() =>
            {
                ButtonClick(ShopType.Gold, UIType.UIGoldItem, goldGrid.transform);
            });

            //道具商城
            proBtn.onClick.Add(() =>
            {
                ButtonClick(ShopType.Prop, UIType.UIPropItem, propGrid.transform);
            });

            //VIP商城
            vipBtn.onClick.Add(() =>
            {
                ButtonClick(ShopType.Vip, UIType.UIVipItem, vipGrid.transform);
            });

            //返回
            returnBtn.onClick.Add(() =>
            {
                Game.Scene.GetComponent<UIComponent>().Remove(UIType.UIShop);
            });

            //确定购买
            sureBtn.onClick.Add(() =>
            {
                if (isCanBuy)
                {
                    if (shopInfo == null)
                        return;
                    BuyItem();
                }
                else
                {
                    buyTip.SetActive(false);
                    ButtonClick(ShopType.Wing, UIType.UIWingItem, wingGrid.transform);
                }
            });

            //取消购买
            cancelBtn.onClick.Add(() =>
            {
                buyTip.SetActive(false);
            });

            //支付宝支付
            AlipayBtn.onClick.Add(() =>
            {
                Recharge(1);
            });

            //微信支付
            WeipayBtn.onClick.Add(() =>
            {
                Recharge(2);
            });

            //关闭支付界面
            maskClick.onClick.Add(() =>
            {
                PaySelectTip.SetActive(false);
            });

            ButtonClick(ShopType.Wing, UIType.UIWingItem, wingGrid.transform);
            #endregion
        }

        //支付充值 payType 支付类型：1支付宝，2微信
        private void Recharge(int payType)
        {
            if (payType == 1)
            {
                PlatformHelper.pay(Constants.PAY_TYPE_ALIPAY, "AndroidCallBack", "GetPayResult", UIItemComponent.SetRequest(shopInfo).ToJson());
            }
            else
            {
                PlatformHelper.pay(Constants.PAY_TYPE_WX, "AndroidCallBack", "GetPayResult", UIItemComponent.SetRequest(shopInfo).ToJson());
            }
            PaySelectTip.SetActive(false);
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            buttonDic.Clear();
            itemDic.Clear();
            shopInfoDic.Clear();
            shopInfoList.Clear();
            itemList.Clear();
            uiList.Clear();
            uiDic.Clear();
        }

        /// <summary>
        /// 购买详细页面
        /// </summary>
        /// <param name="info"></param>
        /// <param name="tip"></param>
        /// <param name="isCanBuy"></param>
        public void BuyTip(ShopInfo info,string tip,bool isCanBuy)
        {
            buyTip.SetActive(true);
            UIAnimation.ShowLayer(BuyTipS);
            shopInfo = info;
            buyTxt.text = tip;
            this.isCanBuy = isCanBuy;
        }

        /// <summary>
        /// 购买物品
        /// </summary>
        private async void BuyItem()
        {
            GetItemInfo info = new GetItemInfo();
            int shopId = CommonUtil.splitStr_Start(shopInfo.Items.ToString(), ':');
            int count = CommonUtil.splitStr_End(shopInfo.Items.ToString(), ':');
            
            UINetLoadingComponent.showNetLoading();
            G2C_BuyItem g2cBuyItem = (G2C_BuyItem)await SessionComponent.Instance.
                Session.Call(new C2G_BuyItem { UId = PlayerInfoComponent.Instance.uid,ShopId = shopInfo.Id});
            UINetLoadingComponent.closeNetLoading();

            ToastScript.createToast("购买成功");
            GameUtil.changeData(shopId, (int)g2cBuyItem.Count);
            if (g2cBuyItem.ShopType == 2)
            {
                PlayerInfoComponent.Instance.GetPlayerInfo().GoldNum = g2cBuyItem.Wealth;
            }
            if (g2cBuyItem.ShopType == 1)
            {
                PlayerInfoComponent.Instance.GetPlayerInfo().WingNum = g2cBuyItem.Wealth;
            }
            Log.Debug("==" + g2cBuyItem.Wealth);
            Log.Debug("--" + PlayerInfoComponent.Instance.GetPlayerInfo().WingNum);
            Game.Scene.GetComponent<UIComponent>().Get(UIType.UIMain).GetComponent<UIMainComponent>
                ().refreshUI();
            buyTip.SetActive(false);
        }

        public void Pay(ShopInfo info)
        {
            shopInfo = info;
            PaySelectTip.SetActive(true);
        }

        /// <summary>
        /// 打开下拉显示内容
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="height"></param>
        public void SetOpenItemPos(int index,ShopType type,float height)
        {
            if (type == ShopType.Prop)
            {
                SetScrollV(propGrid, index, height);
            }
            if(type == ShopType.Vip)
            {
                SetScrollV(vipGrid,index, height);
            }
        }

        /// <summary>
        /// 点击打开详细下拉内容
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="index"></param>
        /// <param name="height"></param>
        private void SetScrollV(GameObject grid,int index,float height)
        {
            SetGridEnable(grid, false);
            float dis = grid.GetComponent<RectTransform>().rect.height + height;
            grid.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dis);
            for (int i = index + 1; i < grid.transform.childCount; ++i)
            {
                grid.transform.GetChild(i).localPosition = new Vector3(grid.transform.GetChild(i).localPosition.x, grid.transform.GetChild(i).localPosition.y - height, 0);
            }
        }

        /// <summary>
        /// 点击关闭下拉列表
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="index"></param>
        /// <param name="height"></param>
        private void SetScrollD(GameObject grid,int index,float height)
        {
            float dis = grid.GetComponent<RectTransform>().rect.height - height;
            grid.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dis);
            for (int i = index + 1; i < grid.transform.childCount; ++i)
            {
                grid.transform.GetChild(i).localPosition = new Vector3(grid.transform.GetChild(i).localPosition.x, grid.transform.GetChild(i).localPosition.y + height, 0);
            }
        }

        private void SetGridEnable(GameObject grid,bool isEnable)
        {
            grid.GetComponent<GridLayoutGroup>().enabled = isEnable;
            grid.GetComponent<ContentSizeFitter>().enabled = isEnable;
        }

        /// <summary>
        /// 根据类型关闭不同的item点击事件
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <param name="height"></param>
        public void SetCloseItemPos(int index,ShopType type,float height)
        {
            switch (type)
            {
                case ShopType.Prop:
                    SetScrollD(propGrid, index, height);
                    break;
                case ShopType.Vip:
                    SetScrollD(vipGrid, index, height);
                    break;
            }
        }

        /// <summary>
        /// 按钮点击事件
        /// </summary>
        /// <param name="shopType"></param>
        /// <param name="uiType"></param>
        /// <param name="parent"></param>
        private void ButtonClick(ShopType shopType,string uiType,Transform parent)
        {
            ClickShopType(shopType);
            shopInfoList = GetShopInfoByType((int)shopType);
            if (shopInfoList == null)
                shopInfoList = new List<ShopInfo>();
            GameObject bundle = GetItemBundleByType(uiType);
            CreateShopInfoList(shopInfoList, bundle, shopType, parent);
        }

        /// <summary>
        /// 显示vip商城
        /// </summary>
        public void ShowVipTab()
        {
            ButtonClick(ShopType.Vip, UIType.UIVipItem, vipGrid.transform);
        }


        /// <summary>
        /// 显示道具商城
        /// </summary>
        public void ShowPropTab()
        {
            ButtonClick(ShopType.Prop, UIType.UIPropItem, propGrid.transform);
        }

        /// <summary>
        /// 点击不同的商店类型(显隐关系)
        /// </summary>
        /// <param name="type"></param>
        private void ClickShopType(ShopType type)
        {
            foreach(var item in buttonDic)
            {
                if(item.Key == type)
                {
                    item.Value.transform.Find("Select").gameObject.SetActive(true);
                    item.Value.transform.Find("ScrollRect").gameObject.SetActive(true);
                }
                else
                {
                    item.Value.transform.Find("Select").gameObject.SetActive(false);
                    item.Value.transform.Find("ScrollRect").gameObject.SetActive(false);
                }
            }   
        }

        /// <summary>
        /// 通过item的类型获得不同的bundle包
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private GameObject GetItemBundleByType(string itemType)
        {
            ResourcesComponent resourcesComponent = ETModel.Game.Scene.GetComponent<ResourcesComponent>();
            resourcesComponent.LoadBundle($"{itemType}.unity3d");
            return (GameObject)resourcesComponent.GetAsset($"{itemType}.unity3d", $"{itemType}");
        }

        /// <summary>
        /// 创建商店列表
        /// </summary>
        /// <param name="shopInfoList"></param>
        /// <param name="itembundle"></param>
        /// <param name="type"></param>
        /// <param name="tr"></param>
        private void CreateShopInfoList(List<ShopInfo> shopInfoList,GameObject itembundle,ShopType type,Transform tr)
        {
            GameObject obj = null;
            uiList = GetUiListByType(type);
            itemList = GetItemListBytype(type);
            if (itemList == null)
            {
                itemList = new List<GameObject>();
            }
            if (uiList == null)
            {
                uiList = new List<UI>();
            }
            for (int i = 0; i < shopInfoList.Count; ++i)
            {
                if (i < itemList.Count)
                {
                    itemList[i].SetActive(true);
                    obj = itemList[i];
                }
                else
                {
                    obj = UnityEngine.Object.Instantiate(itembundle);
                    obj.transform.SetParent(tr);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = Vector3.zero;
                    UI ui = ComponentFactory.Create<UI, GameObject>(obj);
                    ui.AddComponent<UIItemComponent>();
                    if (itemDic.ContainsKey(type))
                    {
                        itemDic[type].Add(obj);
                    }
                    else
                    {
                        itemList.Add(obj);
                        itemDic.Add(type, itemList);
                    }
                    if (uiDic.ContainsKey(type))
                    {
                        uiDic[type].Add(ui);
                    }
                    else
                    {
                        uiList.Add(ui);
                        uiDic.Add(type, uiList);
                    }
                }
                switch (type)
                {
                    case ShopType.Wing:
                        uiList[i].GetComponent<UIItemComponent>().SetWingItem(shopInfoList[i]);
                        break;
                    case ShopType.Gold:
                        uiList[i].GetComponent<UIItemComponent>().SetGoldItem(shopInfoList[i]);
                        break;
                    case ShopType.Prop:
                        uiList[i].GetComponent<UIItemComponent>().SetItem(shopInfoList[i],i,type);
                        break;
                    case ShopType.Vip:
                        uiList[i].GetComponent<UIItemComponent>().SetItem(shopInfoList[i], i,type);
                        break;
                }
            }
        }

        /// <summary>
        /// 通过商店类型获得不同的item列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<GameObject> GetItemListBytype(ShopType type)
        {
            if (itemDic.ContainsKey(type))
                return itemDic[type];
            return null;
        }

        /// <summary>
        /// 通过类型获得缓存的UI
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<UI> GetUiListByType(ShopType type)
        {
            if (uiDic.ContainsKey(type))
                return uiDic[type];
            return null;
        }

        /// <summary>
        /// 通过商店类型把不同的商店数据保存下来
        /// </summary>
        private void AddShopInfoByType()
        {
            for(int i = 0;i< shopInfoList.Count; ++i)
            {
                ShopInfo shopInfo = shopInfoList[i];
                if (shopInfoDic.ContainsKey(shopInfo.ShopType))
                {
                    List<ShopInfo> list = new List<ShopInfo>();
                    list = shopInfoDic[shopInfo.ShopType];
                    if (!list.Contains(shopInfo))
                        shopInfoDic[shopInfo.ShopType].Add(shopInfo);
                    else
                        Log.Error("已经存在该物品");
                }
                else
                {
                    shopInfoDic.Add(shopInfo.ShopType, new List<ShopInfo>());
                    shopInfoDic[shopInfo.ShopType].Add(shopInfo);
                }
            }
        }

        /// <summary>
        /// 通过类型获得商品信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<ShopInfo> GetShopInfoByType(int type)
        {
            return shopInfoDic[type];
        }
    }
}
