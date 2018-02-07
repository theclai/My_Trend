using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nest_Trader_Web
{
    public class Windows_Keyboard_Class
    {
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        [System.Runtime.InteropServices.DllImport("User32.dll")]

        private static extern bool SetForegroundWindow(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);


        public void Restore_Nest_Trader_Window()
        {
            Windows_Api_Class.WINDOWPLACEMENT Window_Place_Ment = Windows_Api_Class.GetPlacement(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id);

            // while (!Windows_Api_Class.IsWindowVisible(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id))

            while (Window_Place_Ment.showCmd == Windows_Api_Class.ShowWindowCommands.Minimized || Window_Place_Ment.showCmd == Windows_Api_Class.ShowWindowCommands.Hide)
            {
                Windows_Api_Class.SetForegroundWindow(GetDesktopWindow());

                Thread.Sleep(300);


                Windows_Api_Class.ShowWindow(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id, Windows_Api_Class.WindowShowStyle.ShowNormal);

                if (IsIconic(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id))
                {
                    Windows_Api_Class.ShowWindow(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id, Windows_Api_Class.WindowShowStyle.Restore);
                }

                Windows_Api_Class.SetForegroundWindow(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id);

                Application.DoEvents();

                Thread.Sleep(300);

                Window_Place_Ment = Windows_Api_Class.GetPlacement(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id);

                if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                {
                    return;
                }
            }
        }
        public void Send_Keys_To_Window_By_Handle(IntPtr Window_Handle, string Keys_Message, int Repeat_Time = 1, int Sleep_Time = 10, bool Render_Form = true)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
            {
                return;
            }

            for (int Send_Key_Cnt = 0; Send_Key_Cnt < Repeat_Time; Send_Key_Cnt++)
            {
                if (Render_Form)
                {
                    Restore_Nest_Trader_Window();
                    if (!Nest_Trader_Web.Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Window_Handle,Window_Handle))
                    {
                        if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                        {
                            return;
                        }
                        Windows_Api_Class.SetForegroundWindow(Window_Handle);
                        Thread.Sleep(500);
                        if (
                            !Nest_Trader_Web.Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(
                                Window_Handle, Window_Handle))
                        {
                            return;
                        }
                    }

                    //while (!Nest_Trader_Web.Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Window_Handle, Window_Handle))
                    //{
                    //    if (!Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Window_Handle))
                    //    {
                    //        return ;
                    //    }
                    //    Restore_Nest_Trader_Window();

                    //    Thread.Sleep(300);

                    //    Windows_Api_Class.SetForegroundWindow(Window_Handle);

                    //    Thread.Sleep(300);

                    //    Application.DoEvents();

                    //    if (!Nest_Trader_Web.Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Window_Handle, Window_Handle))
                    //    {
                    //        Thread.Sleep(2 * 1000);
                    //    }

                    //    else
                    //    {
                    //        Thread.Sleep(200);

                    //        break;
                    //    }

                    //    if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                    //    {
                    //        return;
                    //    }
                    //}

                    Windows_Api_Class.SetForegroundWindow(Window_Handle);

                    /*
                    while(! Nest_Trader_Form.Windows_Api_Class_Instance. Is_Window_Real_At_Foreground(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id) && !Nest_Trader_Form.Windows_Api_Class_Instance.Is_Window_Real_At_Foreground(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id))
                    {
                        Thread.Sleep(300);

                        if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                        {
                            return;
                        }
                    }

                    */
                }

                SendKeys.SendWait(Keys_Message);

                SendKeys.Flush();

                Thread.Sleep(Sleep_Time);
            }
        }

        public void Send_Keys_Directly_To_Window_By_Handle(IntPtr Window_Handle, ushort Keys_Message, int Repeat_Time = 1, int Sleep_Time = 10)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            const uint WM_KEYDOWN = 0x100;

            for (int Send_Key_Cnt = 0; Send_Key_Cnt < Repeat_Time; Send_Key_Cnt++)
            {

                try
                {
                    Windows_Api_Class.SendMessage(Window_Handle, WM_KEYDOWN, ((IntPtr)Keys_Message), (IntPtr)0);

                }

                catch (Exception)
                {

                }

                Thread.Sleep(Sleep_Time);
            }
        }

        public void Send_Keys_Directly_To_Window_By_Handle(IntPtr Window_Handle, string Keys_Message, int Repeat_Time = 1, int Sleep_Time = 10)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            const uint WM_KEYSEND = 0x000C;

            for (int Send_Key_Cnt = 0; Send_Key_Cnt < Repeat_Time; Send_Key_Cnt++)
            {
                try
                {
                    Windows_Api_Class.SendMessage(Window_Handle, 0x112, IntPtr.Zero, Keys_Message);

                }

                catch (Exception)
                {

                }

                Thread.Sleep(Sleep_Time);
            }
        }

        public void Send_Keys_To_Window_By_Handle_Class(IntPtr Process_Id, string Form_Name, string Keys_Message, bool Wait_To_Handle = true, int Repeat_Time = 1, int Sleep_Time = 10, bool Render_Form = true)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            for (int Send_Key_Cnt = 0; Send_Key_Cnt < Repeat_Time; Send_Key_Cnt++)
            {
                if (Render_Form)
                {
                    while (Render_Form && Windows_Api_Class.SetForegroundWindow(Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Process_Id, Form_Name, Wait_To_Handle)) < 1)
                    {
                        Application.DoEvents();

                        Thread.Sleep(10);
                    }
                }

                SendKeys.SendWait(Keys_Message);

                SendKeys.Flush();

                Thread.Sleep(Sleep_Time);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        public void Click_On_Window_By_Handle(IntPtr Window_Handle)
        {
            try
            {
                Windows_Api_Class.SendMessage(Window_Handle, Windows_Api_Class.WM_Click, 0, 0);
            }

            catch (Exception)
            {

            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------
    }
}
