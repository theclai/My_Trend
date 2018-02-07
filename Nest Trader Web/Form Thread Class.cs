using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Akka.Actor;
using Nest_Trader_Web.NestActorSystem.Messages;

namespace Nest_Trader_Web
{

    public class Form_Thread_Class
    {
        public bool Get_Trade_Archive = false; // deter. if we already get account history - once per nest trader restart / first start , if true - we dont get again account history 

        public bool API_User = false; // deter. if form event raised , when we interact with the form , we set it as true , the timer event stop to raise functions . makes our interact sync. with the timer to prevent problem . 

        public bool Nest_Trader_Stopped = false; // deter. if the timer stopped to raise functions after API_User set to true - if yes , set as true . we wait to this var. before we continue to start function in our raised
                                                 //form event . makes our interact sync. with the timer to prevent problem . 

        public System.Timers.Timer Nest_Trader_Timer = new System.Timers.Timer(30); // Our timer

        public System.Timers.Timer Nest_Trader_PB_Timer = new System.Timers.Timer(30); // Our PB timer

        public System.Timers.Timer Nest_Trader_Exit_Timer = new System.Timers.Timer(30); // Our PB timer


        int Signal_Finder_PB_Register = 1;

        public DateTime Last_Time = DateTime.Now.AddMinutes(-3);

        public void Nest_Trader_Exit_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Nest_Trader_Exit_Timer.Stop();

            try
            {

                if (Nest_Trader_Form.Nest_Trader_Form_Instance.Auto_Trade_Box.Text == "Auto")
                {
                    if ((DateTime.Now - Last_Time).TotalSeconds > 5)
                    {
                        for (int Chart_Loader_Cnt = 1; Chart_Loader_Cnt <= Nest_Trader_Form.Meta_Trader_Class_Instance.Chart_Loader_Last_Cnt + 1; Chart_Loader_Cnt++)
                        {
                            if (File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " Requests.txt"))
                            {
                                Last_Time = DateTime.Now;

                                Nest_Trader_Form.Windows_Keyboard_Class_Instance.Restore_Nest_Trader_Window();

                                Thread.Sleep(300);

                                Windows_Api_Class.SetForegroundWindow(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id);

                                break;
                            }
                        }
                    }
                }

                if (Nest_Trader_Form.Nest_Trader_Form_Instance.Form_Closed)
                {
                    On_Form_Close_Event(true);
                }

                Nest_Trader_Exit_Timer.Start();
            }
            catch (Exception)
            {

            }
        }

        public void Nest_Trader_PB_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Nest_Trader_PB_Timer.Stop();

            Action New_Action = (new Action(() =>
        {
            Nest_Trader_Form.Nest_Trader_Form_Instance.Trade_Mode_Label.Text = Nest_Trader_Form.Nest_Trader_Form_Instance.Auto_Trade_Box.Text;

            Nest_Trader_Form.Nest_Trader_Form_Instance.Signal_Finder_PB.Value = Signal_Finder_PB_Register;

            Nest_Trader_Form.Nest_Trader_Form_Instance.Signal_Finder_PB.Value = Signal_Finder_PB_Register - 1;

            Signal_Finder_PB_Register++;

            Application.DoEvents();

            if (Signal_Finder_PB_Register == 101)
            {
                Signal_Finder_PB_Register = 1;

                Nest_Trader_Form.Nest_Trader_Form_Instance.Update();

                Thread.Sleep(30);
            }

        }));
        
            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
            }

            else
            {
                New_Action();
            }

            Nest_Trader_PB_Timer.Start();
        }

        public void Nest_Trader_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Nest_Trader_Timer.Stop(); // stops our timer to raise our timer event in multiple threads . we want to run with single thread . to make our functions safe from confuse .

            if (API_User) // if form event raised
            {
                Nest_Trader_Stopped = true; // set as true . to notify that we already get the message here , and the raised event is safe from confuse .

                return; // dont raise our event . when the form raised event finish is function , it will allow our timer to be raise .
            }

            while (!Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated && !Nest_Trader_Form.Windows_Network_Class_Instance.Server_Connected)
            {
                Application.DoEvents();

                Thread.Sleep(100);
            }

            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated) // if our exp. date arrived or we blocked stop our timer raise .
            {
                return;
            }

            GC.Collect(); // clean our prev. gen. data to free memory .

            if (!Nest_Trader_Form.Windows_Network_Class_Instance.Server_Connected) // if we are still not connected to the server we wait to connection
            {
                Thread.Sleep(3 * 1000);

                Nest_Trader_Timer.Start();

                return;
            }

            Days_Trade_Move_To_Trade_Archives();


            IntPtr Nest_Trader_Process_Id_Temp = Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id; // we get our nest proccess id from the previous cycle

            /*
             * 
            if (!Nest_Trader_Process_Run(Nest_Trader_Process_Id))
            {
                Get_Trade_Archive = false;
            }

            */



            Nest_Trader_Form.Nest_Trader_Form_Instance.Initialize_Process_Id("NestTrader"); // get our current process id


            if (Nest_Trader_Process_Id_Temp != Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id) // if in the previous cycle we had the proceess id , and in this cycle it changed
                                                                                                                  // nest trader closed / changed
            {
                Stack_Stop_Timer_Requests = 0;

                Stack_Start_Timer_Requests = 0;


                Get_Trade_Archive = false; // We need to get the account history to get data
            }

            //"Process ID : " + Nest_Trader_Id + " , Active ID : " + Get_Active_Window() + " , Active Windows : " + Get_Windows_Title_By_Handle(Get_Active_Window()));

            if ((int)Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id < 1 || (int)Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id < 1)
            {
                Nest_Trader_Timer.Start();

                return;
            }

            if (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "NEST Trader Investor-Login")))
            {
                Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Connect(Nest_Trader_Form.Nest_Trader_Form_Instance.Login_Name_Text_Box.Text, Nest_Trader_Form.Nest_Trader_Form_Instance.Login_Pass_Text_Box.Text);

                Thread.Sleep(5 * 1000);


                Nest_Trader_Timer.Start();

                return;
            }

            if ((int)Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id < 1)
            {
                Action New_Action = (new Action(() =>
                {
                   //  Nest_Trader_Form.Nest_Trader_Form_Instance.NEST_Status_Label.Text = "Wait for nest trader !";
                }));

                if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
                {
                    Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
                }

                else
                {
                    New_Action();
                }

                Nest_Trader_Timer.Start();

                return;
            }

            if (Nest_Trader_Form.Windows_Api_Class_Instance.Window_Visible_Flag(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Id))
            {
                if (!Get_Trade_Archive)
                {

                    IntPtr Nest_Trader_Basket_Id = Nest_Trader_Form.Windows_Handler_Clas_Instances.Get_Form_Handle_By_Process_Id(Nest_Trader_Form.Nest_Trader_Form_Instance.Nest_Trader_Process_Id, "NEST Trader Basket");

                    //Trade_Archives_Tab_Updater();

                    Nest_Trader_Form.Nest_Trader_Class_Instance.Get_Current_Detailed_Orders(true);

                    Thread.Sleep(2 * 1000);

                    Get_Trade_Archive = true;


                    Action New_Action = (new Action(() =>
                    {
                        // Nest_Trader_Form.Nest_Trader_Form_Instance.NEST_Status_Label.Text = "Scanning for signal !";
                    }));

                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
                    {
                        Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
                    }

                    else
                    {
                        New_Action();
                    }
                }
            }


            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Notify();

            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close();


            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Init();

            Nest_Trader_Form.Meta_Trader_Class_Instance.Api_Order_Writer_Updater();

            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Notify();

            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close();

            //Open_Positions_Checker();

            Open_Positions_Tab_Updater();

            Nest_Trader_Timer.Start();
        } // our timer event

        public void On_Form_Close_Event(bool Close_Form = false)
        {

            Nest_Trader_Form.Nest_Trader_Form_Instance.Days_Trades_Export_To_CSV();

            Nest_Trader_Form.Nest_Trader_Form_Instance.Save_Trade_Archives();

            Nest_Trader_Form.Nest_Trader_Form_Instance.Save_Log_Messages();


            string[] DGV_String_Array = new string[Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.RowCount];


            for (int Row_Cnt = 0; Row_Cnt < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.RowCount; Row_Cnt++)
            {
                for (int Column_Cnt = 0; Column_Cnt < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.ColumnCount; Column_Cnt++)
                {
                    try
                    {
                        DGV_String_Array[Row_Cnt] += Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[Column_Cnt, Row_Cnt].Value.ToString() + ",";

                        if (Row_Cnt == Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.ColumnCount - 1)
                        {
                            DGV_String_Array[Row_Cnt] = DGV_String_Array[Row_Cnt].Remove(Row_Cnt);
                        }
                    }

                    catch (System.NullReferenceException)
                    {

                    }

                    catch (System.ArgumentOutOfRangeException)
                    {

                    }
                }
            }

            try
            {
                if (DGV_String_Array == null || DGV_String_Array.Length == 0)
                {
                    File.WriteAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + "Trades DB.csv", "");
                }

                else
                {
                    File.WriteAllLines(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + "Trades DB.csv", DGV_String_Array);
                }
            }

            catch (Exception)
            {

            }

            if (Close_Form)
            {
                try
                {
                    Application.Exit();
                }

                catch (Exception)
                {

                }

                try
                {
                    Process.GetCurrentProcess().Kill();
                }

                catch (Exception)
                {

                }

                try
                {
                    Environment.FailFast("");
                }

                catch (Exception)
                {

                }
            }
        }

        public void Nest_Trader_Stop_Form()
        {
            Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated = true;

            Nest_Trader_Timer.Elapsed -= Nest_Trader_Timer_Elapsed;

            Nest_Trader_Timer.Enabled = false;

            Nest_Trader_Timer.Stop();

            Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("User has been disconnected !");
        }  // if we authenticated with our server and our exp. date arrived or we blocked by the server , then we stop our app to respond . 


        int Stack_Stop_Timer_Requests = 0;

        int Stack_Start_Timer_Requests = 0;

        public void Nest_Trader_Stop_Timer()
        {
            if (Stack_Stop_Timer_Requests - Stack_Start_Timer_Requests == 0)
            {
                Stack_Stop_Timer_Requests = 0;

                Stack_Start_Timer_Requests = 0;
            }

            Stack_Stop_Timer_Requests++;

            int Stack_Timer_Local = Stack_Stop_Timer_Requests;

            while (Stack_Timer_Local - Stack_Start_Timer_Requests != 1)
            {
                Application.DoEvents();

                Thread.Sleep(10);
            }


            while (API_User)
            {
                Application.DoEvents();

                Thread.Sleep(10);
            }

            API_User = true;

            while (!Nest_Trader_Stopped)
            {
                Application.DoEvents();

                Thread.Sleep(10);
            }
        }
        // stops and wait to our timer event , by API_User & Nest_Trader_Stopped flags

        public void Nest_Trader_Start_Timer()
        {
            Stack_Start_Timer_Requests++;

            Nest_Trader_Stopped = false;

            API_User = false;

            Nest_Trader_Timer.Start();
        } // starts our timer event , by API_User & Nest_Trader_Stopped flags

        public string Get_Safe_String(object Get_Obj, bool Double_Req = false)
        {
            if (Get_Obj == null)
            {
                if (Double_Req)
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

        private void Days_Trade_Move_To_Trade_Archives()
        {
            Action New_Action = (new Action(() =>
            {
                try
                {

                    string[] Get_All_Info = new string[Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.RowCount];


                    for (int Get_Row_Cnt = 0; Get_Row_Cnt < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.RowCount; Get_Row_Cnt++)
                    {
                        if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[16, Get_Row_Cnt].Value == null || Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[16, Get_Row_Cnt].Value )== "")
                        {
                            continue;
                        }

                        DateTime DGV_2_DT = DateTime.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[17, Get_Row_Cnt].Value));

                        if (DGV_2_DT > DateTime.Now.Date)
                        {
                            continue;
                        }

                        if (Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.Rows[Get_Row_Cnt].Cells[16].Value) != "Closed")
                        {
                            continue;
                        }

                        string Exchange_Name = Get_Safe_String (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[1, Get_Row_Cnt].Value);

                        string Order_Qty = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[7, Get_Row_Cnt].Value);

                        string Order_Id = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[0, Get_Row_Cnt].Value);

                        string Buy_Sell_Int = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[10, Get_Row_Cnt].Value);

                        string Order_Symbol = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[2, Get_Row_Cnt].Value);

                        string Inst_Name = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[3, Get_Row_Cnt].Value);

                        string Expiration_Date = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[6, Get_Row_Cnt].Value);

                        string Order_Price = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[9, Get_Row_Cnt].Value);

                        string Order_Type = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[4, Get_Row_Cnt].Value);

                        string Prod_Type = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2[5, Get_Row_Cnt].Value);

                        string Status_Desc = "Close";


                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView4.Rows.Add(Exchange_Name, Order_Qty, Order_Id, Buy_Sell_Int, Order_Symbol, Inst_Name, "", Expiration_Date, Order_Price, "", Order_Type, Prod_Type, Status_Desc);


                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.Rows.RemoveAt(Get_Row_Cnt);


                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.Update();

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView4.Update();


                        Get_Row_Cnt = 0;
                    }
                }

                catch (Exception A)
                {
                    
                }
            }));

            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
            }

            else
            {
                New_Action();
            }
        }
        private void Check_Oposite()
        {
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

                    for (int i = 0; i < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.Count; i++)
                    {
                        if((Order_Symbol == Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[3, i].Value)
                            && (Order_Buy_Sell == "Buy" && Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[12, i].Value) == "Sell")
                            && (String.CompareOrdinal(Transaction_ID, Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, i].Value)) > 0)) ||

                            (Order_Symbol == Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[3, i].Value)
                            && (Order_Buy_Sell == "Sell" && Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[12, i].Value) == "Buy")
                            && (String.CompareOrdinal(Transaction_ID, Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, i].Value)) > 0))
                            )
                        {
                                string Transaction_ID_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, i].Value);

                                if (Transaction_ID_r == null || Transaction_ID_r == "")
                                {
                                    continue;
                                }

                                string Order_Qty_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[9, i].Value);
                                
                                double Order_TP_r = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[13, i].Value, true));

                                double Order_SL_r = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[14, i].Value, true));


                                double Order_TP_PL_r = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[15, i].Value, true));

                                double Order_SL_PL_r = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[16, i].Value, true));

                                double Current_PL_r = double.Parse(Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[20, i].Value, true));

                                string Exchange_Name_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[2, i].Value);

                                string Order_Symbol_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[3, i].Value);

                                string Inst_Name_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[4, i].Value);

                                string Order_Type_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[5, i].Value);

                                string Prod_Type_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[6, i].Value);


                                string Order_Buy_Sell_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[12, i].Value);

                                string Chart_Id_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[1, i].Value);


                                double Market_Price_r = Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id);


                                string Order_Expiraton_Date_r = Get_Safe_String(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[8, i].Value);
                                Order_Closed = Nest_Trader_Form.Nest_Trader_Class_Instance.API_Order_Close(Exchange_Name_r, Inst_Name_r, Order_Symbol_r, Order_Qty_r, Order_Buy_Sell_r, Order_Expiraton_Date_r, Chart_Id_r, Order_Type_r, Prod_Type_r, Order_Qty_r, Market_Price_r.ToString(), Order_TP_r.ToString(), Order_SL_r.ToString(), Order_TP_PL_r.ToString(), Order_SL_PL_r.ToString(), Current_PL_r.ToString());

                                if (Order_Closed)
                                {
                                    Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar(" Order No : " + Transaction_ID_r + " chart : " + Chart_Id_r + " exchange : " + Exchange_Name_r + " symbol : " + Order_Symbol_r + " " + Order_Buy_Sell_r + "" + " closed on @ " + Market_Price_r + " due to oposite signal");
                                    Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close_Buffer += Chart_Id_r + "," + Transaction_ID_r + "," + Order_Buy_Sell_r + "," + DateTime.Now.ToString() + "," + Market_Price_r + "\r\n";
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.RemoveAt(i);
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();
                                    Order_Closed = false;
                                    Data_Grid_View_1 = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.Count;
                                    break;
                                }
                                else
                                    Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Order No : " + Transaction_ID_r + " chart : " + Chart_Id_r + " exchange : " + Exchange_Name_r + " symbol : " + Order_Symbol_r + " " + Order_Buy_Sell_r + "" + " failed to closed on @ " + Market_Price_r + " due to oposite signal");
                        }
                    }
                }

                catch (System.NullReferenceException f)
                {

                }       
            }    
        }
        private void Square_Of_All()
        {

            for (int tryCount = 0; tryCount < 5; tryCount++)
            {
                if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.Count <= 0)
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
                        
                        if (Order_Closed)
                        {
                            Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar(" Order No : " + Transaction_ID + " chart : " + Chart_Id + " exchange : " + Exchange_Name + " symbol : " + Order_Symbol + " " + Order_Buy_Sell + "" + " closed on @ " + Market_Price + " due to time square off");
                            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close_Buffer += Chart_Id + "," + Transaction_ID + "," + Order_Buy_Sell + "," + DateTime.Now.ToString() + "," + Market_Price + "\r\n";
                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.RemoveAt(Data_Grid_View_1);
                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();
                            Data_Grid_View_1 = -1;
                        }
                        else
                            Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Order No : " + Transaction_ID + " chart : " + Chart_Id + " exchange : " + Exchange_Name + " symbol : " + Order_Symbol + " " + Order_Buy_Sell + "" + " failed to closed on @ " + Market_Price + " due to time square off");    
                    }

                    catch (System.NullReferenceException f)
                    {

                    }
                }

            }
        }
        public void Open_Positions_Checker()
        {

            for (int tryCount = 0; tryCount < 5; tryCount++)
            {
                if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.Count <= 0)
                    break;
                for (int Data_Grid_View_1 = 0; Data_Grid_View_1 < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.Count; Data_Grid_View_1++)
                {
                    bool Order_Closed = false;
                    try
                    {

                        string Transaction_ID =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, Data_Grid_View_1].Value);

                        if (Transaction_ID == null || Transaction_ID == "")
                        {
                            continue;
                        }

                        string Order_Qty =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[9, Data_Grid_View_1].Value);


                        double Order_TP =
                            double.Parse(
                                Get_Safe_String(
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[13, Data_Grid_View_1].Value,
                                    true));

                        double Order_SL =
                            double.Parse(
                                Get_Safe_String(
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[14, Data_Grid_View_1].Value,
                                    true));


                        double Order_TP_PL =
                            double.Parse(
                                Get_Safe_String(
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[15, Data_Grid_View_1].Value,
                                    true));

                        double Order_SL_PL =
                            double.Parse(
                                Get_Safe_String(
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[16, Data_Grid_View_1].Value,
                                    true));

                        double Current_PL =
                            double.Parse(
                                Get_Safe_String(
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[20, Data_Grid_View_1].Value,
                                    true));

                        string Exchange_Name =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[2, Data_Grid_View_1].Value);

                        string Order_Symbol =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[3, Data_Grid_View_1].Value);

                        string Inst_Name =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[4, Data_Grid_View_1].Value);

                        string Order_Type =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[5, Data_Grid_View_1].Value);

                        string Prod_Type =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[6, Data_Grid_View_1].Value);


                        string Order_Buy_Sell =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[12, Data_Grid_View_1].Value);

                        string Chart_Id =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[1, Data_Grid_View_1].Value);


                        double Market_Price = Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id);

                        if (Market_Price <= 0)
                        {
                            continue;
                        }

                        string Order_Expiraton_Date =
                            Get_Safe_String(
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[8, Data_Grid_View_1].Value);


                        if (Current_PL < Order_TP_PL && Current_PL > Order_SL_PL)
                        {
                            if (Order_Buy_Sell == "Buy")
                            {
                                if (Market_Price < Order_TP && Market_Price > Order_SL)
                                {
                                    continue;
                                }
                            }

                            if (Order_Buy_Sell == "Sell")
                            {
                                if (Market_Price > Order_TP && Market_Price < Order_SL)
                                {
                                    continue;
                                }
                            }
                        }

                        if (Market_Price >= Order_TP || Market_Price <= Order_SL)
                        {
                            Order_Closed = Nest_Trader_Form.Nest_Trader_Class_Instance.API_Order_Close(Exchange_Name,
                                Inst_Name, Order_Symbol, Order_Qty, Order_Buy_Sell, Order_Expiraton_Date, Chart_Id,
                                Order_Type, Prod_Type, Order_Qty, Market_Price.ToString(), Order_TP.ToString(),
                                Order_SL.ToString(), Order_TP_PL.ToString(), Order_SL_PL.ToString(),
                                Current_PL.ToString());
                            if (Order_Closed)
                            {
                                //Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar(" Order No : " + Transaction_ID + " chart : " + Chart_Id + " exchange : " + ExchangeName + " symbol : " + OrderSymbol + " " + OrderBuySell + "" + " closed on @ " + Market_Price + " due to TP/SL reached");
                                string Nest_Trader_Message = "";
                                if (Current_PL >= Order_TP_PL)
                                {
                                    Nest_Trader_Message = "PL TP of : " + Current_PL + " while the configured was : " +
                                                          Order_TP_PL;
                                }

                                if (Current_PL <= Order_SL_PL)
                                {
                                    Nest_Trader_Message = "PL SL of : " + Current_PL + " while the configured was : " +
                                                          Order_SL_PL;
                                }

                                if (Current_PL < Order_TP_PL && Current_PL > Order_SL_PL)
                                {
                                    if (Order_Buy_Sell == "Buy")
                                    {
                                        if (Market_Price >= Order_TP)
                                        {
                                            Nest_Trader_Message = "Market TP of while the configured was : " + Order_TP;
                                        }

                                        if (Market_Price <= Order_SL)
                                        {
                                            Nest_Trader_Message = "Market SL of while the configured was : " + Order_SL;
                                        }
                                    }

                                    if (Order_Buy_Sell == "Sell")
                                    {
                                        if (Market_Price <= Order_TP)
                                        {
                                            Nest_Trader_Message = "Market TP of while the configured was : " + Order_TP;
                                        }

                                        if (Market_Price >= Order_SL)
                                        {
                                            Nest_Trader_Message = "Market SL of while the configured was : " + Order_SL;
                                        }
                                    }
                                }
                                Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close_Buffer += Chart_Id +
                                                                                                           "," +
                                                                                                           Transaction_ID +
                                                                                                           "," +
                                                                                                           Order_Buy_Sell +
                                                                                                           "," +
                                                                                                           DateTime.Now
                                                                                                               .ToString
                                                                                                               () + "," +
                                                                                                           Market_Price +
                                                                                                           "\r\n";
                                Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar(
                                    "Order No : " + Transaction_ID + " chart : " + Chart_Id + " exchange : " +
                                    Exchange_Name + " symbol : " + Order_Symbol + " " + Order_Buy_Sell + "" +
                                    " closed on @ " + Market_Price + " due to " + Nest_Trader_Message);

                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.RemoveAt(Data_Grid_View_1);
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();
                                Data_Grid_View_1 = -1;
                            }
                            else
                                Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar(
                                    " Order No : " + Transaction_ID + " chart : " + Chart_Id + " exchange : " +
                                    Exchange_Name + " symbol : " + Order_Symbol + " " + Order_Buy_Sell + "" +
                                    " failed to closed on @ " + Market_Price + " due to TP/SL reached");
                        }
                    }
                    catch (System.NullReferenceException f)
                    {

                    }
                }

            }

        }

        public void Open_Positions_Tab_Updater(string Sended_Tranaction_ID = "")
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (Sended_Tranaction_ID == "0" || Sended_Tranaction_ID == "-1" || Sended_Tranaction_ID == null)
            {
                return;
            }

            try
            {

                string[] Get_Current_Orders_Array = Nest_Trader_Form.Nest_Trader_Class_Instance.Get_Current_Detailed_Orders();


                if (Get_Current_Orders_Array != null && Get_Current_Orders_Array.Length != 0)
                {
                    for (int Get_Current_Orders_Cnt = 0; Get_Current_Orders_Cnt < Get_Current_Orders_Array.Length; Get_Current_Orders_Cnt++)
                    {
                        string[] Info_Split = Get_Current_Orders_Array[Get_Current_Orders_Cnt].Split('\t');
                        if (Info_Split.Length == 1)
                        {
                            continue;
                        }

                        string Order_Ticket = Info_Split[11];

                        if (Sended_Tranaction_ID != "" && Sended_Tranaction_ID != Order_Ticket)
                        {
                            continue;
                        }

                        string Order_Segment = Info_Split[0];

                        string Order_Symbol = Info_Split[13];

                        string Order_Inst_Name = Info_Split[14];

                        string Order_Type = Info_Split[23];

                        string Prod_Type = Info_Split[24];

                        string Order_Strike_Price = Info_Split[15];

                        string Order_Expiry = Info_Split[16];

                        string Order_QTY = Info_Split[5];

                        string Order_Price = Info_Split[4];

                        string Order_Status = Info_Split[17];

                        string Order_Buy_Sell = Info_Split[2];

                        if (Order_Buy_Sell == "BUY" || Order_Buy_Sell == "Buy")
                        {
                            Order_Buy_Sell = "Buy";
                        }

                        else
                        {
                            Order_Buy_Sell = "Sell";
                        }

                        string Client_Id = Info_Split[1];

                        string Order_Date_Time = Info_Split[18] + " " + Info_Split[19];

                        string Order_Open_QTY = Info_Split[5];

                        string Order_Current_PL = "";

                        string Order_Today_PL = "";


                       // double Symbol_Price = Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Client_Id);


                        try
                        {
                            if(Info_Split.Length > 45)
                            Order_Current_PL = Info_Split[45];
                            if (Info_Split.Length > 46)
                                Order_Today_PL = Info_Split[46];
                        }

                        catch (Exception)
                        {

                        }


                        //Action New_Action_Loop = (new Action(() =>
                        //{
                            try
                            {

                                int[] DGV_Indexes = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1, 0, Order_Ticket);

                                if (DGV_Indexes.Length > 0)
                                {
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Invoke((MethodInvoker)
                                        delegate
                                        {
                                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]
                                                ].Cells[9].Value = Order_QTY;
                                            // Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]].Cells[10].Value = Symbol_Price;
                                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]
                                                ].Cells[11].Value = Order_Status;
                                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[DGV_Indexes[0]
                                                ].Cells[18].Value = Order_Date_Time;

                                            if (Order_Today_PL != "" && Order_Today_PL != null)
                                            {
                                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[
                                                    DGV_Indexes[0]].Cells[20].Value = Order_Current_PL;
                                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows[
                                                    DGV_Indexes[0]].Cells[21].Value = Order_Today_PL;
                                            }
                                        });

                                }
                       
                                else if (Sended_Tranaction_ID != "")
                                {
                                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Invoke((MethodInvoker)
                                        delegate
                                        {
                                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.Add(
                                                Order_Ticket, "", Order_Segment, Order_Symbol, Order_Inst_Name,
                                                Order_Type, Prod_Type, Order_Strike_Price
                                                , Order_Expiry, Order_QTY, "", Order_Status, Order_Buy_Sell
                                                , "", "", "", "", Client_Id, Order_Date_Time, Order_Open_QTY,
                                                Order_Current_PL, Order_Today_PL);
                                        });
                                    Get_Current_Orders_Cnt = 0;
                                }
                                Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Invoke((MethodInvoker)
                                    delegate
                                    {
                                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();
                                    });
                            }

                            catch (Exception A)
                            {
                                
                            }
                        //}));

                        //if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
                        //{
                        //    Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action_Loop);
                        //}

                        //else
                        //{
                        //    New_Action_Loop();
                        //}

                        //Thread.Sleep(100);
                    }
                }
            }

            catch (Exception A)
            {
                
            }


            return;

            /*

            Action New_Action = (new Action(() =>
            {
                for (int DGV_Row_1_Cnt = 0; DGV_Row_1_Cnt < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.RowCount; DGV_Row_1_Cnt++)
                {

                    string Order_Ticket = Nest_Trader_Form.Nest_Trader_Form_Instance.Return_String_Null_On_Null(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, DGV_Row_1_Cnt].Value);


                    if (Order_Ticket == "" || Order_Ticket == null)
                    {
                        continue;
                    }

                    bool Order_Closed = true;

                    for (int DGV_Ticket_1_Cnt = 0; DGV_Ticket_1_Cnt < Get_Current_Orders_Array.Length; DGV_Ticket_1_Cnt++)
                    {
                        if (Get_Current_Orders_Array[DGV_Ticket_1_Cnt].Split('\t')[11] == Order_Ticket)
                        {
                            Order_Closed = false;

                            break;
                        }
                    }

                    if (Order_Closed)
                    {
                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView4.Rows.Add(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[2, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[9, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[15, DGV_Row_1_Cnt].Value
                            , Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[3, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[4, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[7, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[8, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[13, DGV_Row_1_Cnt].Value,
                           Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[11, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[5, DGV_Row_1_Cnt].Value, Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[6, DGV_Row_1_Cnt].Value, "Closed");

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.RemoveAt(DGV_Row_1_Cnt);

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView4.Update();

                        Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();

                        DGV_Row_1_Cnt = 0;
                    }
                }
            }));

            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
            }

            else
            {
                New_Action();
            }

            */
        }

        /*
        public void Trade_Archives_Tab_Updater()
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Action New_Action = (new Action(() =>
            {
                string[] History_Lines_Array = Nest_Trader_Form.Nest_Trader_Class_Instance.Get_All_Account_History();

                if (History_Lines_Array == null || History_Lines_Array.Length == 0)
                {
                    return;
                }

                for (int All_HST_Cnt = 0; All_HST_Cnt < History_Lines_Array.Length; All_HST_Cnt++)
                {
                    string[] Order_History_Array = History_Lines_Array[All_HST_Cnt].Split('\t');

                    int[] DGV_Indexes = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView4, 2, Order_History_Array[1]);

                    if (DGV_Indexes.Length > 0)
                    {
                        continue;
                    }

                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView4.Rows.Add(Order_History_Array[0], Order_History_Array[12], Order_History_Array[1], Order_History_Array[4], Order_History_Array[5], Order_History_Array[6], Order_History_Array[7],
                                           Order_History_Array[8], Order_History_Array[10], Order_History_Array[17], Order_History_Array[19], Order_History_Array[20], Order_History_Array[22]);

                    Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView4.Update();
                }
            }));

            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
            }

            else
            {
                New_Action();
            }
        }

        */
        public void Api_Order_Writer_Requests(string Chart_Id)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

             try
            {
                if (
                    File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id +
                                " Requests.txt"))
                {
                    string Api_Order_Writer1_Array =
                        File.ReadAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id +
                                         " Requests.txt");

                    File.WriteAllText(
                        Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " logger Requests.txt",
                        DateTime.Now + " , " + Api_Order_Writer1_Array);
                }
            }

            catch (Exception)
            {

            }

            Action New_Action = (new Action(() =>
            {
                try
                {

                    Open_Positions_Checker();
                    Check_Oposite();

                    string Entry_Ban_Time_1 = Nest_Trader_Form.Nest_Trader_Form_Instance.Entry_Ban_Picker_1.Value.ToString("HH:mm"); // MCX

                    string Entry_Ban_Time_2 = Nest_Trader_Form.Nest_Trader_Form_Instance.Entry_Ban_Picker_2.Value.ToString("HH:mm");// Nse


                    string Square_Off_Time_1 = Nest_Trader_Form.Nest_Trader_Form_Instance.Square_Off_Picker_1.Value.ToString("HH:mm"); // MCX

                    string Square_Off_Time_2 = Nest_Trader_Form.Nest_Trader_Form_Instance.Square_Off_Picker_2.Value.ToString("HH:mm");// Nse


                    string Win_Time = DateTime.Now.ToString("HH:mm");

                    //dataGridView3[2, DGV_Column_Searcher(dataGridView3, 1, Chart_Id)[0]].Value.ToString());

                    int[] DGV_Chart_Id_Rows = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3, 1, Chart_Id);

                    if (DGV_Chart_Id_Rows == null || DGV_Chart_Id_Rows.Length == 0)
                    {
                        return;
                    }

                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[2, DGV_Chart_Id_Rows[0]].Value == null)
                    {
                        return;
                    }

                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[2, DGV_Chart_Id_Rows[0]].Value.ToString() == "MCX")
                    {
                        if (DateTime.ParseExact(Win_Time, "HH:mm", null) <= DateTime.ParseExact(Entry_Ban_Time_1, "HH:mm", null))
                        {
                            try
                            {
                                File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Requests.txt");
                            }

                            catch(Exception)
                            {

                            }

                            return;
                        }

                        if (DateTime.ParseExact(Win_Time, "HH:mm", null) >= DateTime.ParseExact(Square_Off_Time_1, "HH:mm", null))
                        {
                            try
                            {
                                File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Requests.txt");
                            }

                            catch (Exception)
                            {

                            }
                            Square_Of_All();
                            //Opposite_Signal_Checker(Chart_Id, "Buy");

                           // Opposite_Signal_Checker(Chart_Id, "Sell");

                            return;
                        }
                    }


                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[2, DGV_Chart_Id_Rows[0]].Value.ToString() == "NSE")
                    {
                        if (DateTime.ParseExact(Win_Time, "HH:mm", null) <= DateTime.ParseExact(Entry_Ban_Time_2, "HH:mm", null))
                        {
                            try
                            {
                                File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Requests.txt");
                            }

                            catch (Exception)
                            {

                            }

                            return;
                        }

                        if (DateTime.ParseExact(Win_Time, "HH:mm", null) >= DateTime.ParseExact(Square_Off_Time_2, "HH:mm", null))
                        {
                            try
                            {
                                File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Requests.txt");
                            }

                            catch (Exception)
                            {

                            }
                            Square_Of_All();
                            //Opposite_Signal_Checker(Chart_Id, "Buy");

                            //Opposite_Signal_Checker(Chart_Id, "Sell");

                            return;
                        }
                    }

                    if (!File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Requests.txt"))
                    {
                        return;
                    }

                    try
                    {
                        string[] Api_Order_Writer_Array = File.ReadAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Requests.txt").Split(',');

                        int New_Line = Api_Order_Writer_Array[1].IndexOf('\r');

                        if (New_Line > -1)
                        {
                            Api_Order_Writer_Array[1] = Api_Order_Writer_Array[1].Substring(0, New_Line);
                        }

                        if (Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id) == 0)
                        {
                            return;
                        }

                        //API_Order_Write(double.Parse(Api_Order_Writer_Array[0]), Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id), Chart_Id, double.Parse(Api_Order_Writer_Array[1]));
                  //  for (int i = 0; i < 5; i++)
                        {
                            API_Order_Write(double.Parse(Api_Order_Writer_Array[0]),
                                Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id), Chart_Id, 1);
                        }
                        File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Requests.txt");
                    }

                    catch (Exception A)
                    {

                    }
                }

                catch (Exception A)
                {
                    
                }
            }));

            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
            }

            else
            {
                New_Action();
            }
        }

        public bool Opposite_Signal_Checker(string Chart_Id, string Order_Buy_Sell)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return false;
            }

            bool Returned_Value = false;

            Action New_Action = (new Action(() =>
            {
                try
                {

                    int[] DGV_Rows_ID = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1, 1, Chart_Id);

                    if (DGV_Rows_ID == null || DGV_Rows_ID.Length == 0)
                    {
                        return;
                    }

                    if (Order_Buy_Sell == "Buy")
                    {
                        Order_Buy_Sell = "Sell";
                    }

                    else
                    {
                        Order_Buy_Sell = "Buy";
                    }

                    DGV_Rows_ID = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Rows_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1, DGV_Rows_ID, 12, Order_Buy_Sell);

                    int[] DGV_Rows_Exp_Date_3 = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3, 1, Chart_Id);

                    string Order_Expiraton_Date = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[5, DGV_Rows_Exp_Date_3[0]].Value.ToString();


                    DGV_Rows_ID = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Rows_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1, DGV_Rows_ID, 8, Order_Expiraton_Date);


                    if (DGV_Rows_ID == null || DGV_Rows_ID.Length == 0)
                    {
                        return;
                    }


                    for (int Data_Grid_View_1 = 0; Data_Grid_View_1 < DGV_Rows_ID.Length; Data_Grid_View_1++)
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


                        double Market_Price = Nest_Trader_Form.Meta_Trader_Class_Instance.Get_Symbol_Price(Chart_Id);



                        if (Nest_Trader_Form.Nest_Trader_Class_Instance.API_Order_Close(Exchange_Name, Inst_Name, Order_Symbol, Order_Qty, Order_Buy_Sell, Order_Expiraton_Date, Chart_Id, Order_Type, Prod_Type, Order_Qty, Market_Price.ToString(), Order_TP.ToString(), Order_SL.ToString(), Order_TP_PL.ToString(), Order_SL_PL.ToString(), Current_PL.ToString()))
                        {
                            int[] DGV_Rows_ID_Local = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Rows_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1, DGV_Rows_ID, 12, Order_Buy_Sell);


                            if (DGV_Rows_ID_Local.Length == 0 || DGV_Rows_ID_Local == null)
                            {
                                break;
                            }

                            Nest_Trader_Form.Meta_Trader_Class_Instance.API_MQL_Symbol_Close_Buffer += Chart_Id + "," + Transaction_ID + "," + Order_Buy_Sell + "," + DateTime.Now.ToString() + "," + Market_Price + "\r\n";


                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Rows.RemoveAt(DGV_Rows_ID_Local[0]);

                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.Update();


                            Data_Grid_View_1 = 0;
                        }

                        Returned_Value = true;
                    }

                }

                catch (Exception A)
                {
                    
                }
            }));

            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
            }

            else
            {
                New_Action();
            }

            return Returned_Value;
        }

        public void API_Order_Write(double Order_Buy_Sell_Int, double Order_Price, string Chart_Id, double Digit_Point)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Action New_Action = (new Action(async () =>
            {
                try
                {

                    string Order_Buy_Sell = "Sell";

                    if (Order_Buy_Sell_Int / 2 == (int)(Order_Buy_Sell_Int / 2))
                    {
                        Order_Buy_Sell = "Buy";
                    }

                    //dataGridView2.Rows[dataGridView2.RowCount - 1].Cells[2].Value = OrderType;

                    string[] Data_Grid_3_Data = new string[17];

                    for (int Data_Grid_3_Cnt = 0; Data_Grid_3_Cnt < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3.RowCount; Data_Grid_3_Cnt++)
                    {
                        if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[1, Data_Grid_3_Cnt].Value == null)
                        {
                            continue;
                        }

                        try
                        {

                            Data_Grid_3_Data[0] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[1, Data_Grid_3_Cnt].Value.ToString();


                            if (Data_Grid_3_Data[0] != Chart_Id.ToString())
                            {
                                continue;
                            }

                            Data_Grid_3_Data[1] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[2, Data_Grid_3_Cnt].Value.ToString(); // Exchange

                            Data_Grid_3_Data[2] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[3, Data_Grid_3_Cnt].Value.ToString(); // Symbol

                            Data_Grid_3_Data[3] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[4, Data_Grid_3_Cnt].Value.ToString(); // Inst. Name


                            /*
                            if (Order_Buy_Sell_Int > 1)
                            {
                                Data_Grid_3_Data[4] = "LIMIT"; // Order Type
                            }

                            else
                            {
                                Data_Grid_3_Data[4] = "MARKET"; // Order Type
                            }

                            */

                            Data_Grid_3_Data[4] = Nest_Trader_Form.Nest_Trader_Form_Instance.Order_Type_Combo_Box.SelectedItem.ToString();

                            Data_Grid_3_Data[5] = Nest_Trader_Form.Nest_Trader_Form_Instance.Product_Type_Combo_Box.SelectedItem.ToString(); // Prod Type

                            Data_Grid_3_Data[6] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[5, Data_Grid_3_Cnt].Value.ToString(); // Expiry

                            Data_Grid_3_Data[7] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[7, Data_Grid_3_Cnt].Value.ToString(); // TP

                            Data_Grid_3_Data[8] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[8, Data_Grid_3_Cnt].Value.ToString(); // SL

                            Data_Grid_3_Data[9] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[9, Data_Grid_3_Cnt].Value.ToString(); // TP PL

                            Data_Grid_3_Data[10] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[10, Data_Grid_3_Cnt].Value.ToString(); // SL PL

                            Data_Grid_3_Data[11] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[11, Data_Grid_3_Cnt].Value.ToString(); // Order QTY

                            Data_Grid_3_Data[12] = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[12, Data_Grid_3_Cnt].Value.ToString(); // Disc QTY


                            double Take_Profit;

                            double Stop_Loss;


                            string TP_SL_Points_Str = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[6, Data_Grid_3_Cnt].Value.ToString();


                            bool TP_SL_Points = true;


                            if (TP_SL_Points_Str == "%")
                            {
                                TP_SL_Points = false;
                            }


                            if (!TP_SL_Points)
                            {
                                if (Order_Buy_Sell == "Buy")
                                {
                                    Take_Profit = (1 + double.Parse(Data_Grid_3_Data[7]) / 100) * Order_Price;
                                    Stop_Loss = (1 - double.Parse(Data_Grid_3_Data[8]) / 100) * Order_Price;
                                }

                                else
                                {
                                    Take_Profit = (1 - double.Parse(Data_Grid_3_Data[7]) / 100) * Order_Price;
                                    Stop_Loss = (1 + double.Parse(Data_Grid_3_Data[8]) / 100) * Order_Price;
                                }
                            }

                            else
                            {
                                if (Order_Buy_Sell == "Buy")
                                {
                                    Take_Profit = Order_Price + double.Parse(Data_Grid_3_Data[7]) * Digit_Point;
                                    Stop_Loss = Order_Price - double.Parse(Data_Grid_3_Data[8]) * Digit_Point;
                                }

                                else
                                {
                                    Take_Profit = Order_Price - double.Parse(Data_Grid_3_Data[7]) * Digit_Point;
                                    Stop_Loss = Order_Price + double.Parse(Data_Grid_3_Data[8]) * Digit_Point;
                                }
                            }


                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView2.Rows.Add(Chart_Id, Data_Grid_3_Data[1], Data_Grid_3_Data[2], Data_Grid_3_Data[3], Data_Grid_3_Data[4], Data_Grid_3_Data[5], Data_Grid_3_Data[6], Data_Grid_3_Data[11],
                                                   Data_Grid_3_Data[12], Order_Price, Order_Buy_Sell, Take_Profit, Stop_Loss, Data_Grid_3_Data[9], Data_Grid_3_Data[10], "Ready");


                        }

                        catch (Exception A)
                        {
                            
                        }
                    }

                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.Auto_Trade_Box.Text == "Auto")
                    {
                        var t1 = Nest_Trader_Form.Nest_Trader_Form_Instance.DayTraderSendActorRef.Ask(new DaysTradeSendOrderMessage(true, false) ,TimeSpan.FromSeconds(360));
                        await Task.WhenAll(t1);
                        //Nest_Trader_Form.Nest_Trader_Form_Instance.Days_Trade_Send_Order(true, false);
                    }

                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.Auto_Trade_Box.Text == "Semi Auto")
                    {
                        var t1 = Nest_Trader_Form.Nest_Trader_Form_Instance.DayTraderSendActorRef.Ask(new DaysTradeSendOrderMessage(false, false), TimeSpan.FromSeconds(360));
                        await Task.WhenAll(t1);
                        //  Nest_Trader_Form.Nest_Trader_Form_Instance.Days_Trade_Send_Order(false, false);
                    }
                }

                catch (Exception A)
                {
                    
                }
            }));

            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
            }

            else
            {
                New_Action();
            }
        }
    }
}
