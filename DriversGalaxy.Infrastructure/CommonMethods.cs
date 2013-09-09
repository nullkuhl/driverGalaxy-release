
namespace DriversGalaxy.Infrastructure
{
    public static class CommonMethods
    {
        /// <summary>
        /// Converts size in KB or MB depending on size
        /// </summary>
        /// <param name="ulSize"></param>
        /// <param name="strRetSize"></param>
        public static void getSizeInKBMBGB(double ulSize, ref string strRetSize)
        {
            if (ulSize <= 0)
            {
                strRetSize = string.Empty;
                return;
            }
            double dwKBDivisor = 1024;
            double dwMBDivisor = 1024 * 1024;
            double dwGBDivisor = 1024 * 1024 * 1024;

            strRetSize = string.Empty;

            if (ulSize > dwGBDivisor)
            {
                strRetSize = string.Format((("{0:0.00} GB")), (ulSize / dwGBDivisor));

            }
            else if (ulSize > dwMBDivisor)
            {
                strRetSize = string.Format((("{0:0.00} MB")), (ulSize / dwMBDivisor));
            }
            else if (ulSize > dwKBDivisor)
            {
                strRetSize = string.Format((("{0:0.00} KB")), (ulSize / dwKBDivisor));
            }
            else
            {
                strRetSize = string.Format(("{0:0.00}"), ulSize);
            }
        }
    }
}
