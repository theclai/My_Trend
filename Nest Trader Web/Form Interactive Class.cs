using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nest_Trader_Web
{
    public class Form_Interactive_Class
    {
        public int Staus_Message_Lines_Cnt = 0;



        public void Create_Message_On_Status_Bar(String Message_String , bool Alert_Allowed = true , bool Set_Focus = true)
        {
            if (Alert_Allowed)
            {
                Message_String = DateTime.Now.ToString() + " : " + Message_String;
            }

            Action New_Action = (new Action(() =>
            {
                Label New_Btn_Box = new System.Windows.Forms.Label();

                New_Btn_Box.Size = new Size(1074, 20);

                New_Btn_Box.Location = new Point(4 - Nest_Trader_Form.Nest_Trader_Form_Instance.Status_Page_Tab.HorizontalScroll.Value, 26 * Staus_Message_Lines_Cnt + 7 - Nest_Trader_Form.Nest_Trader_Form_Instance.Status_Page_Tab.VerticalScroll.Value);
            
                Staus_Message_Lines_Cnt++;

                New_Btn_Box.FlatStyle = FlatStyle.Popup;

                New_Btn_Box.BackColor = Color.MistyRose;

                New_Btn_Box.ForeColor = Color.Black;

                New_Btn_Box.Visible = true;

                New_Btn_Box.Text = Message_String;

                New_Btn_Box.TextAlign = ContentAlignment.MiddleLeft;

                //

                Nest_Trader_Form.Nest_Trader_Form_Instance.Status_Page_Tab.Controls.Add(New_Btn_Box);

                if (Set_Focus)
                {
                    New_Btn_Box.Focus();
                }

                Nest_Trader_Form.Nest_Trader_Form_Instance.Message_Log_Buffer += Message_String + "\r\n";

                if (Alert_Allowed)
                {
                    Start_Audio_Alert();

                    Start_Pop_UP_Alert("Nest Trader - Plugin", Message_String);
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

        public void Start_Audio_Alert()
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (Nest_Trader_Form.Windows_Network_Class_Instance.Features_Array[3] != "True")
            {
                return;
            }

            if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Audio_Alert_Allowed.Checked)
            {
                return;
            }

            Action New_Action = (new Action(() =>
            {
                System.Media.SystemSounds.Exclamation.Play();
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

        public void Start_Pop_UP_Alert(string Message_Box_Title, string Message_Box_Text)
        {
            if (Nest_Trader_Form.Windows_Network_Class_Instance.Nest_Trader_Outdated)
            {
                return;
            }

            if (Nest_Trader_Form.Windows_Network_Class_Instance.Features_Array[4] != "True")
            {
                return;
            }

            if (!Nest_Trader_Form.Nest_Trader_Form_Instance.Pop_UP_Alert.Checked)
            {
                return;
            }

            Action New_Action = (new Action(() =>
            {
                MessageBox.Show(Message_Box_Text, Message_Box_Title, MessageBoxButtons.OK);
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


        public string Open_File_Dialog(string File_Filter)
        {
            OpenFileDialog OFD = new OpenFileDialog();

            OFD.ShowDialog();

            string File_Name_No_Ext = Path.GetFileNameWithoutExtension(OFD.FileName);

            return File_Name_No_Ext + OFD.Filter;
        }

        public string Save_File_Dialog(string File_Filter)
        {
            SaveFileDialog SFD = new SaveFileDialog();

            SFD.ShowDialog();

            string File_Name_No_Ext = Path.GetFileNameWithoutExtension(SFD.FileName);

            return File_Name_No_Ext + SFD.Filter;
        }
    }
}
