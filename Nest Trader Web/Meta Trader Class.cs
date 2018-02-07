using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nest_Trader_Web
{
    public class Meta_Trader_Class
    {

        public string API_MQL_Symbol_Close_Buffer = "";

        /*
         * 2) API MT4 Notify Problem
        */

        public void API_MQL_Symbol_Notify()
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Action New_Action = (new Action(() =>
            {
                try
                {
                    for (int Chart_Loader_Cnt = 1; Chart_Loader_Cnt <= Chart_Loader_Last_Cnt + 1; Chart_Loader_Cnt++)
                    {
                        if (!File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " File Path.txt"))
                        {
                            continue;
                        }

                        string Api_MQL_File_Path_Text = File.ReadAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " File Path.txt");

                        if (Api_MQL_File_Path_Text.Contains("\r\n"))
                        {
                            Api_MQL_File_Path_Text = Api_MQL_File_Path_Text.Remove(Api_MQL_File_Path_Text.Length - 2);
                        }

                        try
                        {
                            File.WriteAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " File log 1.txt" , Api_MQL_File_Path_Text+ "OM Orders " + Chart_Loader_Cnt + ".txt");
                        }

                        catch(Exception)
                        {

                        }

                        if (File.Exists(Api_MQL_File_Path_Text + "OM Orders " + Chart_Loader_Cnt + ".txt"))
                        {
                            continue;
                        }


                        int[] DGV_Searcher = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3, 1, Chart_Loader_Cnt.ToString());

                        if (DGV_Searcher.Length == 0 || DGV_Searcher == null)
                        {
                            return;
                        }

                        else
                        {
                            string File_Write_Buffer = "";


                            for (int DGV_1_Row_Cnt = 0; DGV_1_Row_Cnt < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1.RowCount; DGV_1_Row_Cnt++)
                            {
                                try
                                {
                                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[1, DGV_1_Row_Cnt].Value == null)
                                    {
                                        continue;
                                    }

                                    string Chart_ID = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[1, DGV_1_Row_Cnt].Value.ToString();

                                    if (Chart_Loader_Cnt.ToString() != Chart_ID)
                                    {
                                        continue;
                                    }

                                    string Order_Ticket = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[0, DGV_1_Row_Cnt].Value.ToString();

                                    string Order_Buy_Sell = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[12, DGV_1_Row_Cnt].Value.ToString();

                                    string Order_Qty = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[19, DGV_1_Row_Cnt].Value.ToString();

                                    string Order_TP = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[13, DGV_1_Row_Cnt].Value.ToString();

                                    string Order_SL = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[14, DGV_1_Row_Cnt].Value.ToString();

                                    string Order_Open_Time = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[18, DGV_1_Row_Cnt].Value.ToString();

                                    string Order_Open_Price = Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView1[10, DGV_1_Row_Cnt].Value.ToString();


                                    File_Write_Buffer += Order_Ticket + "," + Order_Buy_Sell + "," + Order_Qty + "," + Order_TP + "," + Order_SL + "," + Order_Open_Time + "," + Order_Open_Price + "\r\n";
                                }

                                catch (Exception)
                                {

                                }
                            }

                            if (File_Write_Buffer != "")
                            {
                                try
                                {
                                    File_Write_Buffer = File_Write_Buffer.Remove(File_Write_Buffer.Length - 2);

                                    File.WriteAllText(Api_MQL_File_Path_Text + "OM Orders " + Chart_Loader_Cnt + ".txt", File_Write_Buffer);
                                }

                                catch (Exception)
                                {

                                }
                            }
                        }
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

        public void API_MQL_Symbol_Close()
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            string[] API_MQL_Symbol_Close_Buffer_Array = null;

            try
            {
                if(API_MQL_Symbol_Close_Buffer != null && API_MQL_Symbol_Close_Buffer != "")
                {
                    File.WriteAllText("C:\\Logger LV 1.txt", Path.GetPathRoot(Environment.SystemDirectory) + "API MQL Temp Close Array.txt : " + API_MQL_Symbol_Close_Buffer);
                }

                File.WriteAllText(Path.GetPathRoot(Environment.SystemDirectory) + "API MQL Temp Close Array.txt", API_MQL_Symbol_Close_Buffer);

                API_MQL_Symbol_Close_Buffer_Array = File.ReadAllLines(Path.GetPathRoot(Environment.SystemDirectory) + "API MQL Temp Close Array.txt");

                API_MQL_Symbol_Close_Buffer = "";
            }

            catch (Exception)
            {
                return;
            }

            Action New_Action = (new Action(() =>
            {
                try
                {
                    for (int Chart_Loader_Cnt = 1; Chart_Loader_Cnt <= Chart_Loader_Last_Cnt + 1; Chart_Loader_Cnt++)
                    {
                        if (!File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " File Path.txt"))
                        {
                            continue;
                        }

                        string Api_MQL_File_Path_Text = File.ReadAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " File Path.txt");

                        if (Api_MQL_File_Path_Text.Contains("\r\n"))
                        {
                            Api_MQL_File_Path_Text = Api_MQL_File_Path_Text.Remove(Api_MQL_File_Path_Text.Length - 2);
                        }

                        try
                        {
                            File.WriteAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " File log 2.txt", Api_MQL_File_Path_Text + "Close Orders " + Chart_Loader_Cnt + ".txt");
                        }

                        catch(Exception)
                        {

                        }

                        if (File.Exists(Api_MQL_File_Path_Text + "Close Orders " + Chart_Loader_Cnt + ".txt") || API_MQL_Symbol_Close_Buffer_Array.Length == 0)
                        {
                            continue;
                        }

                        string API_Mql_Local_Close_Buffer = "";

                        for (int Local_Close_Buffer_Cnt = 0; Local_Close_Buffer_Cnt < API_MQL_Symbol_Close_Buffer_Array.Length; Local_Close_Buffer_Cnt++)
                        {
                            if (API_MQL_Symbol_Close_Buffer_Array[Local_Close_Buffer_Cnt].Split(',')[0] == Chart_Loader_Cnt.ToString())
                            {
                                API_Mql_Local_Close_Buffer += API_MQL_Symbol_Close_Buffer_Array[Local_Close_Buffer_Cnt] + "\r\n";
                            }
                        }

                        if (API_Mql_Local_Close_Buffer != "")
                        {
                            API_Mql_Local_Close_Buffer = API_Mql_Local_Close_Buffer.Remove(API_Mql_Local_Close_Buffer.Length - 2);

                            File.WriteAllText("C:\\Logger LV 2.txt" , Api_MQL_File_Path_Text + "Close Orders " + Chart_Loader_Cnt + ".txt : " + API_Mql_Local_Close_Buffer);


                            int Tries_Cnt = 0;

                            while (Tries_Cnt < 20)
                            {
                                try
                                {
                                    File.WriteAllText(Api_MQL_File_Path_Text + "Close Orders " + Chart_Loader_Cnt + ".txt", API_Mql_Local_Close_Buffer);
                                }

                                catch (Exception)
                                {
                                    
                                }

                                Tries_Cnt++;

                                Thread.Sleep(500);
                            }
                        }
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

        public void API_MQL_Symbol_Init()
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Action New_Action = (new Action(() =>
            {
                try
                {
                    for (int Chart_Loader_Cnt = 1; Chart_Loader_Cnt <= Chart_Loader_Last_Cnt + 1; Chart_Loader_Cnt++)
                    {

                        if(Symbol_Prices.Length != Chart_Loader_Last_Cnt + 1)
                        {
                            Array.Resize(ref Symbol_Prices , Chart_Loader_Last_Cnt + 1);
                        }

                        if (!File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " Init.txt"))
                        {
                            continue;
                        }

                        Chart_Loader_Last_Cnt = Math.Max(Chart_Loader_Cnt, Chart_Loader_Last_Cnt);

                        string[] Api_MQL_Symbol_Text = File.ReadAllLines(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " Init.txt");

                        File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Loader_Cnt + " Init.txt");

                        int[] DGV_Searcher = Nest_Trader_Form.Nest_Trader_Form_Instance.DGV_Column_Searcher(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3, 1, Chart_Loader_Cnt.ToString());

                        if (DGV_Searcher.Length == 0 || DGV_Searcher == null)
                        {
                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3.Rows.Add(Api_MQL_Symbol_Text[0], Chart_Loader_Cnt);

                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3.Update();
                        }

                        else
                        {
                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3.Rows[DGV_Searcher[0]].Cells[0].Value = Api_MQL_Symbol_Text[0];

                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3.Rows[DGV_Searcher[0]].Cells[1].Value = Chart_Loader_Cnt;

                            Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3.Update();
                        }
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


        double[] Symbol_Prices = new double[0];

        int[] Price_Deletes = new int[0];

        public double Get_Symbol_Price(string Chart_Id)
        {
            try
            {
                if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
                {
                    return 0;
                }

                if (Symbol_Prices.Length - 1 < int.Parse(Chart_Id))
                {
                    Array.Resize(ref Symbol_Prices, int.Parse(Chart_Id) + 1);
                }

                if (Price_Deletes.Length - 1 < int.Parse(Chart_Id))
                {
                    Array.Resize(ref Price_Deletes, int.Parse(Chart_Id) + 1);
                }

                if (!File.Exists(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Price.txt"))
                {
                    Console.WriteLine(DateTime.Now.ToString() + " : 1 : " + Symbol_Prices[int.Parse(Chart_Id)]);

                    return Symbol_Prices[int.Parse(Chart_Id)];
                }

                //if (Price_Deletes[int.Parse(Chart_Id)] < 3)
                //{
                //    Price_Deletes[int.Parse(Chart_Id)]++;

                //    File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Price.txt");

                //    return 0;
                //}

                Price_Deletes[int.Parse(Chart_Id)] = 0;

                double Symbol_Price = 0;

                try
                {

                    string Text_File = File.ReadAllText(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Price.txt");


                    int New_Line = Text_File.IndexOf('\r');

                    if (New_Line > -1)
                    {
                        Text_File = Text_File.Substring(0, New_Line);
                    }

                    Symbol_Price = double.Parse(Text_File);

                    Console.WriteLine(DateTime.Now.ToString() + " : 2 : " + Symbol_Price);

                    File.Delete(Nest_Trader_Form.Nest_Trader_Class_Instance.Nest_Trader_Path + Chart_Id + " Price.txt");

                    Symbol_Prices[int.Parse(Chart_Id)] = Symbol_Price;
                }

                catch (Exception A)
                {
                    
                }

                Console.WriteLine(DateTime.Now.ToString() + " : 3 : " + Symbol_Price);


                return Symbol_Prices[int.Parse(Chart_Id)];
            }

            catch (Exception A)
            {
                return 0;
            }
        }

        public int Chart_Loader_Last_Cnt = 1;

        public void Api_Order_Writer_Updater()
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            Action New_Action = (new Action(() =>
            {
                try
                {
                    for (int DGV_Cnt_3 = 0; DGV_Cnt_3 < Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3.RowCount; DGV_Cnt_3++)
                    {
                        string Chart_Id = Nest_Trader_Form.Nest_Trader_Form_Instance.Return_String_Null_On_Null(Nest_Trader_Form.Nest_Trader_Form_Instance.dataGridView3[1, DGV_Cnt_3].Value);

                        if (Chart_Id == "")
                        {
                            continue;
                        }

                        Nest_Trader_Form.Form_Thread_Class_Instance.Api_Order_Writer_Requests(Chart_Id);
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
