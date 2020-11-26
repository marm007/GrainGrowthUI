using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Reflection;

namespace GrainGrowthUI
{
    class Simulation
    {
        public class SimulationListItem : INotifyPropertyChanged
        {
            public string ID { get; set; }

            public string SizeX { get; set; }

            public string SizeY { get; set; }

            public string SizeZ { get; set; }

            public string Neighbourhood { get; set; }

            public string BC { get; set; }

            public string Nucleons { get; set; }

            public string Simulation { get; set; }

            public string NumberOfIterations { get; set; }

            public string KT { get; set; }

            public string J { get; set; }

            public string PreparationTime
            {
                get { return preparationTime; }
                set
                {
                    if (preparationTime != value)
                    {
                        preparationTime = value;
                        OnPropertyChanged("PreparationTime");
                    }
                }
            }

            public string SimulationTime
            {
                get { return simulationTime; }
                set
                {
                    if (simulationTime != value)
                    {
                        simulationTime = value;
                        OnPropertyChanged("SimulationTime");
                    }
                }
            }

            public string WriteToFileTime
            {
                get { return writeToFileTime; }
                set
                {
                    if (writeToFileTime != value)
                    {
                        writeToFileTime = value;
                        OnPropertyChanged("WriteToFileTime");
                    }
                }
            }

            public int ProgressValue { get { return progressValue; }
                set {
                    if (progressValue != value)
                    {
                        progressValue = value;
                        OnPropertyChanged("ProgressValue");
                    }
                }
            }

            public bool ProgressBool { get { return progressBool; }
                set {
                    if(progressBool != value)
                    {
                        progressBool = value;
                        OnPropertyChanged("ProgressBool");
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }


            string preparationTime = "0";
            string simulationTime = "0" ;
            string writeToFileTime = "0";
            int progressValue;
            bool progressBool;
        }

        private string number;
        private string myFilePath;
        private SimulationListItem item;

        private int size;
        private int gapSize;

        private MyColors myColors;

        private Label wallsLabel;
        private List<string[,]> walls;
        private Canvas canvas;
        private Image image;

        public Simulation(string counter, string fileName, string sizeX, string sizeY, string sizeZ, 
                          string neighbourhood, string bc, string numberOfNucleons, string simulation,
                          string numberOfIterations, string kt, string j, 
                          StackPanel simulationPanel, ListView listView) {

            this.myColors = new MyColors();

            this.number = counter;

            int maxSize = Math.Max(Int32.Parse(sizeX), Math.Max(Int32.Parse(sizeY), Int32.Parse(sizeZ)));

            if (maxSize < 25)
            {
                this.size = 20;
                this.gapSize = 1;
            }
            else if (maxSize < 50)
            {
                this.size = 10;
                this.gapSize = 2;
            }
            else if (maxSize < 100)
            {
                this.size = 5;
                this.gapSize = 4;
            }
            else if (maxSize < 250)
            {
                this.size = 2;
                this.gapSize = 6;
            }
            else
            {
                this.size = 1;
                this.gapSize = 10;
            }

            myColors.InitializeCellColors(Int32.Parse(numberOfNucleons) + 1);

            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string FilePath = @"C:\Users\Marcin\source\repos\GrainGrowthUI\GrainGrowthUI\";

            this.myFilePath = FilePath + fileName + "_" + milliseconds + ".xml";

            this.item = new SimulationListItem
            {
                ID = counter,
                SizeX = sizeX,
                SizeY = sizeY,
                SizeZ = sizeZ,
                Neighbourhood = neighbourhood,
                BC = bc,
                Nucleons = numberOfNucleons,
                Simulation = simulation,
                NumberOfIterations = numberOfIterations,
                KT = kt,
                J = j,
                ProgressValue = 0,
                ProgressBool = true
            };

            listView.Items.Add(item);


            using (XmlWriter writer = XmlWriter.Create(this.myFilePath))
            {

                writer.WriteStartElement("CG_config");
                writer.WriteElementString("FilePath", fileName + ".txt");
                writer.WriteElementString("SizeX", sizeX);
                writer.WriteElementString("SizeY", sizeY);
                writer.WriteElementString("SizeZ", sizeZ);
                writer.WriteElementString("Neighbourhood", neighbourhood);
                writer.WriteElementString("BC", bc);
                writer.WriteElementString("Nucleons", numberOfNucleons);
                writer.WriteElementString("Simulation", simulation);
                writer.WriteElementString("NumberOfIterations", numberOfIterations);
                writer.WriteElementString("KT", kt);
                writer.WriteElementString("J", j);
                writer.WriteEndElement();
                writer.Flush();
            }
        }


        public void Run(string fileName, TabControl tabControl, ListView listView)
        {

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;

            image = new Image();

            TabItem ti = new TabItem();

            Viewbox viewbox = new Viewbox();

            Grid grid = new Grid();
            grid.Width = 1200;
            grid.Height = 600;


            ColumnDefinition c1 = new ColumnDefinition();
            ColumnDefinition c2 = new ColumnDefinition();

            c1.Width = new GridLength(4, GridUnitType.Star);
            c2.Width = new GridLength(1, GridUnitType.Star);
            
            grid.ColumnDefinitions.Add(c1);
            grid.ColumnDefinitions.Add(c2);

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            Grid.SetColumn(scrollViewer, 0);

            canvas = new Canvas();
            canvas.Margin = new Thickness(10);
            canvas.Background = new SolidColorBrush(Colors.Black);
            canvas.HorizontalAlignment = HorizontalAlignment.Left;
            canvas.VerticalAlignment = VerticalAlignment.Top;

            scrollViewer.Content = canvas;


            Grid rightGrid = new Grid();
            RowDefinition r1 = new RowDefinition();
            RowDefinition r2 = new RowDefinition();

            r1.Height = new GridLength(2, GridUnitType.Star);
            r2.Height = new GridLength(1, GridUnitType.Star);

            rightGrid.RowDefinitions.Add(r1);
            rightGrid.RowDefinitions.Add(r2);

            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(10);

            PropertyInfo[] properties = typeof(SimulationListItem).GetProperties();
            int pIndex = 0;

            foreach (PropertyInfo property in properties)
            {
                if (pIndex >= 1 && pIndex <= 13)
                {
                    var bold = new Bold(new Run(property.Name + ": "));
                    var normal = new Run(property.GetValue(item, null).ToString() + "\n");

                    textBlock.Inlines.Add(bold);
                    textBlock.Inlines.Add(normal);
                }
                pIndex++;
            }

            StackPanel containerPanel = new StackPanel();

            StackPanel labelPanel = new StackPanel();
            labelPanel.HorizontalAlignment = HorizontalAlignment.Left;
            Button button = new Button();
            button.Margin = new Thickness(5);
            button.Width = 50;
            button.Height = 25;
            button.Content = "Show all";
            button.Click += new RoutedEventHandler(showAllButton_Click);


            labelPanel.Children.Add(button);

            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Horizontal;

            containerPanel.Children.Add(labelPanel);
            containerPanel.Children.Add(buttonPanel);


            wallsLabel = new Label();
            wallsLabel.Margin = new Thickness(5);
            wallsLabel.Content = "Wall: Left";
            wallsLabel.Visibility = Visibility.Hidden;



            Button bLeft = new Button();
            bLeft.Margin = new Thickness(5);
            bLeft.Width = 50;
            bLeft.Height = 25;
            bLeft.Content = "<-";
            bLeft.Click += new RoutedEventHandler(leftButton_Click);

            Button bRight = new Button();
            bRight.Margin = new Thickness(5);
            bRight.Width = 50;
            bRight.Height = 25;
            bRight.Content = "->";
            bRight.Click += new RoutedEventHandler(rightButton_Click);

            buttonPanel.Children.Add(bLeft);
            buttonPanel.Children.Add(bRight);

            buttonPanel.Children.Add(wallsLabel);

            Grid.SetRow(textBlock, 0);
            Grid.SetRow(containerPanel, 1);


            rightGrid.Children.Add(textBlock);
            rightGrid.Children.Add(containerPanel);

            Grid.SetColumn(rightGrid, 1);

            grid.Children.Add(scrollViewer);
            grid.Children.Add(rightGrid);

            viewbox.Child = grid;

            ti.Content = viewbox;
            ti.Header = "Simulation " + this.number;
            ti.IsEnabled = false;

            canvas.Children.Add(image);


            tabControl.Items.Insert(tabControl.Items.Count, ti);

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {

            string returnvalue = string.Empty;

            ProcessStartInfo info = new ProcessStartInfo(fileName);
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            info.Arguments = this.myFilePath;

            using (Process process = Process.Start(info))
            {
                returnvalue = process.StandardOutput.ReadLine();

                string preparingTime = process.StandardOutput.ReadLine();
                string simulationTime = process.StandardOutput.ReadLine();
                string writingToFileTime = process.StandardOutput.ReadLine();

                this.item.PreparationTime = preparingTime;
                this.item.SimulationTime = simulationTime;
                this.item.WriteToFileTime = writingToFileTime;

            }


            string[] splited = returnvalue.Trim().Split(' ');
            walls = Deserialize(splited[0]);

            int sizeX = 0;
            int sizeY = 0;


            for (int i = 0; i < walls.Count; i += 2)
            {
                int mSizeX = (walls[i].GetLength(0) + 1) * size;
                if (i + 1 < walls.Count)
                    mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
             }

            WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
            simulationBitmap.Clear(Colors.White);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                canvas.Width = sizeX;
                canvas.Height = sizeY;

                if (walls.Count == 1)
                {
                    wallsLabel.Visibility = Visibility.Hidden;
                    button.Visibility = Visibility.Hidden;
                    bLeft.Visibility = Visibility.Hidden;
                    bRight.Visibility = Visibility.Hidden;

                }
            }));

            Draw(walls, simulationBitmap, size);

            simulationBitmap.Freeze();

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                ti.IsEnabled = true;
                image.Source = simulationBitmap;
                this.item.ProgressBool = false;
                this.item.ProgressValue = 100;
            }));

            });

            backgroundWorker.RunWorkerAsync();

        }

        // 21XLH70r

        private List<string [,]> Deserialize(string path)
        {
            
            System.IO.StreamReader file =
                new System.IO.StreamReader(path);

            string currentLine = file.ReadLine();
            int index = 0;

            List<string[,]> walls = new List<string[,]>();

            string[,] array = null;

            while (String.IsNullOrEmpty(currentLine) == false)
            {
                if (currentLine.Contains("dim"))
                {
                    if (index != 0)
                        walls.Add(array);

                    string[] sizes = currentLine.Split(' ');
                    array = new string[Int32.Parse(sizes[1]), Int32.Parse(sizes[2])];
                    index = 0;
                }
                else
                {
                    string[] splited = currentLine.Split(' ');

                    for (int i = 0; i < splited.Length; i++)
                    {
                        array[index, i] = splited[i];
                    }
                    index++;
                }


                currentLine = file.ReadLine();
            }

            walls.Add(array);


            return walls;
        }

        private void Draw(List<string[,]> walls, WriteableBitmap bitmap, int size)
        {

            int oX = 0;
            int oY = 0;

            for (var k = 0; k < walls.Count; k++)
            {
                if (k != 0 && k % 2 == 0)
                    oX = 0;
                if (k != 0 && k % 2 == 0)
                    oY++;

                int sizeX = k - 1 >= 0 && k % 2 != 0 ? walls[k - 1].GetLength(0) + this.gapSize :  0;
                int sizeY = k - 4 >= 0 ? (walls[k - 4].GetLength(1) + this.gapSize + walls[k - 2].GetLength(1) + this.gapSize) : 
                                         k - 2 >= 0 ? walls[k - 2].GetLength(1) + this.gapSize : 0;


                for (int i = 0; i < walls[k].GetLength(0); i++)
                {
                    for (int j = 0; j < walls[k].GetLength(1); j++)
                    {
                        DrawRectangle(bitmap, i +  sizeX,
                                      j + sizeY, size, myColors.Cell[Int32.Parse(walls[k][i, j])]);
                    }
                }

                oX++;

            }
        }

        private static void DrawRectangle(WriteableBitmap bitmap,
                                            int left, int top, int size, Color color)
        {
            var x1 = left * size;
            var y1 = top * size;
            var x2 = x1 + size;
            var y2 = y1 + size;

            bitmap.FillRectangle(x1, y1, x2, y2, color);


        }



        void showAllButton_Click(Object sender, RoutedEventArgs e)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;

            wallsLabel.Visibility = Visibility.Hidden;

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {
                int sizeX = 0;
                int sizeY = 0;
                for (int i = 0; i < walls.Count; i += 2)
                {
                    int mSizeX = (walls[i].GetLength(0) + 1) * size;
                    if (i + 1 < walls.Count)
                        mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
                }

                WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
                simulationBitmap.Clear(Colors.White);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    canvas.Width = sizeX;
                    canvas.Height = sizeY;
                }));

                Draw(walls, simulationBitmap, size);

                simulationBitmap.Freeze();

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    image.Source = simulationBitmap;
                }));
            });

            backgroundWorker.RunWorkerAsync();
        }


        private int wallsCounter = 0;

        string CheckWall()
        {
            switch (wallsCounter)
            {
                case 0:
                    return "Back";
                case 1:
                    return "Front";
                case 2:
                    return "Left";
                case 3:
                    return "Right";
                case 4:
                    return "Top";
                default:
                    return "Bottom";
            }
        }

        void leftButton_Click(Object sender, RoutedEventArgs e)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            wallsLabel.Visibility = Visibility.Visible;

            wallsCounter--;

            if (wallsCounter < 0)
                wallsCounter = this.walls.Count - 1;

            wallsLabel.Content = CheckWall();

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {
               

                List<string[,]> walls = new List<string[,]>();
                walls.Add(this.walls[wallsCounter]);

                int sizeX = 0;
                int sizeY = 0;
                for (int i = 0; i < walls.Count; i += 2)
                {
                    int mSizeX = (walls[i].GetLength(0) + 1) * size;
                    if (i + 1 < walls.Count)
                        mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
                }
                WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
                simulationBitmap.Clear(Colors.White);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    canvas.Width = sizeX;
                    canvas.Height = sizeY;
                }));

                Draw(walls, simulationBitmap, size);

                simulationBitmap.Freeze();

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {

                    image.Source = simulationBitmap;
                }));
            });

            backgroundWorker.RunWorkerAsync();
        }


        void rightButton_Click(Object sender, RoutedEventArgs e)
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            wallsLabel.Visibility = Visibility.Visible;

            wallsCounter++;

            if (wallsCounter >= walls.Count)
                wallsCounter = 0;

            wallsLabel.Content = CheckWall();

            backgroundWorker.DoWork += new DoWorkEventHandler((state, arg) =>
            {


                List<string[,]> walls = new List<string[,]>();
                walls.Add(this.walls[wallsCounter]);

                int sizeX = 0;
                int sizeY = 0;
                for (int i = 0; i < walls.Count; i += 2)
                {
                    int mSizeX = (walls[i].GetLength(0) + 1) * size;
                    if (i + 1 < walls.Count)
                        mSizeX += (walls[i + 1].GetLength(0) + 1) * size;


                    if (mSizeX > sizeX)
                        sizeX = mSizeX;

                    sizeY += (walls[i].GetLength(1) + 1) * size;
                }
                WriteableBitmap simulationBitmap = BitmapFactory.New(sizeX, sizeY);
                simulationBitmap.Clear(Colors.White);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    canvas.Width = sizeX;
                    canvas.Height = sizeY;
                }));

                Draw(walls, simulationBitmap, size);

                simulationBitmap.Freeze();

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {

                    image.Source = simulationBitmap;
                }));
            });

            backgroundWorker.RunWorkerAsync();
        }
    }
}
