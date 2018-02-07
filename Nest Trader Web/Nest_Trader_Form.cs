using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Nest_Trader_Web.NestActorSystem.Actor;
using Nest_Trader_Web.NestActorSystem.Messages;

//  using HWND = System.IntPtr;



namespace Nest_Trader_Web
{

    public partial class Nest_Trader_Form : Form
    {
        public static Nest_Trader_Form Nest_Trader_Form_Instance;


        public static Form_Interactive_Class Form_Interactive_Class_Instance;
        public static Form_Thread_Class Form_Thread_Class_Instance;
        public static Meta_Trader_Class Meta_Trader_Class_Instance;
        public static Nest_Trader_Class Nest_Trader_Class_Instance;
        public static Windows_Api_Class Windows_Api_Class_Instance;
        public static Windows_Encrypt_Class Windows_Encrypt_Class_Instance;
        public static Windows_Handler_Class Windows_Handler_Clas_Instances;
        public static Windows_Keyboard_Class Windows_Keyboard_Class_Instance;
        public static Windows_Network_Class Windows_Network_Class_Instance;

        public static ActorSystem NestActorsSystem;

        public  IActorRef DayTraderSendActorRef;
        public IActorRef APIOrderSendActorRef;
        /// <summary>Contains functionality to get all the open windows.</summary>

        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, string> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();


        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public Nest_Trader_Form()
        {
            InitializeComponent();

            this.Hide();

            Nest_Trader_Form_Instance = this;
            NestActorsSystem = ActorSystem.Create("NestActorsSystem");
            DayTraderSendActorRef = NestActorsSystem.ActorOf(Props.Create(() => new DaysTradeSendOrderActor()),
                "DaysTradeSendOrderActor");
            APIOrderSendActorRef = NestActorsSystem.ActorOf(Props.Create(() => new APIOrderSendActor()),"APIOrderSendActor");
            Form_Interactive_Class_Instance = new Form_Interactive_Class();
            Form_Thread_Class_Instance = new Form_Thread_Class();
            Meta_Trader_Class_Instance = new Meta_Trader_Class();
            Nest_Trader_Class_Instance = new Nest_Trader_Class();
            Windows_Api_Class_Instance = new Windows_Api_Class();
            Windows_Encrypt_Class_Instance = new Windows_Encrypt_Class();
            Windows_Handler_Clas_Instances = new Windows_Handler_Class();
            Windows_Keyboard_Class_Instance = new Windows_Keyboard_Class();
            Windows_Network_Class_Instance = new Windows_Network_Class();

            Clear_All_Data();


            Load_Log_Messages();

            this.Shown += Nest_Trader_Form_Shown;


            Days_Trades_Import_From_CSV();

            Load_Trade_Archives();


            this.FormClosing += Nest_Trader_Form_FormClosing;

            Delete_Api_Order_Write();


            Load_File_Class(Nest_Trader_Class_Instance.Nest_Trader_Path + "File Nest Trader.csv", 2);

            NEST_Status_Label.Text = "Connecting to server ...";


            Load_File_Class(Nest_Trader_Class_Instance.Nest_Trader_Path + "File Nest Trader.csv", 1);


            Form_Thread_Class_Instance.Nest_Trader_Timer.Enabled = true;

            Form_Thread_Class_Instance.Nest_Trader_Timer.Elapsed += Form_Thread_Class_Instance.Nest_Trader_Timer_Elapsed;


            Form_Thread_Class_Instance.Nest_Trader_PB_Timer.Enabled = true;

            Form_Thread_Class_Instance.Nest_Trader_PB_Timer.Elapsed += Form_Thread_Class_Instance.Nest_Trader_PB_Timer_Elapsed;


            Form_Thread_Class_Instance.Nest_Trader_Exit_Timer.Enabled = true;

            Form_Thread_Class_Instance.Nest_Trader_Exit_Timer.Elapsed += Form_Thread_Class_Instance.Nest_Trader_Exit_Timer_Elapsed;


            Auto_Trade_Box.SelectedValueChanged += Auto_Trade_Box_SelectedValueChanged;


            Import_Nest_Trader_Symbol_Settings(Nest_Trader_Class_Instance.Nest_Trader_Path + "Symbol Nest Saver.csv");

            Set_Chart_Loader_Last_Cnt_Invoke();


            Nest_Trader_Form_On_Load();


            Thread New_Thread_Call_Back = new Thread(() => Windows_Network_Class_Instance.Nest_Trader_Server_Thread());

            New_Thread_Call_Back.Start();
        }

        private void Nest_Trader_Form_Shown(object sender, EventArgs e)
        {
            Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("New Start Time Log !", true);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear_All_Data()
        {
            for (int i = 0; i < 50; i++)
            {
                if (File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + i + " Price.txt"))
                {
                    File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + i + " Price.txt");
                }

                if (File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + i + " Requests.txt"))
                {
                    File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + i + " Requests.txt");
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Freeze_Form()
        {
            Days_Trades_Delete_All_Btn.Enabled = false;

            Days_Trades_Execute_Manually_Btn.Enabled = false;

            Days_Trades_Delete_Selected_Btn.Enabled = false;

            Days_Trades_Set_As_Executed_Btn.Enabled = false;


            Load_Settings_Btn.Enabled = false;

            Symbol_Settings_Save_Btn.Enabled = false;

            Symbol_Settings_Import_Symbols_Btn.Enabled = false;

            Symbol_Settings_Delete_Selected_Btn.Enabled = false;

            Auto_Trade_Box.Enabled = false;

            Start_Auto_Trade_Btn.Enabled = false;

            Square_Of_All_Btn.Enabled = false;

            Delete_Selected_Open_Positions.Enabled = false;

            this.Update();
        }

        public void Heat_Form()
        {
            Days_Trades_Delete_All_Btn.Enabled = true;

            Days_Trades_Execute_Manually_Btn.Enabled = true;

            Days_Trades_Delete_Selected_Btn.Enabled = true;

            Days_Trades_Set_As_Executed_Btn.Enabled = true;


            Load_Settings_Btn.Enabled = true;

            Symbol_Settings_Save_Btn.Enabled = true;

            Symbol_Settings_Import_Symbols_Btn.Enabled = true;

            Symbol_Settings_Delete_Selected_Btn.Enabled = true;

            Auto_Trade_Box.Enabled = true;

            Start_Auto_Trade_Btn.Enabled = true;

            Square_Of_All_Btn.Enabled = true;

            Delete_Selected_Open_Positions.Enabled = true;

            this.Update();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        /*

        public void Write_System_Log(string Method_Name , Exception Error_Message)
        {
            string System_Log_File = "";

            try
            {
                System_Log_File = File.ReadAllText("C:\\System Log - Nest Trader.txt");
            }

            catch (Exception)
            {

            }


            System_Log_File += DateTime.Now + " , " + Method_Name + "," + Error_Message.StackTrace + "," + Error_Message.Message + "\r\n";


            File.WriteAllText("C:\\System Log - Nest Trader.txt" , System_Log_File);
        }

        public void Write_System_Log(string Method_Name, string Log_Message)
        {
            string System_Log_File = "";

            try
            {
                System_Log_File = File.ReadAllText("C:\\System Log - Nest Trader.txt");
            }

            catch (Exception)
            {

            }


            System_Log_File += DateTime.Now + " , " + Method_Name + "," + Log_Message + "\r\n";


            File.WriteAllText("C:\\System Log - Nest Trader.txt", System_Log_File);
        }

        */

        //---------------------------------------------------------------------------------------------------------------------------------------------------------


        public void Set_Chart_Loader_Last_Cnt_Invoke()
        {
            for (int DGV_3_Cnt = 0; DGV_3_Cnt < dataGridView3.RowCount; DGV_3_Cnt++)
            {
                object DGV_3_Chart_Id_Obj = dataGridView3[1, DGV_3_Cnt].Value;

                if (DGV_3_Chart_Id_Obj == null || DGV_3_Chart_Id_Obj.ToString() == "")
                {
                    continue;
                }

                Meta_Trader_Class_Instance.Chart_Loader_Last_Cnt = Math.Max(Meta_Trader_Class_Instance.Chart_Loader_Last_Cnt, int.Parse(DGV_3_Chart_Id_Obj.ToString()));
            }
        }

        public void Delete_Api_Order_Write()
        {
            for (int DGV_Cnt_3 = 1; ; DGV_Cnt_3++)
            {
                string File_Chart_Id = Nest_Trader_Class_Instance.Nest_Trader_Path + DGV_Cnt_3 + " Requests.txt";

                if (!File.Exists(File_Chart_Id))
                {
                    break;
                }

                try
                {
                    File.Delete(File_Chart_Id);
                }

                catch (Exception)
                {

                }
            }
        }

        private void Nest_Trader_Form_On_Load()
        {
            if (!File.Exists(Nest_Trader_Class_Instance.Nest_Trader_Path + "Trades DB.csv"))
            {
                return;
            }

            string[] DGV_String_Array = File.ReadAllLines(Nest_Trader_Class_Instance.Nest_Trader_Path + "Trades DB.csv");


            for (int Row_Cnt = 0; Row_Cnt < DGV_String_Array.Length; Row_Cnt++)
            {
                string[] Row_Split = DGV_String_Array[Row_Cnt].Split(',');

                try
                {
                    dataGridView1.Rows.Add(Row_Split[0], Row_Split[1], Row_Split[2], Row_Split[3], Row_Split[4], Row_Split[5], Row_Split[6],
                                           Row_Split[7], Row_Split[8], Row_Split[9], Row_Split[10], Row_Split[11], Row_Split[12], Row_Split[13],
                                           Row_Split[14], Row_Split[15], Row_Split[16], Row_Split[17], Row_Split[18], Row_Split[19], Row_Split[20], Row_Split[21]);
                }

                catch (System.IndexOutOfRangeException)
                {

                }
            }

            dataGridView1.Update();
        }


        public bool Form_Closed = false;

        private void Nest_Trader_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            e.Cancel = true;

            Form_Closed = true;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------


        public IntPtr Nest_Trader_Process_Id = IntPtr.Zero;

        public IntPtr Nest_Trader_Id = IntPtr.Zero;


        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);


        public IntPtr Get_Open_Windows(IntPtr Parent_Process)
        {
            foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
            {
                int Process_ID; 

                IntPtr handle = window.Key;

                GetWindowThreadProcessId(handle, out Process_ID);

                string title = window.Value;

                Console.WriteLine("{0}: {1}: {2}", handle, title , Process_ID);


                IntPtr Get_Parent_Window = GetParent(handle);

                if (Process_ID == (int)Parent_Process)
                {
                    if (Windows_Handler_Clas_Instances.Get_Windows_Title_By_Handle(Get_Parent_Window) != "")
                    {
                        return Get_Parent_Window;
                    }

                    return handle;
                }
            }

            return IntPtr.Zero;
        }

        public void Initialize_Process_Id(string Process_Name)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Nest_Trader_Id = IntPtr.Zero;

            try
            {
                Nest_Trader_Process_Id = new IntPtr(Process.GetProcessesByName(Process_Name).First().Id);

                Nest_Trader_Id = Get_Open_Windows(Nest_Trader_Process_Id);// Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Process_Id, "", false); // 5052476 , To NEST Trader Investor
            }

            catch (InvalidOperationException f)
            {
                Thread.Sleep(100);
            }
        }

        public bool Nest_Trader_Process_Run(IntPtr Nest_Trader_Process_Id_Local)
        {
            Process Nest_Trader_Process = null;

            try
            {
                Nest_Trader_Process = Process.GetProcessById((int)Nest_Trader_Process_Id);
            }

            catch (Exception)
            {

            }

            if (Nest_Trader_Process == null || !Nest_Trader_Process.ProcessName.Contains("NestTrader"))
            {
                return false;
            }

            return true;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------



        //---------------------------------------------------------------------------------------------------------------------------------------------------------

   
        /*
        public string[] Get_List_Box_Options_Shadow(IntPtr List_Box_Handle)
        {
            if (Nest_Trader_Outdated)
            {
                return null;
            }

            bool Send_Message = SendMessage(List_Box_Handle, LB_GETCOUNT, IntPtr.Zero, null);

            int List_Box_Length = (int)Send_Message;

            string[] List_Box_Options_Array = new string[List_Box_Length];

            for (int List_Box_Cnt = 0; List_Box_Cnt < List_Box_Length; List_Box_Cnt++)
            {
                StringBuilder SB_List_Box_Option = new StringBuilder(256);

                SendMessage(List_Box_Handle, LB_GETTEXT, (IntPtr)List_Box_Cnt, SB_List_Box_Option);

                List_Box_Options_Array[List_Box_Cnt] = SB_List_Box_Option.ToString();
            }

            return List_Box_Options_Array;
        }
        */

        public void Set_Text_To_Window_By_Handle(IntPtr Window_Handle, string Text_Message)
        {
            Windows_Api_Class.SendMessage(Window_Handle, Windows_Api_Class.WM_SETTEXT, IntPtr.Zero, Text_Message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------

        public int[] DGV_Column_Searcher(DataGridView DGV_Temp, int DGV_Column_Index, string String_Searcher)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }

            int[] DGV_Indexes = new int[DGV_Temp.Rows.Count];

            int DGV_Index_Cnt = 0;

            for (int DGV_Cnt = 0; DGV_Cnt < DGV_Temp.Rows.Count; DGV_Cnt++)
            {
                try
                {
                    if(DGV_Temp[DGV_Column_Index, DGV_Cnt].Value == null)
                    {
                        continue;
                    }

                    if (DGV_Temp[DGV_Column_Index, DGV_Cnt].Value.ToString().Contains(String_Searcher) && DGV_Temp[DGV_Column_Index, DGV_Cnt].Value.ToString().Length == String_Searcher.Length)
                    {
                        DGV_Indexes[DGV_Index_Cnt] = DGV_Cnt;

                        DGV_Index_Cnt++;
                    }
                }

                catch (Exception g)
                {
                    
                }
            }

            return DGV_Indexes.Take(DGV_Index_Cnt).ToArray();
        }

        public int[] DGV_Rows_Searcher(DataGridView DGV_Temp, int[] DGV_Rows_Index, int Column_Index, string String_Column_Searcher)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }

            int[] DGV_Indexes = new int[DGV_Rows_Index.Length];

            int DGV_Index_Cnt = 0;

            int DGV_Row_Index = 0;

            for (int DGV_Cnt = 0; DGV_Cnt < DGV_Rows_Index.Length; DGV_Cnt++)
            {
                DGV_Row_Index = DGV_Rows_Index[DGV_Cnt];

                if (DGV_Temp[Column_Index, DGV_Row_Index].Value.ToString() == String_Column_Searcher)
                {
                    DGV_Indexes[DGV_Index_Cnt] = DGV_Row_Index;

                    DGV_Index_Cnt++;
                }
            }

            return DGV_Indexes.Take(DGV_Index_Cnt).ToArray();
        }


        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------

        /*
        string[] Semi_Auto_Time_Box_Split = Semi_Auto_Clear_Time_Text_Box.Text.Split(' ');

        string Time_Multiplier_Name = Semi_Auto_Time_Box_Split[1];

        int Time_Multiplier = 1;

        if (Time_Multiplier_Name == "M")
        {
            Time_Multiplier = 60;
        }

        int Get_Semi_Auto_Trade_Timer = int.Parse(Semi_Auto_Time_Box_Split[0]) * Time_Multiplier;
        */

        private void Days_Trades_Delete_All_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();

            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            while (dataGridView2.RowCount > 1)
            {
                try
                {
                    dataGridView2.Rows.RemoveAt(0);

                    dataGridView2.Update();
                }

                catch (Exception d)
                {
                    break;
                }
            }

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        private async void Days_Trades_Execute_Manually_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (Windows_Network_Class_Instance.Features_Array[1] == "False")
            {
                Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Semi trade not allowed on this account !");

                Heat_Form();

                return;
            }


            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

          var t1=  Nest_Trader_Form_Instance.DayTraderSendActorRef.Ask(new DaysTradeSendOrderMessage(false, true), TimeSpan.FromSeconds(60));
            await Task.WhenAll(t1);
            // Days_Trade_Send_Order(false);

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        public async void Days_Trade_Send_Order(bool Auto_Execute, bool Require_Selected = true)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }
            Retry_Failed_Box.Invoke((MethodInvoker) delegate
            {
                if (Retry_Failed_Box.Text[0].ToString().Contains("No Retry"))
                {
                    Nest_Trader_Class_Instance.Max_Fail_Retry_Orders = 1;
                }
                else
                {
                    try
                    {
                        if (!int.TryParse(Retry_Failed_Box.Text[0].ToString(),out Nest_Trader_Class_Instance.Max_Fail_Retry_Orders))
                        {
                            Nest_Trader_Class_Instance.Max_Fail_Retry_Orders += 1;
                        }
                       
                    }

                    catch
                    {
                        Nest_Trader_Class_Instance.Max_Fail_Retry_Orders = 1;
                    }
                }

            });
           
            for (int Data_Grid_View_2 = 0; Data_Grid_View_2 < dataGridView2.Rows.Count; Data_Grid_View_2++)
            {
                if (Require_Selected)
                {
                    if (!dataGridView2[0, Data_Grid_View_2].Selected)
                    {
                        continue;
                    }
                }

                if (dataGridView2[0, Data_Grid_View_2].Value == null)
                {
                    continue;
                }

                if (dataGridView2[15, Data_Grid_View_2].Value.ToString() != "Ready")
                {
                    continue;
                }

                string Chart_ID = dataGridView2[0, Data_Grid_View_2].Value.ToString();
                string Exchange_Name = dataGridView2[1, Data_Grid_View_2].Value.ToString();
                string Order_Symbol = dataGridView2[2, Data_Grid_View_2].Value.ToString();
                string Instrument_Name = dataGridView2[3, Data_Grid_View_2].Value.ToString();
                string Order_Type = dataGridView2[4, Data_Grid_View_2].Value.ToString();
                string Product_Type = dataGridView2[5, Data_Grid_View_2].Value.ToString();
                string Order_Expiration_Date = dataGridView2[6, Data_Grid_View_2].Value.ToString();
                string Order_Quantity = dataGridView2[7, Data_Grid_View_2].Value.ToString();
                string Disc_Quantity = dataGridView2[8, Data_Grid_View_2].Value.ToString();
                string Order_Price = dataGridView2[9, Data_Grid_View_2].Value.ToString();
                string Order_Buy_Sell = dataGridView2[10, Data_Grid_View_2].Value.ToString();
                string Order_Take_Profit = dataGridView2[11, Data_Grid_View_2].Value.ToString();
                string Order_Stop_Loss = dataGridView2[12, Data_Grid_View_2].Value.ToString();
                string Order_Take_Profit_PL = dataGridView2[13, Data_Grid_View_2].Value.ToString();
                string Order_Stop_Loss_PL = dataGridView2[14, Data_Grid_View_2].Value.ToString();

                //-------------------------------------------------------------------------------------------------OrderPrice--------------Order_Slip_Page----------------OrderStopLoss---------------OrderTakeProfit---------------------------------------------------------------------------------------Retention_Order-----------);

                if (!Form_Thread_Class_Instance.Opposite_Signal_Checker(Chart_ID, Order_Buy_Sell))
                {
                    //continue;
                }
                string Order_Ticket = "-1";
                try
                {
                    ApiOrderSendMessage objRequest = new ApiOrderSendMessage(Chart_ID, Exchange_Name, Order_Symbol,
                        Instrument_Name, Order_Type, Product_Type, Order_Expiration_Date, Order_Quantity,
                        Disc_Quantity, Order_Price, Order_Buy_Sell, Order_Take_Profit, Order_Stop_Loss,
                        Order_Take_Profit_PL, Order_Stop_Loss_PL, Auto_Execute, true);


                     Order_Ticket = Nest_Trader_Class_Instance.API_Order_Send(Chart_ID, Exchange_Name, Order_Symbol, Instrument_Name, Order_Type, Product_Type, Order_Expiration_Date, Order_Quantity,
                                                          Disc_Quantity, Order_Price, Order_Buy_Sell, Order_Take_Profit, Order_Stop_Loss, Order_Take_Profit_PL, Order_Stop_Loss_PL, Auto_Execute);

                    //var t1 = APIOrderSendActorRef.Ask(objRequest, TimeSpan.FromSeconds(360));

                    //await Task.WhenAll(t1);
                    //Order_Ticket = (string) t1.Result;
                }
                catch (Exception ex)
                {
                    Order_Ticket = "-1";
                }
            if (Order_Ticket == "-2")
                {
                    continue;
                }

                try
                {
                    dataGridView2.Invoke((MethodInvoker) delegate
                    {
                        dataGridView2.Rows.RemoveAt(Data_Grid_View_2);

                        dataGridView2.Update();
                    });
                }
                catch (Exception ex)
                {
                    
                }
                if (Order_Ticket == "-1")
                {
                    continue;
                }

                //DateTime DT_Now = DateTime.Now;

                //while ((DateTime.Now - DT_Now).TotalMilliseconds < 3 * 1000)
                //{
                //    Application.DoEvents();

                //    Thread.Sleep(10);
                //}

                break;
            }
        }

        private void Days_Trades_Export_To_CSV_Btn_Click(object sender, EventArgs e)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            SFD.Filter = "(*.csv)|";

            SFD.ShowDialog();

            if (SFD.FileName == "" || SFD.FileName == null)
            {
                return;
            }

            string[] Get_All_Info = new string[dataGridView2.RowCount];


            for (int Get_Row_Cnt = 0; Get_Row_Cnt < dataGridView2.RowCount; Get_Row_Cnt++)
            {
                for (int Get_Column_Cnt = 0; Get_Column_Cnt < dataGridView2.ColumnCount; Get_Column_Cnt++)
                {
                    Get_All_Info[Get_Row_Cnt] += Return_String_Null_On_Null(dataGridView2.Rows[Get_Row_Cnt].Cells[Get_Column_Cnt].Value) + ",";
                }

                Get_All_Info[Get_Row_Cnt] = Return_String_Null_On_Null(Get_All_Info[Get_Row_Cnt].Substring(0, Get_All_Info[Get_Row_Cnt].Length - 1));
            }

            File.WriteAllLines(SFD.FileName + ".csv", Get_All_Info);
        }

        public void Days_Trades_Export_To_CSV()
        {
            string[] Get_All_Info = new string[dataGridView2.RowCount];


            for (int Get_Row_Cnt = 0; Get_Row_Cnt < dataGridView2.RowCount; Get_Row_Cnt++)
            {
                if (dataGridView2.Rows[Get_Row_Cnt].Cells[16].Value == null || dataGridView2.Rows[Get_Row_Cnt].Cells[16].Value.ToString() != "Closed")
                {
                    continue;
                }

                for (int Get_Column_Cnt = 0; Get_Column_Cnt < dataGridView2.ColumnCount; Get_Column_Cnt++)
                {
                    Get_All_Info[Get_Row_Cnt] += Return_String_Null_On_Null(dataGridView2.Rows[Get_Row_Cnt].Cells[Get_Column_Cnt].Value) + ",";
                }

                Get_All_Info[Get_Row_Cnt] = Return_String_Null_On_Null(Get_All_Info[Get_Row_Cnt].Substring(0, Get_All_Info[Get_Row_Cnt].Length - 1));
            }

            if (Get_All_Info.Length == 0 || Get_All_Info == null)
            {
                return;
            }


            int Try_Cnt = 0;

            while (Try_Cnt < 5)
            {
                try
                {
                    File.WriteAllLines(Nest_Trader_Class_Instance.Nest_Trader_Path + "Days Trades Raw.csv", Get_All_Info);

                    return;
                }

                catch (Exception)
                {

                }

                Try_Cnt++;

                Thread.Sleep(300);
            }
        }

        public void Days_Trades_Import_From_CSV()
        {
            if (! File.Exists(Nest_Trader_Class_Instance.Nest_Trader_Path + "Days Trades Raw.csv"))
            {
                return;
            }

            string[] Get_All_Info = File.ReadAllLines(Nest_Trader_Class_Instance.Nest_Trader_Path + "Days Trades Raw.csv");


            for (int Get_Row_Cnt = 0; Get_Row_Cnt < Get_All_Info.Length; Get_Row_Cnt++)
            {
                string[] Column_All_Info = Get_All_Info[Get_Row_Cnt].Split(',');

                if(Column_All_Info[0] == null || Column_All_Info[0] == "")
                {
                    continue;
                }

                dataGridView2.Rows.Add(Column_All_Info);
            }

            dataGridView2.Update();
        }

        private void Days_Trades_Delete_Selected_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            for (int Data_Grid_View_2 = 0; Data_Grid_View_2 < dataGridView2.Rows.Count; Data_Grid_View_2++)
            {
                if (!dataGridView2[0, Data_Grid_View_2].Selected)
                {
                    continue;
                }

                try
                {
                    dataGridView2.Rows.RemoveAt(Data_Grid_View_2);
                }

                catch (System.InvalidOperationException f)
                {
                    break;
                }

                dataGridView2.Update();


                break;
            }

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        private async void Days_Trades_Set_As_Executed_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (Windows_Network_Class_Instance.Features_Array[0] == "False")
            {
                Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Auto trade not allowed on this account !");


                Heat_Form();

                return;
            }

            if (Windows_Network_Class_Instance.Features_Array[1] == "False")
            {
                Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Semi trade not allowed on this account !");


                Heat_Form();

                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();
            var t1=Nest_Trader_Form_Instance.DayTraderSendActorRef.Ask(new DaysTradeSendOrderMessage(true, true), TimeSpan.FromSeconds(120));
            await Task.WhenAll(t1);
            // Days_Trade_Send_Order(true);

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }


        public void Save_Trade_Archives()
        {
            string[] Get_All_Info = new string[dataGridView4.RowCount];


            for (int Get_Row_Cnt = 0; Get_Row_Cnt < dataGridView4.RowCount; Get_Row_Cnt++)
            {
                for (int Get_Column_Cnt = 0; Get_Column_Cnt < dataGridView4.ColumnCount; Get_Column_Cnt++)
                {
                    Get_All_Info[Get_Row_Cnt] += Return_String_Null_On_Null(dataGridView4.Rows[Get_Row_Cnt].Cells[Get_Column_Cnt].Value) + ",";
                }

                Get_All_Info[Get_Row_Cnt] = Return_String_Null_On_Null(Get_All_Info[Get_Row_Cnt].Substring(0, Get_All_Info[Get_Row_Cnt].Length - 1));
            }

            if (Get_All_Info.Length == 0 || Get_All_Info == null)
            {
                return;
            }


            int Try_Cnt = 0;

            while (Try_Cnt < 5)
            {
                try
                {
                    File.WriteAllLines(Nest_Trader_Class_Instance.Nest_Trader_Path + "History Trades Raw.csv", Get_All_Info);

                    return;
                }

                catch (Exception)
                {

                }

                Try_Cnt++;

                Thread.Sleep(300);
            }
        }

        public void Load_Trade_Archives()
        {
            if (!File.Exists(Nest_Trader_Class_Instance.Nest_Trader_Path + "History Trades Raw.csv"))
            {
                return;
            }

            string[] Get_All_Info = File.ReadAllLines(Nest_Trader_Class_Instance.Nest_Trader_Path + "History Trades Raw.csv");


            for (int Get_Row_Cnt = 0; Get_Row_Cnt < Get_All_Info.Length; Get_Row_Cnt++)
            {
                string[] Column_All_Info = Get_All_Info[Get_Row_Cnt].Split(',');

                if(Column_All_Info[0] == null || Column_All_Info[0] == "")
                {
                    continue;
                }


                dataGridView4.Rows.Add(Column_All_Info);
            }

            dataGridView4.Update();
        }

        public string Message_Log_Buffer = "";

        public void Save_Log_Messages ()
        {
            int Try_Cnt = 0;

            while (Try_Cnt < 5)
            {
                try
                {
                    File.WriteAllText(Nest_Trader_Class_Instance.Nest_Trader_Path + "Log Window Raw.csv", Message_Log_Buffer);

                    return;
                }

                catch (Exception)
                {

                }

                Try_Cnt++;

                Thread.Sleep(300);
            }
        }

        public void Load_Log_Messages()
        {
            if (!File.Exists(Nest_Trader_Class_Instance.Nest_Trader_Path + "Log Window Raw.csv"))
            {
                return;
            }

            string[] Get_All_Info = File.ReadAllLines(Nest_Trader_Class_Instance.Nest_Trader_Path + "Log Window Raw.csv");


            for (int Get_Row_Cnt = 0; Get_Row_Cnt < Get_All_Info.Length; Get_Row_Cnt++)
            {
                Form_Interactive_Class_Instance.Create_Message_On_Status_Bar(Get_All_Info[Get_Row_Cnt], false , false);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------

        private void Export_To_Csv_Open_Positions_Click(object sender, EventArgs e)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            SFD.Filter = "(*.csv)|";

            SFD.ShowDialog();


            if (SFD.FileName != "" && SFD.FileName != null)
            {
                string[] Get_All_Info = new string[dataGridView1.RowCount];


                for (int Get_Row_Cnt = 0; Get_Row_Cnt < dataGridView1.RowCount; Get_Row_Cnt++)
                {
                    for (int Get_Column_Cnt = 0; Get_Column_Cnt < dataGridView1.ColumnCount; Get_Column_Cnt++)
                    {
                        Get_All_Info[Get_Row_Cnt] += Return_String_Null_On_Null(dataGridView1.Rows[Get_Row_Cnt].Cells[Get_Column_Cnt].Value) + ",";
                    }

                    Get_All_Info[Get_Row_Cnt] = Return_String_Null_On_Null(Get_All_Info[Get_Row_Cnt].Substring(0, Get_All_Info[Get_Row_Cnt].Length - 1));
                }

                File.WriteAllLines(SFD.FileName + ".csv", Get_All_Info);
            }
        }

        private void Delete_Selected_Open_Positions_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();



            for (int Data_Grid_View_1 = 0; Data_Grid_View_1 < dataGridView1.Rows.Count; Data_Grid_View_1++)
            {
                if (!dataGridView1[0, Data_Grid_View_1].Selected)
                {
                    continue;
                }

                try
                {
                    string Transaction_ID = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, Data_Grid_View_1].Value.ToString();

                    string Order_Qty = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[9, Data_Grid_View_1].Value.ToString();


                    double Order_TP = double.Parse(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[13, Data_Grid_View_1].Value.ToString());

                    double Order_SL = double.Parse(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[14, Data_Grid_View_1].Value.ToString());


                    double Order_TP_PL = double.Parse(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[15, Data_Grid_View_1].Value.ToString());

                    double Order_SL_PL = double.Parse(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[16, Data_Grid_View_1].Value.ToString());

                    double Current_PL = double.Parse(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[20, Data_Grid_View_1].Value.ToString());

                    string Exchange_Name = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[2, Data_Grid_View_1].Value.ToString();

                    string Order_Symbol = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[3, Data_Grid_View_1].Value.ToString();

                    string Inst_Name = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[4, Data_Grid_View_1].Value.ToString();

                    string Order_Type = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[5, Data_Grid_View_1].Value.ToString();

                    string Prod_Type = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[6, Data_Grid_View_1].Value.ToString();


                    string Order_Buy_Sell = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[12, Data_Grid_View_1].Value.ToString();

                    string Chart_Id = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[1, Data_Grid_View_1].Value.ToString();


                    double Market_Price = Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id);


                    string Order_Expiraton_Date = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[8, Data_Grid_View_1].Value.ToString();


                    bool Order_Closed = Nest_Trader_Class_Instance.API_Order_Close(Exchange_Name, Inst_Name, Order_Symbol, Order_Qty, Order_Buy_Sell, Order_Expiraton_Date , Chart_Id , Order_Type , Prod_Type , Order_Qty , Market_Price.ToString() , Order_TP.ToString(), Order_SL.ToString(), Order_TP_PL.ToString(), Order_SL_PL.ToString(), Current_PL.ToString());



                    Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Order No : " + Transaction_ID + " chart : " + Chart_Id + " exchange : " + Exchange_Name + " symbol : " + Order_Symbol + " " + Order_Buy_Sell + "" + " closed on @ " + Market_Price + " due to user delete");

                    if (Order_Closed)
                    {
                        dataGridView1.Rows.RemoveAt(Data_Grid_View_1);

                        Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close_Buffer += Chart_Id + "," + Transaction_ID + "," + Order_Buy_Sell + "," + DateTime.Now.ToString() + "," + Market_Price + "\r\n";
                    }
                }

                catch (System.InvalidOperationException f)
                {
                    break;
                }

                dataGridView1.Update();

                if (Windows_Network_Class_Instance.Features_Array[0] == "False")
                {
                    break;
                }

                Data_Grid_View_1 = 0;
            }

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        private void Export_To_Csv_History_Click(object sender, EventArgs e)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            SFD.Filter = "(*.csv)|";

            SFD.ShowDialog();

            if (SFD.FileName == "" || SFD.FileName == null)
            {
                return;
            }

            string[] Get_All_Info = new string[dataGridView4.RowCount];


            for (int Get_Row_Cnt = 0; Get_Row_Cnt < dataGridView4.RowCount; Get_Row_Cnt++)
            {
                for (int Get_Column_Cnt = 0; Get_Column_Cnt < dataGridView4.ColumnCount; Get_Column_Cnt++)
                {
                    Get_All_Info[Get_Row_Cnt] += Return_String_Null_On_Null(dataGridView4.Rows[Get_Row_Cnt].Cells[Get_Column_Cnt].Value) + ",";
                }

                Get_All_Info[Get_Row_Cnt] = Return_String_Null_On_Null(Get_All_Info[Get_Row_Cnt].Substring(0, Get_All_Info[Get_Row_Cnt].Length - 1));
            }

            File.WriteAllLines(SFD.FileName + ".csv", Get_All_Info);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------

        public string Get_Safe_String(object Get_Obj , bool Double_Req = false)
        {
            if(Get_Obj == null)
            {
                if(Double_Req)
                {
                    return "0";
                }

                return "";
            }

            if (Double_Req)
            {
                if (Get_Obj.ToString() == null || Get_Obj.ToString() == "")
                {
                    return "0";
                }
            }

            return Get_Obj.ToString();
        }

        private void Square_Of_All_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            if (Windows_Network_Class_Instance.Features_Array[5] == "False")
            {
                Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Square off all not allowed on this account !");


                Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

                Heat_Form();

                return;
            }
            for (int tryCount = 0; tryCount < 5; tryCount++)
            {
                if (dataGridView1.Rows.Count <= 0)
                    break;
                for (int Data_Grid_View_1 = 0; Data_Grid_View_1 < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.Count; Data_Grid_View_1++)
                {
                    bool Order_Closed = false;
                    try
                    {

                        string Transaction_ID = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, Data_Grid_View_1].Value);

                        if (Transaction_ID == null || Transaction_ID == "")
                        {
                            continue;
                        }

                        string Order_Qty = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[9, Data_Grid_View_1].Value);


                        double Order_TP = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[13, Data_Grid_View_1].Value, true));

                        double Order_SL = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[14, Data_Grid_View_1].Value, true));


                        double Order_TP_PL = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[15, Data_Grid_View_1].Value, true));

                        double Order_SL_PL = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[16, Data_Grid_View_1].Value, true));

                        double Current_PL = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[20, Data_Grid_View_1].Value, true));

                        string Exchange_Name = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[2, Data_Grid_View_1].Value);

                        string Order_Symbol = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[3, Data_Grid_View_1].Value);

                        string Inst_Name = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[4, Data_Grid_View_1].Value);

                        string Order_Type = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[5, Data_Grid_View_1].Value);

                        string Prod_Type = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[6, Data_Grid_View_1].Value);


                        string Order_Buy_Sell = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[12, Data_Grid_View_1].Value);

                        string Chart_Id = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[1, Data_Grid_View_1].Value);


                        double Market_Price = Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id);


                        string Order_Expiraton_Date = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[8, Data_Grid_View_1].Value);


                        Order_Closed = Nest_Trader_Form.Nest_Trader_Class_Instance.API_Order_Close(Exchange_Name, Inst_Name, Order_Symbol, Order_Qty, Order_Buy_Sell, Order_Expiraton_Date, Chart_Id, Order_Type, Prod_Type, Order_Qty, Market_Price.ToString(), Order_TP.ToString(), Order_SL.ToString(), Order_TP_PL.ToString(), Order_SL_PL.ToString(), Current_PL.ToString());


                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Order No : " + Transaction_ID + " chart : " + Chart_Id + " exchange : " + Exchange_Name + " symbol : " + Order_Symbol + " " + Order_Buy_Sell + "" + " closed on @ " + Market_Price + " due to user square off");


                        if (Order_Closed)
                        {
                            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close_Buffer += Chart_Id + "," + Transaction_ID + "," + Order_Buy_Sell + "," + DateTime.Now.ToString() + "," + Market_Price + "\r\n";

                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();
                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.RemoveAt(Data_Grid_View_1);
                            Data_Grid_View_1 = -1;
                        }
                    }

                    catch (System.NullReferenceException f)
                    {

                    }
                }

            }

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        public void Start_Auto_Trade_Changer()
        {
            if (Start_Auto_Trade_Btn.Text.Contains("START AUTO TRADE"))
            {
                if (Windows_Network_Class_Instance.Features_Array[0] == "False")
                {
                    Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Auto trade not allowed on this account !");

                    Start_Auto_Trade_Btn.Text = "START SEMI TRADE";

                    Auto_Trade_Box.Text = "Manually";
                }

                else
                {
                    Start_Auto_Trade_Btn.Text = "STOP AUTO TRADE";

                    Auto_Trade_Box.Text = "Auto";
                }

                return;
            }

            if (Start_Auto_Trade_Btn.Text.Contains("STOP AUTO TRADE"))
            {
                Start_Auto_Trade_Btn.Text = "START SEMI TRADE";

                Auto_Trade_Box.Text = "Manually";

                return;
            }

            if (Start_Auto_Trade_Btn.Text.Contains("START SEMI TRADE"))
            {
                if (Windows_Network_Class_Instance.Features_Array[1] == "False")
                {
                    Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Semi trade not allowed on this account !");

                    Start_Auto_Trade_Btn.Text = "START SEMI TRADE";

                    Auto_Trade_Box.Text = "Manually";
                }

                else
                {
                    Start_Auto_Trade_Btn.Text = "START AUTO TRADE";

                    Auto_Trade_Box.Text = "Semi Auto";
                }

                return;
            }
        }

        private void Start_Auto_Trade_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }


            Trade_Box_Mode_Already_Invoked = true;


            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            Start_Auto_Trade_Changer();

            while (Trade_Box_Mode_Already_Invoked)
            {
                Application.DoEvents();

                Thread.Sleep(50);
            }

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        bool Trade_Box_Mode_Already_Invoked = false;

        private void Auto_Trade_Box_SelectedValueChanged(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (Trade_Box_Mode_Already_Invoked)
            {
                Trade_Box_Mode_Already_Invoked = false;

                Heat_Form();

                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            if (Auto_Trade_Box.Text.Contains("Auto") && Auto_Trade_Box.Text.Length == 4)
            {
                if (Windows_Network_Class_Instance.Features_Array[0] == "False")
                {
                    Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Auto trade not allowed on this account !");

                    Trade_Box_Mode_Already_Invoked = true;

                    Auto_Trade_Box.Text = "Manually";

                    Start_Auto_Trade_Btn.Text = "START SEMI TRADE";
                }

                else
                {
                    Start_Auto_Trade_Btn.Text = "STOP AUTO TRADE";
                }

                Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

                Heat_Form();

                return;
            }

            if (Auto_Trade_Box.Text.Contains("Manually") && Auto_Trade_Box.Text.Length == 8)
            {
                Start_Auto_Trade_Btn.Text = "START SEMI TRADE";

                Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

                Heat_Form();

                return;
            }

            if (Auto_Trade_Box.Text.Contains("Semi Auto") && Auto_Trade_Box.Text.Length == 9)
            {
                if (Windows_Network_Class_Instance.Features_Array[1] == "False")
                {
                    Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Auto trade not allowed on this account !");

                    Trade_Box_Mode_Already_Invoked = true;

                    Auto_Trade_Box.Text = "Manually";

                    Start_Auto_Trade_Btn.Text = "START SEMI TRADE";
                }

                else
                {
                    Start_Auto_Trade_Btn.Text = "START AUTO TRADE";
                }

                Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

                Heat_Form();

                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------s

        private void Symbol_Settings_Delete_Selected_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            for (int Data_Grid_View_3 = 0; Data_Grid_View_3 < dataGridView3.Rows.Count; Data_Grid_View_3++)
            {
                if (!dataGridView3[0, Data_Grid_View_3].Selected)
                {
                    continue;
                }

                try
                {
                    dataGridView3.Rows.RemoveAt(Data_Grid_View_3);
                }

                catch (System.InvalidOperationException f)
                {
                    continue;
                }

                dataGridView3.Update();


                break;
            }

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        private void Symbol_Settings_Export_Symbols_Btn_Click(object sender, EventArgs e)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            SFD.FileName = "";

            SFD.Filter = "(*.csv)|";

            SFD.ShowDialog();

            if (SFD.FileName == "" || SFD.FileName == null)
            {
                return;
            }

            Export_Nest_Trader_Symbol_Settings(SFD.FileName);
        }

        private void Symbol_Settings_Import_Symbols_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            OFD.FileName = "";

            OFD.Filter = "(*.csv)|";

            OFD.ShowDialog();

            if (OFD.FileName == "" || OFD.FileName == null)
            {
                return;
            }

            Import_Nest_Trader_Symbol_Settings(OFD.FileName);

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        public void Import_Nest_Trader_Symbol_Settings(string File_Path)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (!File.Exists(File_Path))
            {
                return;
            }

            string[] DGV_3_Array = File.ReadAllLines(File_Path);

            string[] DGV_3_Temp;

            while (dataGridView3.RowCount > 1)
            {
                dataGridView3.Rows.RemoveAt(0);

                dataGridView3.Update();
            }

            for (int DGV_3_Row_Cnt = 0; DGV_3_Row_Cnt < DGV_3_Array.Length; DGV_3_Row_Cnt++)
            {
                DGV_3_Temp = DGV_3_Array[DGV_3_Row_Cnt].Split(',');

                if (DGV_3_Row_Cnt == DGV_3_Array.Length - 1)
                {
                    try
                    {
                        Entry_Ban_Picker_1.Value = DateTime.Parse(DGV_3_Temp[0]);

                        Entry_Ban_Picker_2.Value = DateTime.Parse(DGV_3_Temp[1]);

                        Square_Off_Picker_1.Value = DateTime.Parse(DGV_3_Temp[2]);

                        Square_Off_Picker_2.Value = DateTime.Parse(DGV_3_Temp[3]);

                        Order_Type_Combo_Box.Text = DGV_3_Temp[4];

                        Product_Type_Combo_Box.Text = DGV_3_Temp[5];

                        Order_Validity_Combo_Box.Text = DGV_3_Temp[6];
                    }

                    catch (System.InvalidOperationException f)
                    {
                        break;
                    }

                    catch (System.NullReferenceException g)
                    {
                        break;
                    }

                    catch (Exception h)
                    {
                        break;
                    }

                    break;
                }

                if (DGV_3_Temp[0] == null || DGV_3_Temp[0] == "")
                {
                    continue;
                }

                dataGridView3.Rows.Add();

                for (int DGV_3_Column_Cnt = 0; DGV_3_Column_Cnt < DGV_3_Temp.Length; DGV_3_Column_Cnt++)
                {
                    try
                    {
                        dataGridView3[DGV_3_Column_Cnt, DGV_3_Row_Cnt].Value = DGV_3_Temp[DGV_3_Column_Cnt];
                    }

                    catch (System.InvalidOperationException f)
                    {
                        break;
                    }

                    catch (System.NullReferenceException g)
                    {
                        break;
                    }

                    catch (Exception h)
                    {
                        break;
                    }
                }
            }

        }

        public void Export_Nest_Trader_Symbol_Settings(string File_Path)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            string[] DGV_3_Array = new string[dataGridView3.RowCount + 1];

            for (int DGV_3_Row_Cnt = 0; DGV_3_Row_Cnt < DGV_3_Array.Length - 1; DGV_3_Row_Cnt++)
            {
                for (int DGV_3_Column_Cnt = 0; DGV_3_Column_Cnt < dataGridView3.ColumnCount; DGV_3_Column_Cnt++)
                {

                    if (DGV_3_Column_Cnt == dataGridView3.ColumnCount - 1)
                    {
                        DGV_3_Array[DGV_3_Row_Cnt] += Return_String_Null_On_Null(dataGridView3[DGV_3_Column_Cnt, DGV_3_Row_Cnt].Value);

                        break;
                    }

                    DGV_3_Array[DGV_3_Row_Cnt] += Return_String_Null_On_Null(dataGridView3[DGV_3_Column_Cnt, DGV_3_Row_Cnt].Value) + ",";
                }
            }


            DGV_3_Array[dataGridView3.RowCount] = Entry_Ban_Picker_1.Value.ToString() + "," + Entry_Ban_Picker_2.Value.ToString() + "," + Square_Off_Picker_1.Value.ToString() + "," +
                                                  Square_Off_Picker_2.Value.ToString() + "," + Order_Type_Combo_Box.Text + "," + Product_Type_Combo_Box.Text + "," + Order_Validity_Combo_Box.Text;

            if (File_Path == "" || File_Path == null)
            {
                return;
            }

            string Directory_Path = Path.GetDirectoryName(File_Path);

            if (!Directory.Exists(Directory_Path))
            {
                Directory.CreateDirectory(Directory_Path);
            }


            try
            {
                File.Delete(File_Path);

                File.WriteAllLines(File_Path, DGV_3_Array);
            }

            catch (InvalidOperationException f)
            {
 
            }
        }

        private void Symbol_Settings_Save_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            Export_Nest_Trader_Symbol_Settings(Nest_Trader_Class_Instance.Nest_Trader_Path + "Symbol Nest Saver.csv");

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------

        OpenFileDialog OFD = new OpenFileDialog();

        SaveFileDialog SFD = new SaveFileDialog();


        private void Load_Settings_Btn_Click(object sender, EventArgs e)
        {
            Freeze_Form();


            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Form_Thread_Class_Instance.Nest_Trader_Stop_Timer();

            OFD.FileName = "";

            OFD.ShowDialog();

            OFD.Filter = "(*.csv)|";

            if (OFD.FileName == "" || OFD.FileName == null)
            {
                return;
            }

            Load_File_Class(OFD.FileName);

            Form_Thread_Class_Instance.Nest_Trader_Start_Timer();

            Heat_Form();
        }

        public void Load_File_Class(string File_Path , int File_Class_Loader = 0)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (!File.Exists(File_Path))
            {
                return;
            }

            string[] Nest_Trader_Settings = File.ReadAllText(File_Path).Split(',');

            if (Nest_Trader_Settings.Length < 11)
            {
                return;
            }

            if (File_Class_Loader == 1)
            {
                try
                {
                    Auto_Trade_Box.Text = Nest_Trader_Settings[3];

                    Start_Auto_Trade_Changer();

                    return;
                }

                catch (Exception)
                {
                    return;
                }
            }

            if (File_Class_Loader == 2)
            {
                try
                {
                    Login_Name_Text_Box.Text = Nest_Trader_Settings[0];

                    Login_Pass_Text_Box.Text = Nest_Trader_Settings[1];

                    Login_Trans_Text_Box.Text = Nest_Trader_Settings[2];

                    Semi_Auto_Clear_Time_Box.Text = Nest_Trader_Settings[4];

                    Retry_Failed_Box.Text = Nest_Trader_Settings[5];

                    if (Nest_Trader_Settings[6] == "True")
                    {
                        Audio_Alert_Allowed.Checked = true;
                    }

                    if (Nest_Trader_Settings[7] == "True")
                    {
                        Pop_UP_Alert.Checked = true;
                    }

                    Name_Box.Text = Nest_Trader_Settings[8];

                    Phone_Box.Text = Nest_Trader_Settings[9];

                    Email_Box.Text = Nest_Trader_Settings[10];

                    return;
                }

                catch (Exception)
                {
                    return;
                }
            }

            try
            {
                Login_Name_Text_Box.Text = Nest_Trader_Settings[0];

                Login_Pass_Text_Box.Text = Nest_Trader_Settings[1];

                Login_Trans_Text_Box.Text = Nest_Trader_Settings[2];


                Auto_Trade_Box.Text = Nest_Trader_Settings[3];


                //Auto_Trade_Box.SelectedValue = Nest_Trader_Settings[3];

                Semi_Auto_Clear_Time_Box.Text = Nest_Trader_Settings[4];

                Retry_Failed_Box.Text = Nest_Trader_Settings[5];

                if (Nest_Trader_Settings[6] == "True")
                {
                    Audio_Alert_Allowed.Checked = true;
                }

                if (Nest_Trader_Settings[7] == "True")
                {
                    Pop_UP_Alert.Checked = true;
                }

                Name_Box.Text = Nest_Trader_Settings[8];

                Phone_Box.Text = Nest_Trader_Settings[9];

                Email_Box.Text = Nest_Trader_Settings[10];
            }

            catch (Exception)
            {
                File.Move(File_Path, File_Path + ".Old");

                File.Delete(File_Path);
            }


            Start_Auto_Trade_Changer();
        }

        private void Save_Btn_Click(object sender, EventArgs e)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }


            string Directory_Path = Path.GetDirectoryName(Nest_Trader_Class_Instance.Nest_Trader_Path);

            if (!Directory.Exists(Nest_Trader_Class_Instance.Nest_Trader_Path))
            {
                Directory.CreateDirectory(Nest_Trader_Class_Instance.Nest_Trader_Path);
            }

            try
            {
                if (File.Exists(Nest_Trader_Class_Instance.Nest_Trader_Path + "File Nest Trader.csv"))
                {
                    File.Delete(Nest_Trader_Class_Instance.Nest_Trader_Path + "File Nest Trader.csv");
                }

                File.WriteAllText(Nest_Trader_Class_Instance.Nest_Trader_Path + "File Nest Trader.csv", Login_Name_Text_Box.Text + "," + Login_Pass_Text_Box.Text + "," + Login_Trans_Text_Box.Text + "," + Auto_Trade_Box.Text + "," +
                                                                 Semi_Auto_Clear_Time_Box.Text + "," + Retry_Failed_Box.Text + "," + Audio_Alert_Allowed.Checked + "," +
                                                                 Pop_UP_Alert.Checked + "," + Name_Box.Text + "," + Phone_Box.Text + "," +
                                                                 Email_Box.Text);
            }

            catch (InvalidOperationException f)
            {

            }

            SFD.FileName = "";

            SFD.Filter = "(*.csv)|";

            SFD.ShowDialog();

            if (SFD.FileName == "" || SFD.FileName == null)
            {
                return;
            }

            try
            {
                File.WriteAllText(SFD.FileName, Login_Name_Text_Box.Text + "," + Login_Pass_Text_Box.Text + "," + Login_Trans_Text_Box.Text + "," + Auto_Trade_Box.Text + "," +
                                                                 Semi_Auto_Clear_Time_Box.Text + "," + Retry_Failed_Box.Text + "," + Audio_Alert_Allowed.Checked + "," +
                                                                 Pop_UP_Alert.Checked + "," + Name_Box.Text + "," + Phone_Box.Text + "," +
                                                                 Email_Box.Text);
            }

            catch (InvalidOperationException f)
            {

            }

        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------


        public string Return_String_Null_On_Null(object Null_String)
        {
            if (Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return null;
            }

            if (Null_String == null)
            {
                return "";
            }

            try
            {
                return Null_String.ToString();
            }

            catch (Exception)
            {
                return "";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------

        /*
        public string[] Get_Trade_History()
        {
            if (Nest_Trader_Outdated)
            {
                return null;
            }

            IntPtr Nest_Trader_Order_History_Id = Get_Form_Handle_By_Process_Id(Nest_Trader_Id, "Report View Generator");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "^+R"); // ^+R


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{Down}");


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{UP}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}", 8);



            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "1990/1990/1990");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "2036/2036/2036");


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}", 6);

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{ENTER}");

            IntPtr Nest_Trader_Complete_History_Id = Get_Form_Handle_By_Process_Id(Nest_Trader_Id, "NEST - " + Login_Name_Text_Box.Text);

            while (GetForegroundWindow() != Nest_Trader_Complete_History_Id)
            {
                Thread.Sleep(10);

                Application.DoEvents();
            }

            Thread.Sleep(500);

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}", 14);

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{Down}", 2);


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "^{A}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "^{c}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{UP}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{Down}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{ESC}");

            File.WriteAllText("Trade History.hst" , Clipboard.GetText());

            return File.ReadAllLines("Trade History.hst");
        }

        public string[] Get_Order_History()
        {
            if (Nest_Trader_Outdated)
            {
                return null;
            }

            Send_Keys_To_Window_By_Handle(Nest_Trader_Id, "^+R");

            IntPtr Nest_Trader_Order_History_Id = Get_Form_Handle_By_Process_Id(Nest_Trader_Process_Id, "Report View Generator");

            while((int)Nest_Trader_Order_History_Id < 1)
            {
                Send_Keys_To_Window_By_Handle(Nest_Trader_Id, "^+R");

                Nest_Trader_Order_History_Id = Get_Form_Handle_By_Process_Id(Nest_Trader_Process_Id, "Report View Generator");

                (Nest_Trader_Order_History_Id + " , " + Nest_Trader_Process_Id + " , " + Nest_Trader_Id);

                Thread.Sleep(2 * 1000);
            }

            (2);


            for (int i = 0; i < 63; i++)
            {
                (i + " , " + Get_Windows_Title_By_Handle(Get_Child_By_Index(Nest_Trader_Order_History_Id, i)));
            }

            IntPtr Report_Type_Ptr = Get_Child_By_Index(Nest_Trader_Order_History_Id, 0);

            IntPtr Exchange_Name_Ptr = Get_Child_By_Index(Nest_Trader_Order_History_Id, 1);

            IntPtr Start_Date_Ptr = Get_Child_By_Index(Nest_Trader_Order_History_Id, 11);

            IntPtr End_Date_Ptr = Get_Child_By_Index(Nest_Trader_Order_History_Id, 12);

            IntPtr Generate_Report_Ptr = Get_Child_By_Index(Nest_Trader_Order_History_Id, 2);


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "^+R"); // ^+R


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{UP}");


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{UP}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}", 8);



            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "1990/1990/1990");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "2036/2036/2036");


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}", 6);

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{ENTER}");


            

            while(GetForegroundWindow() != Nest_Trader_Complete_History_Id)
            {
                Thread.Sleep(10);

                Application.DoEvents();
            }

            Thread.Sleep(500);

            Send_Keys_To_Window_By_Handle(Nest_Trader_Complete_History_Id, "{ENTER}");


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{ENTER}", 1);

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{TAB}", 14);

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{Down}", 2);


            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "^{A}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "^{c}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{UP}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{Down}");

            Send_Keys_To_Window_By_Handle(Nest_Trader_Order_History_Id, "{ESC}");


            File.WriteAllText("Order History.hst", Clipboard.GetText());

            return File.ReadAllLines("Order History.hst");
        }
        */

        private void Web_Site_Link_Label_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Web_Site_Link_Label.Enabled = false;

            this.Update();

            Process.Start("IExplore.exe", Web_Site_Link_Label.Text);

            Web_Site_Link_Label.Enabled = true;

            this.Update();
        }

        private void Facebook_Link_Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Web_Site_Link_Label.Enabled = false;

            this.Update();

            Process.Start("IExplore.exe", "https://www.facebook.com/MyTrend-Software-Nse-Mcx-212469248777221");

            Web_Site_Link_Label.Enabled = true;

            this.Update();
        }

        private void Phone_Link_Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void Clear_Log_Btn_Click(object sender, EventArgs e)
        {
            Message_Log_Buffer = "";

            Status_Page_Tab.Controls.Clear();

            Form_Interactive_Class_Instance.Staus_Message_Lines_Cnt = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------
    }
}