namespace ETModel
{
	[Config(AppType.Client)]
	public partial class RoomConfigCategory : ACategory<RoomConfig>
	{
	}

	public class RoomConfig: IConfig
	{
		public long Id { get; set; }
		public string Name;
		public int ServiceCharge;
		public int Multiples;
		public int MinThreshold;

	    /// <summary>
	    /// 6λ����id
	    /// </summary>
	    public int FriendRoomId { get; set; }

	    /// <summary>
	    /// ����
	    /// </summary>
	    public int JuCount { get; set; }

	    /// <summary>
	    /// ����userid
	    /// </summary>
	    public long MasterUserId { get; set; }

	    /// <summary>
	    /// �Ƿ񹫿�
	    /// </summary>
	    public bool IsPublic { get; set; }

        /// <summary>
        /// ����Կ��
        /// </summary>
	    public int KeyCount { get; set; }
	}
}
