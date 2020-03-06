using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace OpcClientForMetering
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            bool createNew;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                {
                    Application.ThreadException += Application_ThreadException;
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
                else
                {
                    MessageBox.Show("目前已有一个程序在运行，请勿重复运行程序", "提示"); System.Threading.Thread.Sleep(1000);
                    System.Environment.Exit(1);
                }
            }


        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                Form1.logger.Debug("error,win [{}]",e.ToString());
                Form1.logger.Debug("error,win [{}]", e.Exception.ToString());

                MessageBox.Show(e.Exception.Message);
            }
            catch { }

        }
        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Form1.logger.Fatal("**************MyHandler caught : ", e.Message);
            Form1.logger.Fatal("**************error caught : [{}]", e.ToString());
        }
    }
}
