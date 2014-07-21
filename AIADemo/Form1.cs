using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Net;
using System.Drawing.Imaging;


namespace AIADemo
{
    public partial class Form1 : Form
    {
        const int NumPerQuery = 10;
        const int BinPerImg = 8;

        float param_w = 100;
        int num_query = 0;

        int bins = BinPerImg * BinPerImg * BinPerImg; 

        string pathVocabulary = "G:\\ASmallVocabulary.txt";
        string pathImgDB = @"G:\ImgDB";
        string pathAIAResults = null;
        string file2Anno = @"G:\ImgDB\city\2.jpg";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.pathVocabulary = System.Environment.CurrentDirectory;
            textBox1.Text = pathVocabulary;
            textBox2.Text = pathImgDB;
            textBox3.Text = param_w.ToString();
            textBox4.Text = file2Anno;
            //pictureBox1.Image = Image.FromFile(file2Anno);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = System.Environment.CurrentDirectory;
            openFileDialog1.Filter = "文本文件 (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                pathVocabulary = openFileDialog1.FileName;
            textBox1.Text = pathVocabulary;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            pathVocabulary = textBox1.Text.Trim();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pathVocabulary.EndsWith(".txt"))
                System.Diagnostics.Process.Start(pathVocabulary);
            else
                MessageBox.Show("请选择一个txt文件！");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                pathImgDB = folderBrowserDialog1.SelectedPath;
            textBox2.Text = pathImgDB;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (pathImgDB == null)
                MessageBox.Show("请新建一个目录用于存储图像！");
            else
            {
                ImgDB imdb = new ImgDB(pathVocabulary, pathImgDB, NumPerQuery, label3);
                imdb.getImgDB();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pathImgDB = folderBrowserDialog1.SelectedPath;
                    Directory.CreateDirectory(pathImgDB);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error when creating directory!");
                }
            }
            textBox2.Text = pathImgDB;
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            StreamReader reader = null;
            FeatureExtracter fe = new FeatureExtracter(pathImgDB, BinPerImg, NumPerQuery);

            try
            {
                reader = new StreamReader(pathVocabulary);
                for (string query = reader.ReadLine(); query != null; query = reader.ReadLine())
                {
                    label3.Text = "extracting:" + query + "...";
                    fe.extractImgs(query);
                }
                label3.Text = "特征提取完毕";
                reader.Close();
            }
            catch (Exception ex)
            {
            }
        }        

        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = System.Environment.CurrentDirectory;
            openFileDialog1.Filter = "图像文件 (*.jpg;*.jpeg;*.bmp)|*.jpg;*.jpeg;*.bmp";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                file2Anno = openFileDialog1.FileName;
            
            textBox4.Text = file2Anno;
            pictureBox1.Image = Image.FromFile(file2Anno);
        }

        private void button7_Click(object sender, EventArgs e)
        {          
            if (file2Anno.EndsWith(".jpg") || file2Anno.EndsWith(".jpeg") || file2Anno.EndsWith(".bmp"))
            {                 
                StreamReader reader = null;                
                string[] querys = null;

                //read the vocabulary
                try
                {
                    num_query = 0;
                    reader = new StreamReader(pathVocabulary);
                    for (string query = reader.ReadLine(); query != null; query = reader.ReadLine())
                    {
                        num_query++;
                    }
                    reader.Close();
                    querys = new string[num_query];

                    reader = new StreamReader(pathVocabulary);
                    int i = 0;
                    for (string query = reader.ReadLine(); query != null; query = reader.ReadLine())
                    {
                        querys[i++] = query;
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                }

                label3.Text = "标注中……";
                //annotate
                AIA aia = new AIA(num_query, querys, NumPerQuery, pathImgDB, BinPerImg);
                double[] scores = aia.annoImg_weightedKNN(file2Anno, param_w);

                //display the results
                label3.Text = "标注完成";
                pictureBox1.Image = Image.FromFile(file2Anno);
                ListViewItem lvi = null;
                while (listView1.Items.Count > 0)
                    listView1.Items.RemoveAt(listView1.Items.Count - 1);
                for (int i = 0; i < num_query; i++)
                {
                    lvi = listView1.Items.Add(querys[i]);
                    float prob = (int)(scores[i] * 10000) / (float)10000;
                    lvi.SubItems.Add(prob.ToString());
                }               
            }
            else
                MessageBox.Show("Please choise a image file!");
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (textBox3.Text.Length > 0)
                    param_w = float.Parse(textBox3.Text.Trim());
            }
            catch (Exception ex)
            {
                System.Console.Write(textBox3.Text);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {        
        }

        private void label8_Click(object sender, EventArgs e)
        {
        }
    }
}
