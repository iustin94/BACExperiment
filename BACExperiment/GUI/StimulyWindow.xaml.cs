﻿using BACExperiment.GUI;
using BACExperiment.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
namespace BACExperiment
{
    /// <summary>
    /// Interaction logic for StimulyWindow.xaml
    /// </summary>
    public partial class StimulyWindow : Window
    {

        #region INotifyPropertyChangedImplementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


        //Links
        private Course course;
        private AnimationQueue queue1;
        private coordinateRecorder recorder;
        private CoordinateHolder holder;
        private System.Timers.Timer t;
        private MainWindow mainWindow;
        private StimulyWindowViewModel model = StimulyWindowViewModel.GetInstance();
      

        //Variables
        private int CourseMode;
        private int CourseSpeed;
        private int CourseComplexity;
        private bool random;
        private bool showTrajectory;
        private Random r = new Random();
        private int color = 0;
        private List<System.Windows.Point> coordinates;




        private static StimulyWindow instance;

     
        private delegate void  CourseGenerate(List<System.Windows.Point> coordiantes ) ;
        CourseGenerate generate;

        public int getCourseMode() { return CourseMode; }
        public void setCourseMode(int CourseMode) { this.CourseMode = CourseMode; }
        public int getCourseSpeed() { return CourseSpeed; }
        public void setCourseSpeed(int CourseSpeed) { this.CourseSpeed = CourseSpeed; }
        public int getCourseComplexity() { return CourseComplexity; }
        public void setCourseComplexity(int CourseComplexity) { this.CourseComplexity = CourseComplexity; }
        public bool isRandom() { return random; }
        public void setRandom(bool random) { this.random = random; }
        public bool isShowTrajectory() { return showTrajectory; }
        public void setShowTrajectory(bool value) { this.showTrajectory = value; }
        public static StimulyWindow GetInstance(MainWindow observer) { if(instance == null){ instance = new StimulyWindow(observer);} return instance; }
         


        private StimulyWindow(MainWindow mainWindow /*, string mode , int complexity*/ )
        {

           
          
            InitializeComponent();
            Pointer1.DataContext = model;
            Pointer2.DataContext = model;
          
            
            course = new Course(this);
            this.queue1 = new AnimationQueue(StimulyEllipse1, Canvas.LeftProperty, Canvas.TopProperty);
            this.mainWindow = mainWindow;
            recorder = coordinateRecorder.getInstance(this);
            holder = CoordinateHolder.GetInstance();

            t = new System.Timers.Timer();
            t.Elapsed += new ElapsedEventHandler(SendInfo);
            t.Interval += 100;

            this.SetBinding(Window.WidthProperty, new Binding("RezolutionX") { Source = model, Mode = BindingMode.OneWayToSource });
            this.SetBinding(Window.HeightProperty, new Binding("RezolutionY") { Source = model, Mode = BindingMode.OneWayToSource });

        }


        public void buildCourse()
        {
            if (generate == null)
            { MessageBox.Show(" Please select a course setting from the main window before attempting to run ."); }
            else
            {
                if(CourseComplexity == null )
                { MessageBox.Show(" Please select a course complexity ."); }

            }
        }

        private void Synchronous(List<Point> coordinates )
        {
          
            int i = 1;
            double lastX = 0;
            double lastY = 0;
            while (i != coordinates.Count)
            {


                if (Math.Abs(coordinates[i].X - coordinates[i - 1].X) > 3 || Math.Abs(coordinates[i].Y - coordinates[i - 1].Y) > 3)
                {
                   this.queue1.queueAnimation(new DoubleAnimation(coordinates[i].X - 50, new Duration(TimeSpan.FromMilliseconds(1000 / CourseSpeed / 3))),
                                           new DoubleAnimation(coordinates[i].Y - 50, new Duration(TimeSpan.FromMilliseconds(1000 / CourseSpeed / 3)))
                                           );

                    Line l = new Line();
                    l.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                    if (showTrajectory == true)
                    {

                        //Starts the line drawing from where the first coordinate set is
                        if (lastX == 0 || lastY == 0)
                        {
                            lastX = coordinates[i].X;
                            lastY = coordinates[i].Y;
                        }

                        //Draw the line 
                        l.X1 = lastX;
                        l.X2 = coordinates[i].X;
                        l.Y1 = lastY;
                        l.Y2 = coordinates[i].Y;

                        l.StrokeThickness = 2;
                        if (color % 2 == 0)
                            l.Stroke = System.Windows.Media.Brushes.Blue;
                        else
                            l.Stroke = System.Windows.Media.Brushes.Red;
                        color++;
                        StimulyReferencePoint.Children.Add(l);
                    }

                }

                //Position Ellipse to the begining of the course

                if (i == 1)
                {
                    Canvas.SetLeft(StimulyEllipse1, coordinates[i].X - 50);
                    Canvas.SetTop(StimulyEllipse1, coordinates[i].Y - 50);
                }

                lastX = coordinates[i].X;
                lastY = coordinates[i].Y;
                i++;
            }

        }

        private void Self_Paced(List<Point> coordinates)
        {
           
            int i = 1;
            double lastX = 0;
            double lastY = 0;
            while (i != coordinates.Count)
            {
                    Line l = new Line();
                    l.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

                        if (lastX == 0 || lastY == 0)
                        {
                            lastX = coordinates[i].X;
                            lastY = coordinates[i].Y;
                        }

                        //Draw the line 
                        l.X1 = lastX;
                        l.X2 = coordinates[i].X;
                        l.Y1 = lastY;
                        l.Y2 = coordinates[i].Y;

                        l.StrokeThickness = 2;
                        if (color % 2 == 0)
                            l.Stroke = System.Windows.Media.Brushes.Blue;
                        else
                            l.Stroke = System.Windows.Media.Brushes.Red;
                        color++;
                        StimulyReferencePoint.Children.Add(l);
                }

                //Position Ellipse to the begining of the course

                if (i == 1)
                {
                    Canvas.SetLeft(StimulyEllipse1, coordinates[i].X - 50);
                    Canvas.SetTop(StimulyEllipse1, coordinates[i].Y - 50);
                }

                lastX = coordinates[i].X;
                lastY = coordinates[i].Y;
                i++;
            }

        private void Asynchronous(List<Point> coordinates)
        {

        }
        
        public void startCourse()
        {
            queue1.start();
        }

        private class AnimationQueue
        {
            private List<DoubleAnimation> animation1;
            private DependencyProperty property1;

            private List<DoubleAnimation> animation2;
            private DependencyProperty property2;

            private int curent;
            private UIElement element;
            private bool finished;

            public AnimationQueue(UIElement element, DependencyProperty property)
            {
                curent = -1;
                this.element = element;
                animation1 = new List<DoubleAnimation>();
                animation2 = new List<DoubleAnimation>();
                this.property1 = property;
            }

            public AnimationQueue(UIElement element, DependencyProperty property1, DependencyProperty property2)
            {
                curent = -1;
                this.element = element;
                animation1 = new List<DoubleAnimation>();
                animation2 = new List<DoubleAnimation>();
                this.property1 = property1;
                this.property2 = property2;
               
            }

            public void queueAnimation(DoubleAnimation animation1, DoubleAnimation animation2)
            {

                this.animation1.Add(animation1);
                this.animation2.Add(animation2);
                

                animation1.Completed += (s, e) =>
                {
                    if (this.animation1.Contains(animation1))
                    {
                        int index1 = this.animation1.IndexOf(animation1);
                        if (index1 + 1 < this.animation1.Count)
                        {
                            element.BeginAnimation(property1, this.animation1[index1 + 1]);
                        }
                    }
                };
                animation2.Completed += (s, e) =>
                {
                    if (this.animation2.Contains(animation2))
                    {
                        int index2 = this.animation2.IndexOf(animation2);
                        if (index2 + 1 < this.animation2.Count)
                        {
                            element.BeginAnimation(property2, this.animation2[index2 + 1]);

                        }
                    }
                };



            }



            public void start(int i)
            {
                if (animation1.Count != animation2.Count)
                    Console.WriteLine("Animation Queue not equal");
                else
                {
                    if (i == animation1.Count)
                        Console.WriteLine("Animation finished");
                    else if (i <= animation1.Count && i <= animation2.Count)
                    {
                        element.BeginAnimation(property1, animation1[i]);
                        element.BeginAnimation(property2, animation2[i]);
                        curent++;
                        if (curent == animation1.Capacity)
                            finished = true;
                    };
                }
            }

            public void start()
            {
                curent = 0;
                element.BeginAnimation(property1, animation1[curent]);
                element.BeginAnimation(property2, animation2[curent]);

            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {

            mainWindow.OpenBtn.IsEnabled = true;
            mainWindow.complexitySlider.IsEnabled = true;
            mainWindow.SpeedSlider.IsEnabled = true;           
            mainWindow.StopFullRecording();
            recorder.Stop();

            t.Stop();

            instance = null;
        }

        

        private Point makeToCartezian(int x, int y)
        {
            Point p = new Point();
            p.X = (500 - x);
            p.Y = (500 - y);
            return p;
        }

      
        public void startRecording()
        {
            recorder.Run();
        }

        public void StartSendingInfo()
        {
            t.Start();
        }

        private void SendInfo(object sender, ElapsedEventArgs e)
        {
            

            Action action = () =>
            {

                holder.SetEllipseCoordinates(Canvas.GetLeft(StimulyEllipse1), Canvas.GetTop(StimulyEllipse1));
                holder.SetPointerCoordinates(0, new Point(Canvas.GetLeft(Pointer1), Canvas.GetTop(Pointer1)));
                holder.SetPointerCoordinates(1, new Point(Canvas.GetLeft(Pointer2), Canvas.GetTop(Pointer2)));
            };

            Dispatcher.BeginInvoke(action);
        }

    

        


  

    }
}
