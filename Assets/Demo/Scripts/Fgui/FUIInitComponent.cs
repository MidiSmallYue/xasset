using FairyGUI;

namespace ETModel
{
    public class FUIInitComponent
    {
        public const string DefaultFont = "FZXuanZhenZhuanBianS-R-GB";
        public static string ModelPackageName = "FUI/Model";
        private UIPackage modelPackage;

        public void Awake()
        {
            UIConfig.defaultFont = DefaultFont;
            modelPackage = UIPackage.AddPackage(ModelPackageName);
            //ModelBinder.BindAll();
        }

        public void Dispose()
		{
			//if (IsDisposed)
			//{
			//	return;
			//}

			//base.Dispose();

            if(modelPackage != null)
            {
                var p = UIPackage.GetByName(modelPackage.name);

                if(p != null)
                {
                    UIPackage.RemovePackage(modelPackage.name);
                }

                modelPackage = null;
            }
        }
    }
}