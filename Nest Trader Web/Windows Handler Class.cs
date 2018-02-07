using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace Nest_Trader_Web
{
    public class Windows_Handler_Class
    {
        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public IntPtr Get_Child_By_Index(IntPtr Parent_Handle, int Child_Index)
        {
            try
            {
                return new Windows_Api_Class.WindowHandleInfo(Parent_Handle).GetAllChildHandles()[Child_Index];
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        public IntPtr Get_Children_By_Name(IntPtr Parent_Handle, string Control_Text)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return new IntPtr();
            }

            var Windows_All_Child_Handles = new Windows_Api_Class.WindowHandleInfo(Parent_Handle).GetAllChildHandles();

            for (int Windows_Child_Cnt = 0; Windows_Child_Cnt < Windows_All_Child_Handles.Count; Windows_Child_Cnt++)
            {
                StringBuilder DLL_Process_Message = new StringBuilder(10);

                //GetWindowText(Windows_All_Child_Handles[Windows_Child_Cnt], DLL_Process_Message , DLL_Process_Message.Length);

                //Console.WriteLine(DLL_Process_Message.ToString());

                //Console.WriteLine(Get_Windows_Title_By_Handle(Windows_All_Child_Handles[Windows_Child_Cnt]));

                if (Get_Windows_Title_By_Handle(Windows_All_Child_Handles[Windows_Child_Cnt]).Contains(Control_Text))
                {
                    return Windows_All_Child_Handles[Windows_Child_Cnt];
                }
            }

            return new IntPtr(-1);
        }

        public IntPtr[] Get_Child_By_Name(IntPtr Parent_Handle, string Control_Text)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return new IntPtr[0];
            }

            var Windows_All_Child_Handles = new Windows_Api_Class.WindowHandleInfo(Parent_Handle).GetAllChildHandles();

            IntPtr[] Children_Name = new IntPtr[Windows_All_Child_Handles.Count];

            int Children_Names_Cnt = 0;

            for (int Windows_Child_Cnt = 0; Windows_Child_Cnt < Windows_All_Child_Handles.Count; Windows_Child_Cnt++)
            {
                StringBuilder DLL_Process_Message = new StringBuilder(10);

                //GetWindowText(Windows_All_Child_Handles[Windows_Child_Cnt], DLL_Process_Message , DLL_Process_Message.Length);

                //Console.WriteLine(DLL_Process_Message.ToString());

                //Console.WriteLine(Get_Windows_Title_By_Handle(Windows_All_Child_Handles[Windows_Child_Cnt]));

                if (Get_Windows_Title_By_Handle(Windows_All_Child_Handles[Windows_Child_Cnt]).Contains(Control_Text))
                {
                    Children_Name[Children_Names_Cnt] = Windows_All_Child_Handles[Windows_Child_Cnt];

                    Children_Names_Cnt++;
                }
            }

            return Children_Name;
        }

        public IntPtr[] Get_Child_By_Names_Tree(IntPtr Tree_Parent_Handle, string[] Tree_Child_Names)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return new IntPtr[0];
            }

            IntPtr[] Tree_Child_Searcher = new IntPtr[Tree_Child_Names.Length];

            IntPtr[] Tree_Child_Searcher_Shadow = new IntPtr[Tree_Child_Searcher.Length];


            IntPtr[] Tree_Child_Buffer = new IntPtr[Tree_Child_Names.Length];

            int Tree_Child_Buffer_Offset = 0;


            Tree_Child_Searcher_Shadow = Get_Child_By_Name(Tree_Parent_Handle, Tree_Child_Names[0]);


            Tree_Child_Searcher_Shadow.CopyTo(Tree_Child_Buffer, Tree_Child_Buffer_Offset);

            Tree_Child_Buffer_Offset += Tree_Child_Searcher.Length;


            for (int Tree_Child_Search_1_Cnt = 1; Tree_Child_Search_1_Cnt < Tree_Child_Names.Length; Tree_Child_Search_1_Cnt++)
            {
                Tree_Child_Searcher = new IntPtr[0];

                for (int Tree_Child_Search_2_Cnt = 0; Tree_Child_Search_2_Cnt < Tree_Child_Searcher_Shadow.Length; Tree_Child_Search_2_Cnt++)
                {
                    Tree_Child_Searcher = Get_Child_By_Name(Tree_Child_Searcher_Shadow[Tree_Child_Search_2_Cnt], Tree_Child_Names[Tree_Child_Search_1_Cnt]);

                    Array.Resize(ref Tree_Child_Searcher, Tree_Child_Buffer_Offset + Tree_Child_Searcher.Length);

                    Tree_Child_Searcher.CopyTo(Tree_Child_Buffer, Tree_Child_Buffer_Offset); // we copy them to our parents buffer

                    Tree_Child_Buffer_Offset += Tree_Child_Searcher.Length;
                }

                Tree_Child_Buffer.CopyTo(Tree_Child_Searcher_Shadow, 0); // we copy our parents buffer to temp buffer
            }

            return Tree_Child_Buffer;
        }

        //--------------------------------------------------------------------------------------------------------------

        public IntPtr Get_Form_Handle_By_Process_Id(IntPtr Process_Id, string Form_Name, bool Wait_To_Handle = true)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return new IntPtr();
            }

            StringBuilder DLL_Process_Message;

            int Wait_To_Handle_Cnt = 0;

            while (Wait_To_Handle_Cnt < 1)
            {
                // Console.WriteLine(89870870);

                foreach (var Form_Handle in Windows_Api_Class.EnumerateProcessWindowHandles((int)Process_Id))
                {
                    DLL_Process_Message = new StringBuilder(1000);

                    // Console.WriteLine(4647575);

                    try
                    {
                        Windows_Api_Class.SendMessage(Form_Handle, Windows_Api_Class.WM_GETTEXT, DLL_Process_Message.Capacity, DLL_Process_Message);
                    }

                    catch (Exception)
                    {

                    }

                    // Console.WriteLine(765897696);

                    if (DLL_Process_Message.ToString().Contains(Form_Name))
                    {
                        // Console.WriteLine(7976968);

                        return Form_Handle;
                    }

                    // Console.WriteLine(35353546453);
                }

                //   Console.WriteLine(809807856785);

                Wait_To_Handle_Cnt++;

                if (Wait_To_Handle)
                {
                    Wait_To_Handle_Cnt++;

                    Application.DoEvents();

                    Thread.Sleep(10);
                }

                //  Console.WriteLine(5786585689);


                if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                {
                    // Console.WriteLine(7976965754747);

                    return new IntPtr();
                }
            }

            return new IntPtr(-1);
        }

        public string Get_Windows_Title_By_Handle(IntPtr Window_Handle)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }

            StringBuilder DLL_Process_Message = new StringBuilder(1000);

            try
            {
                Windows_Api_Class.SendMessage(Window_Handle, Windows_Api_Class.WM_GETTEXT, DLL_Process_Message.Capacity, DLL_Process_Message);
            }

            catch (Exception)
            {

            }

            return DLL_Process_Message.ToString();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] Get_List_Box_Options(IntPtr List_Box_Handle)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }

            string List_Box_Options_Collector = "";

            string Last_Box_Option = "";

            int List_Box_Cnt = 0;

            for (List_Box_Cnt = 0; List_Box_Cnt < List_Box_Cnt + 1; List_Box_Cnt++)
            {
                if (Last_Box_Option == Get_Windows_Title_By_Handle(List_Box_Handle))
                {
                    break;
                }

                Last_Box_Option = Get_Windows_Title_By_Handle(List_Box_Handle);

                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{UP}");

                Thread.Sleep(500);
            }

            Last_Box_Option = "";

            for (List_Box_Cnt = 0; List_Box_Cnt < List_Box_Cnt + 1; List_Box_Cnt++)
            {
                if (Last_Box_Option == Get_Windows_Title_By_Handle(List_Box_Handle))
                {
                    break;
                }

                List_Box_Options_Collector += Get_Windows_Title_By_Handle(List_Box_Handle) + ",";

                Last_Box_Option = Get_Windows_Title_By_Handle(List_Box_Handle);

                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{Down}");

                Thread.Sleep(200);
            }

            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{Up}", List_Box_Cnt);

            return List_Box_Options_Collector.Take(List_Box_Options_Collector.Length - 1).ToString().Split(',');
        }

        public void Select_List_Box_Option_By_Text(IntPtr List_Box_Handle, string Option_Text)
        {
            if (Get_Windows_Title_By_Handle(List_Box_Handle) == Option_Text)
            {
                return;
            }

            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }
            if (Get_Windows_Title_By_Handle(List_Box_Handle) == Option_Text)
            {
                return;
            }


            string Last_Box_Option = "";

            while (Last_Box_Option != Get_Windows_Title_By_Handle(List_Box_Handle))
            {
                if (Get_Windows_Title_By_Handle(List_Box_Handle) == Option_Text)
                    break;
                Last_Box_Option = Get_Windows_Title_By_Handle(List_Box_Handle);
                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{UP}");
            }

            if (Get_Windows_Title_By_Handle(List_Box_Handle) != Option_Text)
            {
                Last_Box_Option = "";

                while (Last_Box_Option != Get_Windows_Title_By_Handle(List_Box_Handle))
                {
                    if (Get_Windows_Title_By_Handle(List_Box_Handle) == Option_Text)
                        break;
                    Last_Box_Option = Get_Windows_Title_By_Handle(List_Box_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle,
                        "{Down}");
                }

            }


            //int List_Box_Length = Get_List_Box_Options_Length(List_Box_Handle);

            //Thread.Sleep(500);

            //for (int List_Box_Cnt = 0; List_Box_Cnt < List_Box_Length; List_Box_Cnt++)
            //{
            //    if (Get_Windows_Title_By_Handle(List_Box_Handle) == Option_Text)
            //    {
            //        return;
            //    }
            //  //  Thread.Sleep(500);
            //    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{Down}");


            //}
            // Thread.Sleep(100);
        }

        public void Select_List_Box_Option_By_Index(IntPtr List_Box_Handle, int List_Box_Index)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Get_List_Box_Options_Length(List_Box_Handle);

            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{Down}", List_Box_Index);
        }

        public int Get_List_Box_Options_Length(IntPtr List_Box_Handle)
        {
            string Last_Box_Option = "";

            for (int List_Box_Cnt = 0; List_Box_Cnt < List_Box_Cnt + 1; List_Box_Cnt++)
            {
                if (Last_Box_Option == Get_Windows_Title_By_Handle(List_Box_Handle))
                {
                    break;
                }

                Last_Box_Option = Get_Windows_Title_By_Handle(List_Box_Handle);

                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{UP}");

                Thread.Sleep(500);
            }

            Last_Box_Option = "";

            for (int List_Box_Cnt = 0; List_Box_Cnt < List_Box_Cnt + 1; List_Box_Cnt++)
            {
                if (Last_Box_Option == Get_Windows_Title_By_Handle(List_Box_Handle))
                {
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{UP}", List_Box_Cnt, List_Box_Cnt);

                    Thread.Sleep(500);

                    return List_Box_Cnt + 1;
                }

                Last_Box_Option = Get_Windows_Title_By_Handle(List_Box_Handle);

                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(List_Box_Handle, "{Down}");

                Thread.Sleep(500);
            }

            return -1;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] Get_All_Element_Text_Lines(IntPtr Element_Pointer)
        {
            AutomationElement AE_Tree_View;

            try
            {
                AE_Tree_View = AutomationElement.FromHandle(Element_Pointer);
            }

            catch (Exception)
            {
                return null;
            }

            TreeWalker Tree_Walker_Searcher = TreeWalker.ContentViewWalker;

            Stopwatch New_SP = new Stopwatch();



            string Tree_Walker_Buffer = "";


            New_SP.Start();

            try
            {
                for (AutomationElement AE_Tree_Child_Walker = Tree_Walker_Searcher.GetFirstChild(AE_Tree_View, CacheRequest.Current); AE_Tree_Child_Walker != null; AE_Tree_Child_Walker = Tree_Walker_Searcher.GetNextSibling(AE_Tree_Child_Walker, CacheRequest.Current))
                {
                    for (AutomationElement AE_Tree_Child_Line_Walker = Tree_Walker_Searcher.GetFirstChild(AE_Tree_Child_Walker, CacheRequest.Current); AE_Tree_Child_Line_Walker != null; AE_Tree_Child_Line_Walker = Tree_Walker_Searcher.GetNextSibling(AE_Tree_Child_Line_Walker, CacheRequest.Current))
                    {
                        Tree_Walker_Buffer += AE_Tree_Child_Line_Walker.Current.Name + "\t";

                        //string s = AE_Tree_Child_Line_Walker.Current.Name;
                    }

                    Tree_Walker_Buffer += "\n";
                }
            }

            catch (Exception)
            {

            }


            New_SP.Stop();

            // Console.WriteLine(New_SP.ElapsedMilliseconds);

            //Console.WriteLine(Fast_SB.ToString());

            if (Tree_Walker_Buffer.Length > 0)
            {
                try
                {
                    return Tree_Walker_Buffer.Split('\n');
                }

                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        public WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(
            IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }
    }
}
