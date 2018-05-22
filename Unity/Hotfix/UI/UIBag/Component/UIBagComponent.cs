﻿using ETModel;
using Hotfix;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIBagSystem : AwakeSystem<UIBagComponent>
    {
        public override void Awake(UIBagComponent self)
        {
            self.Awake();
        }
    }

    public class UIBagComponent : Component
    {
        /*UseBtn DescTxt UIItemIcon Grid BgGrid ReturnBtn
         */
        private Button useBtn;
        private Text descTxt;
        private Image uiItemIcon;
        private GameObject grid;
        private GameObject bgGrid;
        private Button returnBtn;
        private GameObject useBg;
        private Button sureBtn;
        private Button cancelBtn;
        private Text useTxt;
        private GameObject bagItem = null;
        private List<GameObject> bagItemList = new List<GameObject>();
        private GameObject bgItem = null;
        private List<GameObject> bgItemList = new List<GameObject>();
        private List<UI> uiList = new List<UI>();
        private Bag item;
        private PropInfo propInfo;
        //private int row = 3;//初始三行
        //private int itemCount = 3;//每行个数

        public void Awake()
        {
            ReferenceCollector rc = GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            useBtn = rc.Get<GameObject>("UseBtn").GetComponent<Button>();
            descTxt = rc.Get<GameObject>("DescTxt").GetComponent<Text>();
            uiItemIcon = rc.Get<GameObject>("UIItemIcon").GetComponent<Image>();
            grid = rc.Get<GameObject>("Grid");
            bgGrid = rc.Get<GameObject>("BgGrid");
            returnBtn = rc.Get<GameObject>("ReturnBtn").GetComponent<Button>();
            useBg = rc.Get<GameObject>("UseBg");
            sureBtn = rc.Get<GameObject>("SureBtn").GetComponent<Button>();
            cancelBtn = rc.Get<GameObject>("CancelBtn").GetComponent<Button>();
            useTxt = rc.Get<GameObject>("UseTxt").GetComponent<Text>();
            returnBtn.onClick.Add(() =>
            {
                Game.Scene.GetComponent<UIComponent>().Remove(UIType.UIBag);
            });
            bagItem = CommonUtil.getGameObjByBundle(UIType.UIBagItem);
            bgItem = CommonUtil.getGameObjByBundle(UIType.UIBagBgL);
            useBg.SetActive(false);

            useBtn.onClick.Add(() =>
            {
                useBg.SetActive(true);
                useTxt.text = new StringBuilder().Append("是否使用道具")
                                                 .Append("\"")
                                                 .Append(propInfo.prop_name)
                                                 .Append("\"").ToString();
            });
            sureBtn.onClick.Add(() =>
            {
                UseItem(item);
            });
            cancelBtn.onClick.Add(() =>
            {
                useBg.SetActive(false);
            });
            GetBagInfoList();
        }

        private void GetBagInfoList()
        {
            //long uid = PlayerInfoComponent.Instance.uid;
            //G2C_BagOperation g2cBag =(G2C_BagOperation) await SessionWrapComponent.Instance.Session.Call(new C2G_BagOperation() { UId = uid });
            CreateItemList(PlayerInfoComponent.Instance.GetBagInfoList());
        }

        private void CreateItemList(List<Bag> itemList)
        {
            GameObject obj = null;
            for (int i = 0; i < itemList.Count; ++i)
            {
                if (itemList[i].Count == 0)
                    continue;
                if (i < bagItemList.Count)
                    obj = bagItemList[i];
                else
                {
                    obj = GameObject.Instantiate(bagItem);
                    obj.transform.SetParent(grid.transform);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = Vector3.zero;
                    bagItemList.Add(obj);
                    UI ui = ComponentFactory.Create<UI, GameObject>(obj);
                    ui.AddComponent<UIBagItemComponent>();
                    uiList.Add(ui);
                }
                if (item != null)
                    SetItemInfo(item);
                else
                    SetItemInfo(itemList[0]);
                uiList[i].GetComponent<UIBagItemComponent>().SetItemInfo(itemList[i], i + 1);
            }
        }

        public void SetItemInfo(Bag item)
        {
            this.item = item;
            propInfo = PropConfig.getInstance().getPropInfoById((int)item.ItemId);
            if (propInfo == null)
                Debug.LogError("道具信息错误");
            useBtn.gameObject.SetActive(propInfo.type == 1);
            uiItemIcon.sprite = Game.Scene.GetComponent<UIIconComponent>().GetSprite(propInfo.prop_id.ToString());
            descTxt.text = propInfo.desc;
        }

        private async void UseItem(Bag item)
        {
            G2C_UseItem g2cBag = (G2C_UseItem)await SessionWrapComponent.Instance.Session.Call(new C2G_UseItem() { UId = PlayerInfoComponent.Instance.uid, ItemId = (int)item.ItemId });
            GameUtil.changeData(item.ItemId, -1);
            if (g2cBag.result == 1)
            {
                Debug.Log("Use Success");
                GetBagInfoList();
                useBg.SetActive(false);
            }   
            else
                Debug.Log("Use Fail");
        }

        //private void SetBagItemL(int count)
        //{
        //    if(count > (row * itemCount))
        //    {
        //        int bgCount = (count - row * itemCount) / itemCount;
        //        if ((count - row * itemCount) % itemCount != 0)
        //            bgCount += 1;
        //        GameObject obj = null;
        //        for(int i = 0;i< bgCount; ++i)
        //        {
        //            if (i < bgItemList.Count)
        //                obj = bgItemList[i];
        //            else
        //            {
        //                obj = GameObject.Instantiate(bgItem);
        //                obj.transform.SetParent(bgGrid.transform);
        //                obj.transform.localScale = Vector3.one;
        //                obj.transform.localPosition = Vector3.zero;
        //                bgItemList.Add(obj);
        //            }
        //        }
        //    }
        //}

        public override void Dispose()
        {
            base.Dispose();
            uiList.Clear();
            bagItemList.Clear();
            item = null;
        }
    }
}