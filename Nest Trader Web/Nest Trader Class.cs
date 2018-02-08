using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nest_Trader_Web
{
    public class Nest_Trader_Class
    {
        public string Nest_Trader_Path = Path.GetPathRoot(Environment.SystemDirectory) + "Nest Trader Symbols\\";
        public int Max_Fail_Retry_Orders;

        public void Nest_Trader_Connect(string Nest_Trader_User, string Nest_Trader_Pass)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            try
            {
                IntPtr Nest_Trader_Login_Id = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "NEST Trader Investor-Login");

                while ((int)Nest_Trader_Login_Id < 1)
                {
                    Nest_Trader_Login_Id = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "NEST Trader Investor-Login");

                    if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                    {
                        return;
                    }
                }

                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, "^{A}");
                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, "");
                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, "^{A}");
                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, "");
                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, Nest_Trader_User);
                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, "{ENTER}");

                Thread.Sleep(1 * 1000);

                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, Nest_Trader_Pass);
                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Login_Id, "{ENTER}");
            }

            catch (Exception A)
            {

            }

            Thread.Sleep(10 * 1000);

            Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Connect To Nest Trader succeeded !");
        }

        public DateTime Get_Last_Transaction_Date()
        {
            DateTime Max_Order_Date_Time = new DateTime(1970, 1, 1);

            try
            {
                string[] Get_Current_Orders_Array = Get_Current_Orders();

                if (Get_Current_Orders_Array == null || Get_Current_Orders_Array.Length == 0)
                {
                    return Max_Order_Date_Time;
                }

                for (int Order_Selected_Cnt = 0; Order_Selected_Cnt < Get_Current_Orders_Array.Length; Order_Selected_Cnt++)
                {
                    string[] Select_Order_Info_Array = Get_Current_Orders_Array[Order_Selected_Cnt].Split('\t');

                    if (Max_Order_Date_Time < DateTime.Parse(Select_Order_Info_Array[18] + " " + Select_Order_Info_Array[19])) // datetime
                    {
                        DateTime.Parse(Select_Order_Info_Array[18] + " " + Select_Order_Info_Array[19]);
                        Max_Order_Date_Time = DateTime.Parse(Select_Order_Info_Array[18] + " " + Select_Order_Info_Array[19]);  // datetime
                    }
                }

            }

            catch (Exception A)
            {

            }

            return Max_Order_Date_Time;
        }

        public string Get_Sended_Transaction_ID(DateTime Send_DT)
        {
            try
            {
                string[] Get_Current_Orders_Array = Get_Current_Orders();

                if (Get_Current_Orders_Array == null || Get_Current_Orders_Array.Length == 0)
                {
                    return "-1";
                }

                string Selected_Order_Id = "-1";
                DateTime Max_Order_Date_Time = Send_DT;

                for (int Order_Selected_Cnt = 0; Order_Selected_Cnt < Get_Current_Orders_Array.Length; Order_Selected_Cnt++)
                {
                    if (Get_Current_Orders_Array[Order_Selected_Cnt] == null || Get_Current_Orders_Array[Order_Selected_Cnt] == "")
                    {
                        continue;
                    }

                    string[] Select_Order_Info_Array = Get_Current_Orders_Array[Order_Selected_Cnt].Split('\t');

                    if (Max_Order_Date_Time < DateTime.Parse(Select_Order_Info_Array[18] + " " + Select_Order_Info_Array[19])) // datetime
                    {
                        DateTime.Parse(Select_Order_Info_Array[18] + " " + Select_Order_Info_Array[19]);
                        Selected_Order_Id = Select_Order_Info_Array[11]; // Order ID
                        Max_Order_Date_Time = DateTime.Parse(Select_Order_Info_Array[18] + " " + Select_Order_Info_Array[19]);  // datetime
                    }
                }

                if (Max_Order_Date_Time >= Send_DT)
                {
                    return Selected_Order_Id;
                }
            }

            catch (Exception A)
            {

            }

            return "-1";
        }

        public string API_Order_Send(string Chart_ID, string Exchange_Name, string Order_Symbol, string Instrument_Name, string Order_Type,
                                            string Product_Type, string Order_Expiration_Date, string Order_Quantity, string Disc_Quantity, string Order_Price,
                                            string Order_Buy_Sell, string Order_Take_Profit, string Order_Stop_Loss, string Order_Take_Profit_PL, string Order_Stop_Loss_PL, bool Execute_Auto = true, bool Update_On_Positions_Tab = true)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }

            string Return_Value = "-1";

            if (Order_Quantity == "0")
            {
                return null;
            }
            try
            {
                for (int Send_Order_Fails_Cnt = 0; Send_Order_Fails_Cnt <= Max_Fail_Retry_Orders; Send_Order_Fails_Cnt++)
                {
                    if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                    {
                        if (Send_Order_Fails_Cnt >= Max_Fail_Retry_Orders) return "";
                        Send_Order_Fails_Cnt--;
                        continue;
                    }

                    Process.GetProcessById((int)Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id).WaitForInputIdle();
                    Windows_Api_Class.SetForegroundWindow(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id);
                    IntPtr Order_Entry_Handle = IntPtr.Zero;

                    int retryInsertValue = 0;

                    Order_Entry_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "Buy" + " Order Entry");

                    while (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Order_Entry_Handle))
                    {
                        Thread.Sleep(500);
                        Application.DoEvents();

                        if (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Order_Entry_Handle, Order_Entry_Handle))
                        {
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{Esc}");
                        }
                        if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                        {
                            return "";
                        }
                    }

                    Order_Entry_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "Sell" + " Order Entry");

                    Thread.Sleep(250);
                    Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(
                        Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Order_Entry_Handle);

                    while ((int)Order_Entry_Handle < 1 || !Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Order_Entry_Handle))
                    {
                        Nest_Trader_Form.Windows_Keyboard_Class_Instance.Restore_Nest_Trader_Window();

                        Windows_Api_Class.SetForegroundWindow(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id);

                        if (Order_Buy_Sell == "Buy")
                        {
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id, "{F1}");
                        }

                        if (Order_Buy_Sell == "Sell")
                        {
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id, "{F2}");
                        }

                        Order_Entry_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Order_Buy_Sell + " Order Entry");

                        if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                        {
                            return "";
                        }
                    }

                    Nest_Trader_Form.Form_Thread_Class_Instance.Last_Time = DateTime.Now.AddMinutes(-3);
                    DateTime Start_Order_Date_Time = Get_Last_Transaction_Date();
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}", 7);
                    IntPtr Exchange_Name_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(Order_Entry_Handle, 0);
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Handler_Clas_Instances.Select_List_Box_Option_By_Text(Exchange_Name_Handle, Exchange_Name); // Expiry Date
                    Thread.Sleep(250);

                    if (Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Exchange_Name_Handle) != Exchange_Name)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Exchange name not configured correctly on chart ID : " + Chart_ID);

                        return "-1";
                    }

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}");

                    if (Order_Type == "MARKET")
                    {
                        Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{Down}");
                    }

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}", 2);
                    IntPtr Instrument_Ptr = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(Order_Entry_Handle, 6);
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Instrument_Ptr, Instrument_Name);
                    Thread.Sleep(250);

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}");
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, Order_Symbol);

                    Thread.Sleep(500);
                    IntPtr Symbol_Ptr = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(Order_Entry_Handle, 8);

                    retryInsertValue = 0;

                    if (retryInsertValue < 50)
                    {
                        retryInsertValue++;
                        Thread.Sleep(100);
                    }
                    else if (retryInsertValue % 10 == 0 && retryInsertValue < 50)
                    {
                        Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, Order_Symbol);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        retryInsertValue = 0;
                        break;
                    }

                    // Symbol Name
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}", 3);
                    IntPtr Expiry_Date_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(Order_Entry_Handle, 12);

                    if (Order_Expiration_Date == "NA")
                        Order_Expiration_Date = "";

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);

                    Nest_Trader_Form.Windows_Handler_Clas_Instances.Select_List_Box_Option_By_Text(Expiry_Date_Handle, Order_Expiration_Date); // Expiry Date
                    Thread.Sleep(250);
                    retryInsertValue = 0;

                    while (Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Expiry_Date_Handle) != Order_Expiration_Date)
                    {
                        if (retryInsertValue < 20)
                        {
                            retryInsertValue++;
                            Thread.Sleep(100);
                        }
                        else
                        {
                            retryInsertValue = 0;
                            break;
                        }
                    }

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}", 2);
                    //Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, OrderQuantity.ToString());
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);

                    IntPtr Order_Quantity_Ptr = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(Order_Entry_Handle, 14);
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);

                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Quantity_Ptr, "");
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Quantity_Ptr, Order_Quantity.ToString());
                    Thread.Sleep(250);

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);

                    if (Order_Type != "MARKET")
                    {
                        Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}");
                    }

                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}");
                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);

                    retryInsertValue = 0;
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, Disc_Quantity.ToString());
                    Thread.Sleep(250);

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}");

                    retryInsertValue = 0;
                    IntPtr Product_Type_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(Order_Entry_Handle, 19);
                    Nest_Trader_Form.Windows_Handler_Clas_Instances.Select_List_Box_Option_By_Text(Product_Type_Handle, Product_Type); // Expiry Date
                    Thread.Sleep(250);

                    Windows_Api_Class.SetForegroundWindow(Order_Entry_Handle);
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}", 2);

                    if (Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Exchange_Name_Handle) != Exchange_Name)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Exchange name not configured correctly on chart ID : " + Chart_ID);
                        if (Send_Order_Fails_Cnt < Max_Fail_Retry_Orders)
                            continue;
                        return "-1";
                    }

                    if (Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Instrument_Ptr) != Instrument_Name)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Instrument name not configured correctly on chart ID : " + Chart_ID);
                        if (Send_Order_Fails_Cnt < Max_Fail_Retry_Orders)
                            continue;
                        return "-1";
                    }

                    if (Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Symbol_Ptr) != Order_Symbol)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Symbol name not configured correctly on chart ID : " + Chart_ID);
                        if (Send_Order_Fails_Cnt < Max_Fail_Retry_Orders)
                            continue;
                        return "-1";
                    }

                    if (Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Expiry_Date_Handle) != Order_Expiration_Date)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Expiry date not configured correctly on chart ID : " + Chart_ID);
                        if (Send_Order_Fails_Cnt < Max_Fail_Retry_Orders)
                            continue;
                        return "-1";
                    }
                    if (Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Product_Type_Handle) != Product_Type)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Product Type not configured correctly on chart ID : " + Product_Type);
                        if (Send_Order_Fails_Cnt < Max_Fail_Retry_Orders)
                            continue;
                        return "-1";
                    }

                    if (Execute_Auto && Nest_Trader_Form.Windows_Network_Class_Instance.Features_Array[0] == "True")
                    {
                        Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{TAB}");
                        Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{Enter}");
                        IntPtr Order_Transaction_Pass_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "Transaction Password");
                        int Transaction_Pass_Cnt = 0;

                        while ((int)Order_Transaction_Pass_Handle < 1 || !Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Order_Transaction_Pass_Handle))
                        {
                            Order_Transaction_Pass_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "Transaction Password");

                            Transaction_Pass_Cnt++;

                            Thread.Sleep(75);

                            if (Transaction_Pass_Cnt == 7)
                            {
                                break;
                            }
                        }

                        if (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Order_Transaction_Pass_Handle))
                        {
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Transaction_Pass_Handle, Nest_Trader_Form.Nest_Trader_Form_Instance.Login_Trans_Text_Box.Text);
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Transaction_Pass_Handle, "{TAB}");
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Transaction_Pass_Handle, "{Enter}");
                        }

                        if (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Order_Entry_Handle, Order_Entry_Handle))
                        {
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{Esc}");
                        }
                    }

                    while (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Order_Entry_Handle))
                    {
                        Thread.Sleep(500);
                        Application.DoEvents();

                        if (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Order_Entry_Handle, Order_Entry_Handle))
                        {
                            Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Order_Entry_Handle, "{Esc}");
                        }
                        if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                        {
                            return "";
                        }
                    }

                    string Transaction_ID = "-1";// Get_Last_Log()
                    int Transaction_Reader_Cnt = 0;

                    while (Transaction_ID == "-1")
                    {
                        Transaction_ID = Get_Sended_Transaction_ID(Start_Order_Date_Time);
                        Transaction_Reader_Cnt++;
                        Thread.Sleep(50);

                        if (Transaction_Reader_Cnt >= 10)
                        {
                            break;
                        }
                    }

                    if (Transaction_ID == "-1")
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Attempt " + (Send_Order_Fails_Cnt + 1) + " to send order to Nest Trader failed !");

                        continue;
                    }
                    if (Update_On_Positions_Tab)
                    {
                        API_Order_Update(Transaction_ID, Chart_ID, Order_Take_Profit, Order_Stop_Loss, Order_Take_Profit_PL, Order_Stop_Loss_PL, Order_Buy_Sell, Exchange_Name, Order_Symbol, Order_Price, "");
                    }

                    Return_Value = Transaction_ID;

                    break;
                }
            }
            catch (Exception A)
            {

            }

            return Return_Value;
        }

         /*
              1) if Nest Trader is Visible and Order Entry not - its closed forcefully so we return -1
              2) if Nest Trader is not Visible and Order Entry not visible - restart the process
          */

        bool Order_Entry_Is_Opened(IntPtr Order_Entry_Handle)
        {
            return Windows_Api_Class.IsWindowVisible(Order_Entry_Handle);
        }

        bool Nest_Trader_Is_Visible()
        {
            Windows_Api_Class.WINDOWPLACEMENT Window_Place_Ment = Windows_Api_Class.GetPlacement(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id);

            if (Window_Place_Ment.showCmd == Windows_Api_Class.ShowWindowCommands.Minimized || Window_Place_Ment.showCmd == Windows_Api_Class.ShowWindowCommands.Hide || !Windows_Api_Class.IsWindowVisible(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id))
            {
                return false;
            }

            return true;
        }

        public void API_Order_Update(string Transaction_ID, string Chart_ID, string Order_Take_Profit, string Order_Stop_Loss, string Order_Take_Profit_PL, string Order_Stop_Loss_PL, string Order_Buy_Sell, string Exchange_Name, string Order_Symbol, string Order_Price, string Order_Strike)
        {
            try
            {
                Nest_Trader_Form.Form_Thread_Class_Instance.Open_Positions_Tab_Updater(Transaction_ID);
                int[] DGV_Indexes = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1, 0, Transaction_ID);

                if (DGV_Indexes != null && DGV_Indexes.Length != 0)
                {
                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        string Symbol_Price =
                            Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_ID).ToString();

                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar(
                            "Send order to Nest Trader succeeded ! , Order no : " + Transaction_ID + " chart : " +
                            Chart_ID + " exchange : " + Exchange_Name + " symbol : " + Order_Symbol + " " +
                            Order_Buy_Sell + " " + "Opened on @ " + Symbol_Price + " TP : " + Order_Take_Profit +
                            " SL : " + Order_Stop_Loss + " PL TP : " + Order_Take_Profit_PL + " PL SL : " +
                            Order_Stop_Loss_PL);

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[0].Value
                            = Transaction_ID;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[1].Value
                            = Chart_ID;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[7].Value
                            = Order_Strike;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[10]
                            .Value = Order_Price;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[13]
                            .Value = Order_Take_Profit;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[14]
                            .Value = Order_Stop_Loss;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[15]
                            .Value = Order_Take_Profit_PL;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[16]
                            .Value = Order_Stop_Loss_PL;

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();
                    });
                }
            }

            catch (Exception A)
            {

            }
        }

        public bool API_Order_Close(string Exchange_Name, string Inst_Name, string Order_Symbol, string Order_Quantity, string Order_Buy_Sell, string Order_Expiration_Date, string Chart_Id, string Order_Type, string Prod_Type, string Order_Qty, string Order_Price, string Order_TP, string Order_SL, string Profit_Limit_Pl, string SL_Limit_Pl, string Trade_RMS_PL)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return false;
            }

            if (Order_Price == "0" || Order_Price == "" || Order_Price == null)
            {
                return false;
            }

            int Loop_Max_Close = 1;

            try
            {
                if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Retry_Failed_Box.Text.Contains("No Retry"))
                {
                    Loop_Max_Close = int.Parse(Nest_Trader_Form.Nest_Trader_Form_Instance.Retry_Failed_Box.Text[0].ToString()) + 1;
                    Nest_Trader_Form.Nest_Trader_Class_Instance.Max_Fail_Retry_Orders = int.Parse(Nest_Trader_Form.Nest_Trader_Form_Instance.Retry_Failed_Box.Text[0].ToString()) + 1;
                }
                else
                {
                    Nest_Trader_Form.Nest_Trader_Class_Instance.Max_Fail_Retry_Orders = 1;
                }

                string Transaction_ID_String = "";

                for (int Send_Order_Fails_Cnt = 0; Send_Order_Fails_Cnt < Loop_Max_Close; Send_Order_Fails_Cnt++)
                {
                    if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                    {
                        return false;
                    }

                    if (Order_Buy_Sell == "Buy")
                    {
                        Transaction_ID_String = API_Order_Send("", Exchange_Name, Order_Symbol, Inst_Name, Nest_Trader_Form.Nest_Trader_Form_Instance.Order_Type_Combo_Box.Text, Nest_Trader_Form.Nest_Trader_Form_Instance.Product_Type_Combo_Box.Text, Order_Expiration_Date, Order_Quantity, "0", "0", "Sell", "0", "0", "0", "0", true, false);
                    }

                    else
                    {
                        Transaction_ID_String = API_Order_Send("", Exchange_Name, Order_Symbol, Inst_Name, Nest_Trader_Form.Nest_Trader_Form_Instance.Order_Type_Combo_Box.Text, Nest_Trader_Form.Nest_Trader_Form_Instance.Product_Type_Combo_Box.Text, Order_Expiration_Date, Order_Quantity, "0", "0", "Buy", "0", "0", "0", "0", true, false);
                    }

                    if (Transaction_ID_String != "" && Transaction_ID_String != "-1" && Transaction_ID_String != "0" && Transaction_ID_String != null)
                    {
                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.Invoke((MethodInvoker)delegate
                       {
                           Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.Rows.Add(Chart_Id, Exchange_Name,
                               Order_Symbol, Inst_Name, Order_Type, Prod_Type, Order_Expiration_Date, Order_Qty, "",
                               Order_Price, Order_Buy_Sell, Order_TP, Order_SL, Profit_Limit_Pl, SL_Limit_Pl,
                               Trade_RMS_PL, "Closed", DateTime.Now.ToString());
                       });
                        return true;
                    }
                }
            }
            catch (Exception A)
            {

            }

            return false;
        }

        public string[] Get_Log_Messages(DateTime Log_DT_Filter)
        {
            IntPtr Nest_Log_Messages_Handle = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Names_Tree(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id, new string[] { "Message Bar", "Message Bar", "Message Bar" })[0];

            return null;
        }

        public string[] Get_Current_Orders(bool Require_Show_Tab = false)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }
            try
            {
                IntPtr Open_Orders_Tab = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "Trade Book");
                Windows_Api_Class.RECT New_Window_Rect = new Windows_Api_Class.RECT();
                Windows_Api_Class.GetWindowRect(Open_Orders_Tab, ref New_Window_Rect);
                Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Open_Orders_Tab);

                while (((int)Open_Orders_Tab < 1) || (!Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Open_Orders_Tab) && Require_Show_Tab)) // || Nest_Trader_Form.Windows_Handler_Clas_Instances.GetPlacement(Open_Orders_Tab).showCmd == Windows_Handler_Class.ShowWindowCommands.Hide)
                {
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id, "{F8}");
                    Open_Orders_Tab = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "Trade Book");

                    if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                    {
                        return null;
                    }
                }

                IntPtr Open_Orders_DGV = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(Open_Orders_Tab, 0);

                for (int Get_Orders_Cnt = 0; Get_Orders_Cnt < 3; Get_Orders_Cnt++)
                {
                    string[] File_Orders_Array = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_All_Element_Text_Lines(Open_Orders_DGV);

                    if (File_Orders_Array != null)
                    {
                        return File_Orders_Array;
                    }
                }
            }
            catch (Exception A)
            {

            }

            Thread.Sleep(100);

            return null;
        }

        public string[] Get_Rms_Orders(bool Require_Show_Tab = false)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }
            try
            {
                IntPtr RMS_Orders_Tab = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "RMS View Limits");
                Windows_Api_Class.RECT New_Window_Rect = new Windows_Api_Class.RECT();
                Windows_Api_Class.GetWindowRect(RMS_Orders_Tab, ref New_Window_Rect);

                while ((int)RMS_Orders_Tab < 1 || (!Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, RMS_Orders_Tab) && Require_Show_Tab))
                {
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id, "^+{v}");
                    Thread.Sleep(5 * 1000);

                    RMS_Orders_Tab = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "RMS View Limits");

                    if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Run(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id))
                    {
                        return null;
                    }
                }

                IntPtr RMS_All_Clients_Btn = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(RMS_Orders_Tab, 0);

                if (!Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(RMS_All_Clients_Btn).Contains("Show All Clients"))
                {
                    Nest_Trader_Form.Windows_Keyboard_Class_Instance.Send_Keys_To_Window_By_Handle(RMS_All_Clients_Btn, "{Down}", 3);
                }

                IntPtr RMS_Orders_DGV = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Child_By_Index(RMS_Orders_Tab, 41);

                return Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_All_Element_Text_Lines(RMS_Orders_DGV);
            }
            catch (Exception A)
            {

            }

            return null;
        }

        public string[] Add_Account_PL(string[] Current_Orders_Array, string[] RMS_Orders_Array)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }
            if (Current_Orders_Array == null || Current_Orders_Array.Length == 0 || RMS_Orders_Array == null || RMS_Orders_Array.Length == 0)
            {
                return Current_Orders_Array;
            }
            try
            {
                for (int Get_Current_Orders_Cnt = 0; Get_Current_Orders_Cnt < Current_Orders_Array.Length; Get_Current_Orders_Cnt++)
                {
                    if (string.IsNullOrEmpty(Current_Orders_Array[Get_Current_Orders_Cnt]))
                        continue;

                    string[] Get_Current_Orders_Split = Current_Orders_Array[Get_Current_Orders_Cnt].Split('\t');

                    string Order_Client_ID;
                    string Order_Segment;
                    string Order_Symbol;
                    string Order_Expiry_Date;
                    string Order_Inst_Name;
                    string Order_QTY;
                    string Order_Buy_Sell;
                    string Order_Type;

                    try
                    {
                        Order_Client_ID = Get_Current_Orders_Split[7];
                        Order_Segment = Get_Current_Orders_Split[0];
                        Order_Symbol = Get_Current_Orders_Split[13];
                        Order_Expiry_Date = Get_Current_Orders_Split[16];
                        Order_Inst_Name = Get_Current_Orders_Split[14];
                        Order_QTY = Get_Current_Orders_Split[5];
                        Order_Buy_Sell = Get_Current_Orders_Split[2];
                        Order_Type = Get_Current_Orders_Split[24];
                    }

                    catch (Exception)
                    {
                        continue;
                    }

                    for (int Get_RMS_Orders_Cnt = 0; Get_RMS_Orders_Cnt < RMS_Orders_Array.Length; Get_RMS_Orders_Cnt++)
                    {
                        string[] Get_Rms_Orders_Split;
                        string RMS_Client_ID;
                        string RMS_Segment;
                        string RMS_Symbol;
                        string RMS_Expiry_Date;
                        string RMS_Inst_Name;
                        string Rms_QTY = "";
                        string RMS_Buy_Sell;
                        string RMS_Type;

                        try
                        {
                            if (string.IsNullOrEmpty(RMS_Orders_Array[Get_RMS_Orders_Cnt]))
                                continue;

                            Get_Rms_Orders_Split = RMS_Orders_Array[Get_RMS_Orders_Cnt].Split('\t');
                            RMS_Client_ID = Get_Rms_Orders_Split[0];
                            RMS_Segment = Get_Rms_Orders_Split[3];
                            RMS_Symbol = Get_Rms_Orders_Split[4];
                            RMS_Expiry_Date = Get_Rms_Orders_Split[5];
                            RMS_Inst_Name = Get_Rms_Orders_Split[6];
                            Rms_QTY = "";
                            RMS_Buy_Sell = Get_Rms_Orders_Split[11];
                            RMS_Type = Get_Rms_Orders_Split[23];
                        }

                        catch (Exception)
                        {
                            continue;
                        }

                        if (RMS_Segment == Order_Segment && RMS_Symbol == Order_Symbol && RMS_Inst_Name == Order_Inst_Name && RMS_Expiry_Date == Order_Expiry_Date && RMS_Client_ID == Order_Client_ID && RMS_Type == Order_Type)
                        {
                            string RMS_Current_PL = Get_Rms_Orders_Split[19];
                            string RMS_Todays_PL = Get_Rms_Orders_Split[21];

                            if (RMS_Current_PL == "" || RMS_Current_PL == "-")
                            {
                                RMS_Current_PL = "0";
                            }

                            if (RMS_Todays_PL == "" || RMS_Todays_PL == "-")
                            {
                                RMS_Todays_PL = "0";
                            }

                            Current_Orders_Array[Get_Current_Orders_Cnt] = Current_Orders_Array[Get_Current_Orders_Cnt] + "\t" + RMS_Current_PL + "\t" + RMS_Todays_PL;

                            break;
                        }
                    }
                }
            }

            catch (Exception A)
            {

            }

            return Current_Orders_Array;
        }

        public string[] Get_Current_Detailed_Orders(bool Require_Show_Tab = false)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }

            return Add_Account_PL(Get_Current_Orders(Require_Show_Tab), Get_Rms_Orders(Require_Show_Tab));
        }
    }
}
