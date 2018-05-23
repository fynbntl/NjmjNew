﻿using System.Collections.Generic;
using ETHotfix;

namespace ETModel
{
    public class HandCardsComponent : Component
    {
        //所有手牌
        public List<MahjongInfo> library = new List<MahjongInfo>();

        //出的牌
        public readonly List<MahjongInfo> PlayCards = new List<MahjongInfo>();

        //花牌
        public readonly List<MahjongInfo> FaceCards = new List<MahjongInfo>();

        //碰
        public readonly List<MahjongInfo> PengCards = new List<MahjongInfo>();

        //明杠牌
        public readonly List<MahjongInfo> GangCards = new List<MahjongInfo>();

        //明杠牌
        public readonly List<MahjongInfo> MingGangCards = new List<MahjongInfo>();

        //暗杠牌
        public readonly List<MahjongInfo> AnGangCards = new List<MahjongInfo>();
        //身份
        public bool IsBanker { get; set; }

        //是否托管
        public bool IsTrusteeship { get; set; }

        //手牌数
        public int CardsCount
        {
            get { return library.Count; }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            this.library.Clear();
            PlayCards.Clear();
            FaceCards.Clear();
            PengCards.Clear();
            IsTrusteeship = false;
            IsBanker = false;
        }
    }
}