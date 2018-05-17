﻿using ETModel;
using Hotfix;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIItemSystem: AwakeSystem<UIItemComponent>
    {
        public override void Awake(UIItemComponent self)
        {
            self.Awake();
        }
    }

    public class UIItemComponent : Component
    {
        private Image icon;
        private Text nameTxt;
        private Text priceTxt;
        private Button buyBtn;
        private ReferenceCollector rc;
        private int index;
        private ShopInfo shopInfo;

        public void Awake()
        {
            rc = GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            icon = rc.Get<GameObject>("Icon").GetComponent<Image>();
            nameTxt = rc.Get<GameObject>("NameTxt").GetComponent<Text>();
            priceTxt = rc.Get<GameObject>("PriceTxt").GetComponent<Text>();
            buyBtn = rc.Get<GameObject>("BuyBtn").GetComponent<Button>();

            buyBtn.onClick.Add(() =>
            {
                if(shopInfo.CurrencyType == 2)
                {
                    string tip = "";
                    bool isCanBuy = false;
                    //用元宝购买
                    long yuan = PlayerInfoComponent.Instance.GetPlayerInfo().WingNum;
                    if(yuan >= shopInfo.Price)
                    {
                        //可以购买
                        tip = new StringBuilder().Append("确定花费")
                                                        .Append(shopInfo.Price)
                                                        .Append("元宝购买")
                                                        .Append(shopInfo.Name)
                                                        .Append("吗").ToString();
                        isCanBuy = true;
                    }
                    else
                    {
                        //元宝不够
                        tip = "您还没有足够的元宝，现在去充值吧！";
                        isCanBuy = false;
                    }
                    Game.Scene.GetComponent<UIComponent>().Get(UIType.UIShop).GetComponent<UIShopComponent>().BuyTip(shopInfo, tip, isCanBuy);
                }
                else
                {
                    //接购买SDK
                    Debug.Log("暂时还未接sdk");
                }
            });
        }

        public void SetCommonItem(ShopInfo info)
        {
            shopInfo = info;
            nameTxt.text = info.Name;
            icon.sprite = Game.Scene.GetComponent<UIIconComponent>().GetSprite(info.Icon);
            if (info.ShopType == (int)ShopType.Wing)
                priceTxt.text = new StringBuilder().Append(info.Price).Append("元").ToString();
            else
                priceTxt.text = info.Price.ToString();
        }

        public void SetWingItem(ShopInfo info)
        {
            SetCommonItem(info);
            int prop_id = CommonUtil.splitStr_Start(info.Items, ':');
            int prop_num = CommonUtil.splitStr_End(info.Items, ':');
        }

        public void SetGoldItem(ShopInfo info)
        {
            SetCommonItem(info);
            Text disCountTxt = rc.Get<GameObject>("DisCountTxt").GetComponent<Text>();
            Text descTxt = rc.Get<GameObject>("DescTxt").GetComponent<Text>();
            string[] strArr = info.Desc.Split(';');
            string[] itemsArr = info.Items.Split(';');
            int prop_id = CommonUtil.splitStr_Start(info.Items, ':');
            int prop_num = CommonUtil.splitStr_End(info.Items, ':');
            
            descTxt.text = strArr[0];
            disCountTxt.text = strArr[1];
        }

        Button openBtn;
        Button closeBtn;
        public void SetItem(ShopInfo info,int index,ShopType shopType)
        {
            SetCommonItem(info);
            int prop_id = CommonUtil.splitStr_Start(info.Items, ':');
            int prop_num = CommonUtil.splitStr_End(info.Items, ':');
            this.index = index;
            float height = 0;
            if(openBtn == null && closeBtn == null)
            {
                openBtn = rc.Get<GameObject>("OpenBtn").GetComponent<Button>();
                closeBtn = rc.Get<GameObject>("CloseBtn").GetComponent<Button>();
                switch (shopType)
                {
                    case ShopType.Prop:
                        Text descTxt = rc.Get<GameObject>("DescTxt").GetComponent<Text>();
                        Text disCountTxt = rc.Get<GameObject>("DisCountTxt").GetComponent<Text>();
                        string[] strArr = info.Desc.Split(';');
                        descTxt.text = strArr[0];
                        disCountTxt.text = strArr[1];
                        height = 150;
                        break;
                    case ShopType.Vip:
                        nameTxt.text = info.Desc;
                        height = 200;
                        break;
                }
                openBtn.onClick.Add(() =>
                {
                    openBtn.gameObject.SetActive(false);
                    closeBtn.gameObject.SetActive(true);
                    Game.Scene.GetComponent<UIComponent>().Get(UIType.UIShop).GetComponent<UIShopComponent>().SetOpenItemPos(index, shopType, height);

                });
                closeBtn.onClick.Add(() =>
                {
                    closeBtn.gameObject.SetActive(false);
                    openBtn.gameObject.SetActive(true);
                    Game.Scene.GetComponent<UIComponent>().Get(UIType.UIShop).GetComponent<UIShopComponent>().SetCloseItemPos(index, shopType, height);
                });
            }
            
        }
    }
}
