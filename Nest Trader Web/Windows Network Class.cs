using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nest_Trader_Web
{
    public class Windows_Network_Class
    {
        public string[] Features_Array = new string[6];

        public bool Nest_Trader_Outdated = false, Server_Connected = false, Wait_For_Server = true;

        bool Connect_To_Server_Success_Wroted = false , Connect_To_Server_Failed_Wroted = false , Server_Message_Wroted = false , User_Has_Been_Disconnected = false;

        public void Show_Server_State_N_Freeze(bool Server_Connected = false)
        {
            DateTime Date_Time_Now = DateTime.Now;

            if (Server_Connected == false)
            {
                Connect_To_Server_Success_Wroted = false;

                int Elapsed_Seconds = 0;

                while (Elapsed_Seconds <= 30)
                {
                    Action New_Action = (new Action(() =>
                    {
                        Nest_Trader_Form.Nest_Trader_Form_Instance.NEST_Status_Label.Text = "Reconnect in " + (30 - Elapsed_Seconds);
                    }));

                    if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
                    {
                        Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action);
                    }

                    else
                    {
                        New_Action();
                    }

                    Thread.Sleep(100);

                    Elapsed_Seconds = (int)(DateTime.Now - Date_Time_Now).TotalSeconds;
                }

                Action New_Action_1 = (new Action(() =>
                {
                    Nest_Trader_Form.Nest_Trader_Form_Instance.NEST_Status_Label.Text = "Connecting to server ... ";
                }));

                if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
                {
                    Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action_1);
                }

                else
                {
                    New_Action_1();
                }

                return;
            }

            else
            {
                Action New_Action = (new Action(() =>
                {
                    Nest_Trader_Form.Nest_Trader_Form_Instance.NEST_Status_Label.Text = "Connected to server !";
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

        public void Nest_Trader_Server_Thread()
        {


            Features_Array[0] = "True";
            Features_Array[1] = "True";
            Features_Array[2] = "True";
            Features_Array[3] = "True";
            Features_Array[4] = "True";
            Features_Array[5] = "True";


            Server_Connected = true;

            Action New_Action_1 = (new Action(() =>
            {
                try
                {
                    Nest_Trader_Form.Nest_Trader_Form_Instance.NEST_Status_Label.Invoke((MethodInvoker)delegate
                    {
                        Nest_Trader_Form
                            .Nest_Trader_Form_Instance
                            .NEST_Status_Label
                            .Text
                            = "";
                        // "Unlimited Version !";
                    });
                }
                catch (Exception)
                {

                }
            }));

            if (Nest_Trader_Form.Nest_Trader_Form_Instance.InvokeRequired)
            {
                Nest_Trader_Form.Nest_Trader_Form_Instance.Invoke(New_Action_1);
            }

            else
            {
                Thread.Sleep(200);
                New_Action_1();
            }


            return;



            Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Connecting to server ... !");

            while (true)
            {
                Socket Server_Socket = User_Socket_Connector_Builder("124.40.244.178", 52000); // 124.40.244.178 , 10.100.102.2

                if (Server_Socket == null)
                {
                    // fail - retry in 30 sec

                    if (!Connect_To_Server_Failed_Wroted)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Connect to server failed !");
                    }

                    Show_Server_State_N_Freeze();

                    Connect_To_Server_Success_Wroted = false; Server_Message_Wroted = false; User_Has_Been_Disconnected = false;


                    Server_Connected = false;

                    Connect_To_Server_Failed_Wroted = true;

                    continue;
                }

                NetworkInterface[] All_Network_Interfaces = NetworkInterface.GetAllNetworkInterfaces();

                int Socket_End_Point_Length;

                try
                {
                    Socket_End_Point_Length = Server_Socket.LocalEndPoint.ToString().IndexOf(':');
                }

                catch (Exception)
                {
                    // fail - retry in 30 sec

                    if (!User_Has_Been_Disconnected)
                    {
                        Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("User has been disconnected !");
                    }

                    Show_Server_State_N_Freeze();

                    User_Has_Been_Disconnected = true;

                    Server_Connected = false;

                    Connect_To_Server_Success_Wroted = false; Server_Message_Wroted = false; Connect_To_Server_Failed_Wroted = false;

                    continue;
                }

                if (!Connect_To_Server_Success_Wroted)
                {
                    Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("Connect To server succeeded !");

                    Show_Server_State_N_Freeze(true);

                    Connect_To_Server_Success_Wroted = true;

                    Server_Connected = true;

                    Server_Message_Wroted = false; Connect_To_Server_Failed_Wroted = false; User_Has_Been_Disconnected = false;
                }

                string Local_Address = Server_Socket.LocalEndPoint.ToString().Substring(0, Socket_End_Point_Length);


                string HDD_Serial_No = "";

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Win32 Settings");


                if (key == null)
                {
                    try
                    {
                        RegistryKey key1 = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Win32 Settings");

                        //storing the values  
                        key1.SetValue("Setting1", Windows_Encrypt_Class.GetHDDSerialNo().Substring(0, Math.Min(Windows_Encrypt_Class.GetHDDSerialNo().Length, 45)));

                        key1.Close();
                    }

                    catch (Exception)
                    {

                    }
                }

                else
                {
                    try
                    {
                        HDD_Serial_No = key.GetValue("Setting1").ToString();

                        key.Close();
                    }

                    catch (Exception)
                    {

                    }
                }
            

                if (HDD_Serial_No == null || HDD_Serial_No == "")
                {
                    HDD_Serial_No = Windows_Encrypt_Class.GetHDDSerialNo().Substring(0, Math.Min(Windows_Encrypt_Class.GetHDDSerialNo().Length, 45));
                }


                Socket_Text_Sender(Server_Socket, "Auth. Login," + HDD_Serial_No + "," + Nest_Trader_Form.Nest_Trader_Form_Instance.Name_Box.Text + "," + Nest_Trader_Form.Nest_Trader_Form_Instance.Email_Box.Text + "," + Nest_Trader_Form.Nest_Trader_Form_Instance.Phone_Box.Text);

                Features_Array = Socket_Text_Reader(Server_Socket).Split(',');

                //Console.WriteLine("Mac Address 0 : " + MAC_Address);

                //Console.WriteLine("Mac Address 2 : " + Decrypt_Data_Algorithm(MAC_Address));

                try
                {
                    Socket_Text_Sender(Server_Socket, "Check User Token Id," + HDD_Serial_No);


                    string Socket_Call_Back_Text_2_Step = Socket_Text_Reader(Server_Socket);

                    if (Socket_Call_Back_Text_2_Step.Contains("Expired") || Socket_Call_Back_Text_2_Step.Contains("Not Original") || Socket_Call_Back_Text_2_Step.Contains("Blocked"))
                    {
                        if (!Server_Message_Wroted)
                        {
                             Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("User has been " + '"' + Socket_Call_Back_Text_2_Step + '"' + " . " + "Contact Admin or visit our website : " + @"http://www.mytrendsoftware.com/");

                            Server_Message_Wroted = true;

                            Connect_To_Server_Success_Wroted = false; Connect_To_Server_Failed_Wroted = false; User_Has_Been_Disconnected = false;
                        }

                        Server_Connected = true;

                        Show_Server_State_N_Freeze();

                        continue;
                    }
                }

                catch (Exception)
                {
                    if (!User_Has_Been_Disconnected)
                    {
                         Nest_Trader_Form.Form_Interactive_Class_Instance.Create_Message_On_Status_Bar("User has been disconnected !");
                    }

                    Connect_To_Server_Success_Wroted = false; Server_Message_Wroted = false; Connect_To_Server_Failed_Wroted = false;

                    User_Has_Been_Disconnected = true;


                    Show_Server_State_N_Freeze();

                    Server_Connected = false;

                    continue;
                }

                try
                {
                    Server_Socket.Dispose();
                }

                catch(Exception)
                {

                }

                Show_Server_State_N_Freeze(true);

                Server_Connected = true;

                Thread.Sleep(3 * 60 * 1000); //  * 60
            }
        }
    
        //--------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Server_Socket_Listener_Builder(string Server_IP_Address, int Server_Port, int Max_Listeneres, Action<object> Thread_Call_Back_Method)
        {
            Socket Duplex_Socket_Server;

            IPEndPoint Server_End_Point = new IPEndPoint(IPAddress.Parse(Server_IP_Address), Server_Port);
            Duplex_Socket_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Duplex_Socket_Server.Bind(Server_End_Point);

            Duplex_Socket_Server.Listen(Max_Listeneres);

            while (true)
            {
                Duplex_Socket_Server = Duplex_Socket_Server.Accept();

                Thread New_Thread_Call_Back = new Thread(() => Thread_Call_Back_Method(Duplex_Socket_Server));

                New_Thread_Call_Back.Start();
            }
        }

        public Socket User_Socket_Connector_Builder(string Server_Connector_IP_Address, int Server_Connector_Port)
        {
            Socket Duplex_Socket_User;

            IPEndPoint Server_Connector_End_Point = new IPEndPoint(IPAddress.Parse(Server_Connector_IP_Address), Server_Connector_Port);
            Duplex_Socket_User = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            Duplex_Socket_User.ReceiveTimeout = 5 * 1000;
            Duplex_Socket_User.SendTimeout = 5 * 1000;

            try
            {
                Duplex_Socket_User.Connect(Server_Connector_IP_Address, Server_Connector_Port);
                Duplex_Socket_User.SendBufferSize = 65 * 1024;// 10 * 1024 * 1024
                Duplex_Socket_User.ReceiveBufferSize = 65 * 1024;// 10 * 1024 * 1024
            }

            catch (Exception g)
            {
                Console.WriteLine("Error");

                Console.WriteLine(g.InnerException);

                Console.WriteLine(g.Message);


                Console.WriteLine(g.Source);

                Console.WriteLine(g.StackTrace);
            }

            return (Duplex_Socket_User);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public void Socket_Text_Sender(Socket New_Duplex_Socket_Accepter, string Text = "")
        {
            byte[] Byte_Message = Encoding.Default.GetBytes(Text + "~/..~");

            try
            {
                New_Duplex_Socket_Accepter.Send(Byte_Message);
            }

            catch (Exception)
            {

            }
        }

        public string Socket_Text_Reader(Socket New_Duplex_Socket_Accepter, int Packet_Size = 150)
        {
            byte[] Get_Text_Bytes = new byte[Packet_Size];

            try
            {
                New_Duplex_Socket_Accepter.Receive(Get_Text_Bytes);
            }

            catch (Exception)
            {
                return null;
            }

            string Get_Bytes_String = Encoding.Default.GetString(Get_Text_Bytes);

            return (Get_Bytes_String.Substring(0, Get_Bytes_String.LastIndexOf("~/..~")));
        }

        //----------------------------------------------------------------------------------------------------------------------------------------
    }
}
