using System.Data;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using Alturos.Yolo;
using Alturos.Yolo.Model;

namespace WinFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox3.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
            InitializeDataGridView();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private FilterInfoCollection CaptureDevices;
        private VideoCaptureDevice videoSource;
        private YoloWrapper yolo;
        int frameCount = 0;
        int count = 0;
        int countX;
        string pathNet;
        List<YoloItem> _items;

        private void button1_Click(object sender, EventArgs e)
        {
            videoSource = new VideoCaptureDevice(CaptureDevices[comboBox1.SelectedIndex].MonikerString);
            videoSource.NewFrame += videoSource_NewFrame;
            videoSource.Start();
        }
        private void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap img = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = img;
            if (frameCount == count)
            {
                count += countX;
                CNN_detection(img);
            }
            frameCount++;
        }
        private void CNN_detection(Image img)
        {
               var img2 = img;
               pictureBox2.Image = img2;
               var configurationDetector = new YoloConfigurationDetector();
               var config = configurationDetector.Detect(pathNet);
               yolo = new YoloWrapper(config);
               var memoryStream = new MemoryStream();
               pictureBox2.Image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
               _items = yolo.Detect(memoryStream.ToArray()).ToList();
               AddDetailsToPictureBox(pictureBox2, _items);

               if (_items.Count != 0)
               {
                ResetSource();
                GetSource(_items);
               }
               else
               {
                ResetSource();
               }
        }
        private void ResetSource()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = "";
                }
            }
        }

        private void GetSource(List<YoloItem> _items)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                    dataGridView1.Rows[i].Cells[0].Value = _items[i].Type;
                    dataGridView1.Rows[i].Cells[1].Value = Math.Round(_items[i].Confidence,3);
                    dataGridView1.Rows[i].Cells[2].Value = _items[i].X;
                    dataGridView1.Rows[i].Cells[3].Value = _items[i].Y;
                    dataGridView1.Rows[i].Cells[4].Value = _items[i].Width;
                    dataGridView1.Rows[i].Cells[5].Value = _items[i].Height;
            }
        }

        private void InitializeDataGridView()
        {
            dataGridView1.ColumnCount = 6;
            dataGridView1.RowCount = 7;
            dataGridView1.ColumnHeadersVisible = true;
            dataGridView1.AutoSize = true;
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
            columnHeaderStyle.BackColor = Color.Beige;
            columnHeaderStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            dataGridView1.Columns[0].Name = "Type";
            dataGridView1.Columns[1].Name = "Confidence";
            dataGridView1.Columns[2].Name = "X";
            dataGridView1.Columns[3].Name = "Y";
            dataGridView1.Columns[4].Name = "Width";
            dataGridView1.Columns[5].Name = "Height";
        }

        private void AddDetailsToPictureBox(PictureBox pictureBoxToRender, List<YoloItem> items)
        {
            var img = pictureBoxToRender.Image;
            var font = new Font("Arial", 18, FontStyle.Bold);
            var brush = new SolidBrush(Color.Red);
            var graphics = Graphics.FromImage(img);
            
            foreach (YoloItem item in items)
                 {
                     var x = item.X;
                     var y = item.Y;
                     var width = item.Width;
                     var height = item.Height;
                     var tung = item.Type;
                     var confid = item.Confidence;
                     var rect = new Rectangle(x, y, width, height);
                     var pen = new Pen(Color.LightGreen, 2);
                     var point = new Point(x, y);
                     graphics.DrawRectangle(pen, rect);
                     graphics.DrawString(item.Type, font, brush, point);
            }
            pictureBoxToRender.Image = img;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevices)
            {
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = 0;
            videoSource = new VideoCaptureDevice();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (videoSource.IsRunning)
            {
                videoSource.Stop();
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            pathNet = Directory.GetCurrentDirectory() + "\\networks\\" + comboBox2.Text +"\\";
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCount = comboBox3.SelectedItem.ToString();
            countX = Convert.ToInt32(selectedCount);
        }
    }
}
