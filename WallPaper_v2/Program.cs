using System;
using System.Xml;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WallPaper_v2
{
    class Program
    {
        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uiParam, String pvParam, UInt32 fWinIni);
        private static readonly UInt32 SPI_SETDESKWALLPAPER = 0x14;//Flag to set wallpaper
        private static readonly UInt32 SPIF_UPDATEINIFILE = 0x01;//Flag to update changes
        private static readonly UInt32 SPIF_SENDWININICHANGE = 0x02;//Flag to broadcast changes

        private static string Get_Image_Url()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("https://www.bing.com/HPImageArchive.aspx?format=xml&idx=0&n=1&mkt=en-US");
                Console.WriteLine("Xml Loaded");

                try
                {
                    XmlNode node = doc.DocumentElement.SelectSingleNode("/images/image/url");
                    string url = node.InnerText;
                    Console.WriteLine("Url Extracted");
                    url = "http://www.bing.com" + url.Replace("1366x768", "1920x1080");//string.Replace(string1, string2) searches for string1 then replaces it with string2 but if string1 not found then no replancement is done. COOL
                    Console.WriteLine("Url Processed");
                    Console.WriteLine("\t" + url);
                    return url;
                }
                catch (System.NullReferenceException ex)
                {
                    Console.WriteLine("\tImage Url Not Found; XML Parsing Error\n\t"+ex.Message);
                    throw;
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("\tERROR: 404 XML Document Not Found\n\t" + ex.Message);
                throw;
            }
            
        }
        private static void Download(string url, string output)
        {
            try
            {
                WebClient wc = new WebClient();
                Console.WriteLine("Initiating Download");
                wc.DownloadFile(url, output);
                Console.WriteLine("Download Complete");
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("\tERROR: 404 Image Not Found\n\t"+ex.Message);
                throw;
            }
            
        }
        private static void SetWallPaper(string imgPath)
        {
            Console.WriteLine("Setting Wallpaper");
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"\Control Panel\Desktop\", true);//true indicates the registry key is accessed in write mode
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imgPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            Console.WriteLine("WallPaper Set");
        }
        static void Main(string[] args)
        {
            bool NetConAvailable = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (NetConAvailable)
            {
                try
                {
                    string imgPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory).ToString() + @"\WallPaper.jpg";
                    Console.WriteLine("Resource path generated");
                    Console.WriteLine("\t" + imgPath);
                    Download(Get_Image_Url(), imgPath);
                    SetWallPaper(imgPath);
                }
                catch (Exception)
                {
                    Environment.Exit(0);   
                }

            }
            else
            {
                Console.WriteLine("No Network Connection Available");
            }
            //Console.ReadLine();
            
        }
    }
}
/*
    Reference :
    1. http://www.jasinskionline.com/windowsapi/ref/s/systemparametersinfo.html accessed: 17 September 2017 21:44
          SystemParametersInfo Function 
          public static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uiParam, String pvParam, UInt32 fWinIni );
          SPI_SETDESKWALLPAPER = 20(dec) or, 0x14(hex)->prefered
          Set the current desktop wallpaper bitmap(Windows 7 and Lower). uiParam must be 0. pvParam is a String holding the filename of the bitmap(jpg/jpeg/png) file to use as the wallpaper. 
*/
