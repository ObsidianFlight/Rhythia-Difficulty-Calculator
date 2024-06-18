using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.IO;
using System.Windows.Forms;

namespace Rhythia_Difficulty_Calculator__GUI_
{
    public partial class form1 : Form
    {
        double speedmodifier = 1;
        List<MapInfo> mapList = new List<MapInfo>();
        int sortType = 1;
        public form1()
        {
            InitializeComponent();
        }
        OpenFileDialog openFileDialog1;
        private void button1_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Title = "Select Map List File",
                Filter = "Text Documents (*.txt)|*.txt"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sr = new StreamReader(dialog.FileName);
                        mapList.Add(CalculateDifficulty.ConvertMap(sr.ReadToEnd(), speedmodifier, false, " "));
                        UpdateTextbox();
                    }
                    catch (SecurityException ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        void UpdateTextbox()
        {
            string m1 = "";
            string m2 = "";
            string m3 = "";
            string m4 = "";
            string m5 = "";
            int index = 1;
            switch (sortType)
            {
                case 1: // Sort by Alphabetical
                    var orderedList1 = mapList.OrderBy(x => x.MapName);
                    foreach (var n in orderedList1)
                    {
                        n.index = index;
                        m1 += $"\n #{n.index}";
                        m2 += $"\n {n.MapName}";
                        m3 += $"\n {n.OverallDifficulty}";
                        m4 += $"\n {n.AverageDifficulty}";
                        m5 += $"\n {n.MaxDifficulty}";
                        index++;
                    }
                    richTextBox1.Text = m2;
                    richTextBox2.Text = m3;
                    richTextBox3.Text = m4;
                    richTextBox4.Text = m5;
                    richTextBox5.Text = m1;
                    break;

                case 2: // Sort by Average
                    orderedList1 = mapList.OrderByDescending(x => x.AverageDifficulty);
                    foreach (var n in orderedList1)
                    {
                        n.index = index;
                        m1 += $"\n #{n.index}";
                        m2 += $"\n {n.MapName}";
                        m3 += $"\n {n.OverallDifficulty}";
                        m4 += $"\n {n.AverageDifficulty}";
                        m5 += $"\n {n.MaxDifficulty}";
                        index++;
                    }
                    richTextBox1.Text = m2;
                    richTextBox2.Text = m3;
                    richTextBox3.Text = m4;
                    richTextBox4.Text = m5;
                    richTextBox5.Text = m1;
                    break;

                case 3: // Sort by Max
                    orderedList1 = mapList.OrderByDescending(x => x.MaxDifficulty);
                    foreach (var n in orderedList1)
                    {
                        n.index = index;
                        m1 += $"\n #{n.index}";
                        m2 += $"\n {n.MapName}";
                        m3 += $"\n {n.OverallDifficulty}";
                        m4 += $"\n {n.AverageDifficulty}";
                        m5 += $"\n {n.MaxDifficulty}";
                        index++;
                    }
                    richTextBox1.Text = m2;
                    richTextBox2.Text = m3;
                    richTextBox3.Text = m4;
                    richTextBox4.Text = m5;
                    richTextBox5.Text = m1;
                    break;

                default: // Sort by Overall, this is the important one.
                    orderedList1 = mapList.OrderByDescending(x => x.OverallDifficulty);
                    foreach (var n in orderedList1)
                    {
                        n.index = index;
                        m1 += $"\n #{n.index}";
                        m2 += $"\n {n.MapName}";
                        m3 += $"\n {n.OverallDifficulty}";
                        m4 += $"\n {n.AverageDifficulty}";
                        m5 += $"\n {n.MaxDifficulty}";
                        index++;
                    }
                    richTextBox1.Text = m2;
                    richTextBox2.Text = m3;
                    richTextBox3.Text = m4;
                    richTextBox4.Text = m5;
                    richTextBox5.Text = m1;
                    break;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //Sort by Alphabetical
            sortType = 1;
            UpdateTextbox();
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            //Sort by Overall Button
            sortType = 0;
            UpdateTextbox();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Sort by Average Button
            sortType = 2;
            UpdateTextbox();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Sort by Max Button
            sortType = 3;
            UpdateTextbox();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Change Name of Map button
            foreach(var n in mapList)
            {
                if(n.index == int.Parse(richTextBox6.Text))
                {
                    n.MapName = richTextBox7.Text;
                }
            }
            UpdateTextbox();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Remove Map in List Button
            foreach (var n in mapList)
            {
                if (n.index == int.Parse(richTextBox6.Text))
                {
                    mapList.Remove(n);
                    UpdateTextbox();
                    return;
                }
            }
            UpdateTextbox();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Save List Button
            using (var sfd = new SaveFileDialog
            {
                Title = "Save Colorset",
                Filter = "Text Documents (*.maplist)|*.maplist"
            })
            {
                var result = sfd.ShowDialog();

                if (result == DialogResult.OK)
                {

                    SaveMapList(sfd.FileName);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Load List Button
            using (var dialog = new OpenFileDialog
            {
                Title = "Select Map List File",
                Filter = "Text Documents (*.maplist)|*.maplist"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadMapList(dialog.FileName);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // Set Speed Button
            try
            {
                speedmodifier = Double.Parse(richTextBox9.Text);
            }
            catch { speedmodifier = 1; }
            for(int i = 0; i < mapList.Count; i++)
            {
                mapList[i] = CalculateDifficulty.ConvertMap(mapList[i].mapdata, speedmodifier, true, mapList[i].MapName);
            }
            UpdateTextbox();
        }

        private void richTextBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void LoadMapList(string file)
        {
            if (file == null)
                return;

            try
            {
                mapList.Clear();
                string[] theMapList = File.ReadAllLines(file);
                foreach(var map in theMapList)
                {
                    string[] theMapData = map.Split('│');
                    mapList.Add(CalculateDifficulty.ConvertMap(theMapData[1], speedmodifier, true, theMapData[0]));
                }
                UpdateTextbox();
            }
            catch { }
        }

        private void SaveMapList(string file)
        {
            if (file == null)
                return;

            string data = "";
            try
            {
                foreach(var map in mapList)
                {
                    data += $"{map.MapName}│{map.mapdata}\n";
                }
                File.WriteAllText(file, data, Encoding.UTF8);
            }
            catch {}
        }
    }
}
