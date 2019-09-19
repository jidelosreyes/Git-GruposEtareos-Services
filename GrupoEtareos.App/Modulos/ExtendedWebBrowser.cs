using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SHDocVw;

namespace GrupoEtareos.App.Modulos
{
    public class ExtendedWebBrowser : WebBrowser
    {
        bool renavigating = false;

        public string UserAgent { get; set; }

        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser browser = sender as WebBrowser;
            using (Bitmap bitmap = new Bitmap(browser.Width, browser.Height))
            {
                browser.DrawToBitmap(bitmap, new Rectangle(0, 0, browser.Width, browser.Height));
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] bytes = stream.ToArray();
                    imgScreenShot.Visible = true;
                    imgScreenShot.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(bytes);
                }
            }
        }

        public ExtendedWebBrowser()
        {
            DocumentCompleted += SetupBrowser;

            //this will cause SetupBrowser to run (we need a document object)
            Navigate("about:blank");
        }

        void SetupBrowser(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            DocumentCompleted -= SetupBrowser;
            SHDocVw.WebBrowser xBrowser = (SHDocVw.WebBrowser)ActiveXInstance;
            xBrowser.BeforeNavigate2 += BeforeNavigate;
            DocumentCompleted += PageLoaded;
        }

        void PageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        void BeforeNavigate(object pDisp, ref object url, ref object flags, ref object targetFrameName,
            ref object postData, ref object headers, ref bool cancel)
        {
            if (!string.IsNullOrEmpty(UserAgent))
            {
                if (!renavigating)
                {
                    headers += string.Format("User-Agent: {0}\r\n", UserAgent);
                    renavigating = true;
                    cancel = true;
                    Navigate((string)url, (string)targetFrameName, (byte[])postData, (string)headers);
                }
                else
                {
                    renavigating = false;
                }
            }
        }
    }
}