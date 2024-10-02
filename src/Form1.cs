using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DOISM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // File picker dialog
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Open Save file to export",
                FileName = "User"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
             if (!File.Exists(ofd.FileName))
            {
                MessageBox.Show("Error! File does not exist!");
            }

            // Read configuration file
            List<string> fileInfo = new List<string>(File.ReadLines(ofd.FileName));

            // Read Base64 data
            string AHS_enc = fileInfo[6].Substring(6, fileInfo[6].Length - 7);
            string UserData_enc = fileInfo[11].Substring(6, fileInfo[11].Length - 7);

            // Convert Base64
            byte[] AHS_gzip = Convert.FromBase64String(AHS_enc);
            byte[] UserData_gzip = Convert.FromBase64String(UserData_enc);

            // Inflate
            byte[] AHS = ZlibStream.UncompressBuffer(AHS_gzip);
            byte[] UserData = ZlibStream.UncompressBuffer(UserData_gzip);

            // Convert to string
            string AHS_str = Encoding.UTF8.GetString(AHS);
            string UserData_str = Encoding.UTF8.GetString(UserData);

            // Cut off the null byte that's there for some reason
            AHS_str = AHS_str.Substring(0, AHS_str.Length - 1);
            UserData_str = UserData_str.Substring(0, UserData_str.Length - 1);

            // Combine the data
            string outSave = $"{AHS_str.Replace("]", ",")}{UserData_str}]";

            // Save File Dialog
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "json files (*.json)|*.json",
                Title = "Save exported save file"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            // Write to file
            File.WriteAllText(sfd.FileName, outSave);
            MessageBox.Show($"Save file successfully exported!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Open File Dialog
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "json files (*.json)|*.json",
                Title = "Open exported save file"
            };
            ofd.ShowDialog();

            // Read all the json data
            string jsonData = File.ReadAllText(ofd.FileName);

            // Find 2nd occurance of '[' to determine AHS end
            int end = jsonData.IndexOf("[", 1);

            // Store AHS json in seperate string
            string AHS_json = $"{jsonData.Substring(0, end - 1)}]";

            // Store UserData in seperate string
            string UserData_json = $"{jsonData.Substring(end, jsonData.Length - end - 1)}";

            // Convert to byte arrays
            byte[] AHS = new byte[AHS_json.Length + 1];
            Encoding.ASCII.GetBytes(AHS_json).CopyTo(AHS, 0);
            AHS[AHS.Length - 1] = 0x00;

            byte[] UserData = new byte[UserData_json.Length + 1];
            Encoding.ASCII.GetBytes(UserData_json).CopyTo(UserData, 0);
            UserData[UserData.Length - 1] = 0x00;

            // Deflate
            byte[] AHS_gzip = ZlibStream.CompressBuffer(AHS);
            byte[] UserData_gzip = ZlibStream.CompressBuffer(UserData);

            // Base64
            string AHS_enc = Convert.ToBase64String(AHS_gzip);
            string UserData_enc = Convert.ToBase64String(UserData_gzip);

            // Build our new savefile
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[?]");
            sb.AppendLine("?=\"Xzip by XGASOFT\"");
            sb.AppendLine("0=\"AHS\"");
            sb.AppendLine("1=\"UserData\"");
            sb.AppendLine("[AHS]");
            sb.AppendLine("index=\"0\"");
            sb.AppendLine($"data=\"{AHS_enc}\"");
            sb.AppendLine("path=\"\"");
            sb.AppendLine("readonly=\"false\"");
            sb.AppendLine("[UserData]");
            sb.AppendLine("index=\"1\"");
            sb.AppendLine($"data=\"{UserData_enc}\"");
            sb.AppendLine($"path=\"\"");
            sb.AppendLine("readonly=\"false\"");

            // Save File Dialog
            SaveFileDialog sfd = new SaveFileDialog()
            {
                FileName = "User",
                Title = "Save exported save file"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            File.WriteAllText(sfd.FileName, sb.ToString());
            MessageBox.Show("Save file Successfully imported!");
        }
    }
}