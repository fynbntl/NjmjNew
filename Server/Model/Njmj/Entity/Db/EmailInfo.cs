﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ETModel
{
    public class EmailInfo : EntityDB
    {
        public long UId { get; set; }
        public string EmailTitle { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
        public bool IsRead { get; set; }
        public string RewardItem { get; set; }
    }
}