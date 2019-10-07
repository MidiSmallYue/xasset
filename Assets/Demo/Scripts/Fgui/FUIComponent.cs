using FairyGUI;

namespace ETModel
{
	/// <summary>
	/// 管理所有顶层UI, 顶层UI都是GRoot的孩子
	/// </summary>
	public class FUIComponent
	{
		public FUI Root;

        public FUIComponent()
        {
            GRoot.inst.SetContentScaleFactor(1280, 720, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
            this.Root = new FUI();
            this.Root.Awake(GRoot.inst);
        }

        public void Dispose()
		{
			//if (IsDisposed)
			//{
			//	return;
			//}

			//base.Dispose();
			
            Root.Dispose();
            Root = null;
        }

		public void Add(FUI ui)
		{
			Root.Add(ui);
		}
		
		public void Remove(string name)
		{
			Root.Remove(name);
		}
		
		public FUI Get(string name)
		{
			return Root.Get(name);
        }
	}
}