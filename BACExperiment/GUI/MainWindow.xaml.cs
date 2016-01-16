﻿using BACExperiment.GUI;
using BACExperiment.Model;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BACExperiment
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private Service service;
        private MovementWindow stimulyWindow;
        private ReadingWIndow prompter;
        public event PropertyChangedEventHandler PropertyChanged;
        private SequenceForm sequence;

        #region INotifyPropertyChanged Members

        protected void Notify(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion

        public MainWindow()
        {


            InitializeComponent();

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;


            service = Service.getInstance(this);
            CourseModeSelect.Items.Add(new ComboboxItem("Synchronous", "Synchronous"));
            CourseModeSelect.Items.Add(new ComboboxItem("Asynchronous", "Asynchronous"));
            CourseModeSelect.Items.Add(new ComboboxItem("Self-Paced", "Self-Paced"));

            PrompterModeSelect.Items.Add(new ComboboxItem("Synchronous", "Synchronous"));
            PrompterModeSelect.Items.Add(new ComboboxItem("Asynchronous", "Asynchronous"));
            PrompterModeSelect.Items.Add(new ComboboxItem("Self-Paced", "Self-Paced"));

            Mic1_VolumeBar.DataContext = this;
            Mic2_VolumeBar.DataContext = this;

            service.getMicrophoneList();
            WM1_groupbox.DataContext = service.wm1_data_context;
            WM2_groupbox.DataContext = service.wm2_data_context;


            Mic1_VolumeBar.DataContext = service.mic_data_context;
            Microphone1_ComboBox.ItemsSource = service.mic_data_context.Mics;
            Mic2_VolumeBar.DataContext = service.mic_data_context;
            Microphone2_ComboBox.ItemsSource = service.mic_data_context.Mics;
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }


            public ComboboxItem(string Text, string Value)
            {
                this.Text = Text;
                this.Value = Value;
            }
            public override string ToString()
            {
                return Text;
            }
        }



        #region MovementSettingCode

        private void CourseModeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == 0) // Synchronous
            {
                AsynchronousGroupBox.IsEnabled = false;
                SelfpacedCombobox.IsEnabled = false;
                SynchronousGroupBox.IsEnabled = true;
            }
            if (((ComboBox)sender).SelectedIndex == 1) // Asynchronous
            {
                AsynchronousGroupBox.IsEnabled = true;
                SelfpacedCombobox.IsEnabled = false;
                SynchronousGroupBox.IsEnabled = false;
            }
            if (((ComboBox)sender).SelectedIndex == 2) // Self-Paced
            {
                AsynchronousGroupBox.IsEnabled = false;
                SelfpacedCombobox.IsEnabled = true;
                SynchronousGroupBox.IsEnabled = false;
            }
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;

                if (CourseModeSelect.SelectedIndex == -1)
                {
                    System.Windows.MessageBox.Show("Please select a course mode before attempting to run the experiment");
                }
                else
                {
                    if (((ComboboxItem)CourseModeSelect.SelectedItem).Value == "Synchronous")
                    {
                        SyncMovement();
                    }
                    else
                    if (((ComboboxItem)CourseModeSelect.SelectedItem).Value == "Asynchronous")
                    {
                        AsyncMovement();
                    }
                    else
                    if (((ComboboxItem)CourseModeSelect.SelectedItem).Value == "Self-Paced")
                    {
                        SelfPacedMovement();
                    }

                    stimulyWindow.Show();


                    GenerateCourseBtn.IsEnabled = true;
                    OpenBtn.IsEnabled = false;
                    CloseBtn.IsEnabled = true;
                }


            }

            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Arrow;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            stimulyWindow.Close();
            CloseBtn.IsEnabled = false;
        }

        private void GenerateCourse_Click(object sender, RoutedEventArgs e)
        {
            generateCourse();
            StartBtn.IsEnabled = true;
            GenerateCourseBtn.IsEnabled = false;
        }

        private void generateCourse()
        {
            stimulyWindow.ResizeMode = ResizeMode.NoResize;
            stimulyWindow.buildCourse();

        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartBtn.IsEnabled = false;
            complexitySlider.IsEnabled = false;
            SpeedSlider.IsEnabled = false;
            ReqFrequencySlider.IsEnabled = false;
            startCourse();


        }

        private void startCourse()
        {
            stimulyWindow.startCourse();
            stimulyWindow.StartSendingInfo();
            stimulyWindow.startRecording();
            StartFullRecording();
        }

        public void enableStartBtn()
        {
            StartBtn.IsEnabled = true;
        }



        #endregion


        #region PrompterMenuCode

        private void PrompterModeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == 0) // Synchronous
            {
                Prompter_SynchronousMode_GroupBox.IsEnabled = true;
                Prompter_SelfPaced_GroupBox.IsEnabled = false;
                Prompter_Asynchronous_GroupBox.IsEnabled = false;
            }
            if (((ComboBox)sender).SelectedIndex == 1) // Asynchronous
            {
                Prompter_SynchronousMode_GroupBox.IsEnabled = false;
                Prompter_SelfPaced_GroupBox.IsEnabled = false;
                Prompter_Asynchronous_GroupBox.IsEnabled = true;
            }
            if (((ComboBox)sender).SelectedIndex == 2) // Self-Paced
            {
                Prompter_SynchronousMode_GroupBox.IsEnabled = false;
                Prompter_SelfPaced_GroupBox.IsEnabled = true;
                Prompter_Asynchronous_GroupBox.IsEnabled = false;
               
            }
        }

        private void prompterOpen_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(pathTxt.Text))
            {
                if (PrompterModeSelect.SelectedIndex == 0)
                {
                    try
                    {
                        SyncReading();
                    }
                    catch (Exception ex)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
                    }
                }
                else
                if (PrompterModeSelect.SelectedIndex == 1)
                {
                    try
                    {
                        AsyncReading();
                    }
                    catch (Exception ex)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
                    }
                }
                    else 
                if (PrompterModeSelect.SelectedIndex == 2)
                {
                    try
                    {
                        SelfPacedReading();
                    }
                    catch (Exception ex)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
                    }
                }

                if(PrompterModeSelect.SelectedIndex == -1)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please select a prompter mode first.");
                }
                prompter.Visibility = System.Windows.Visibility.Visible;
                prompterPlay.IsEnabled = true;

            }

            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No file specified in path");
            }
        }



        private void prompterPlay_Click(object sender, RoutedEventArgs e)
        {
            prompter.play();
        }

        private void RandomizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            int i = r.Next(3);

            PrompterModeSelect.SelectedIndex = i;
    }

        private void TextSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (prompter != null)
                prompter.textSize_Changed((int)TextSizeSlider.Value);
        }

        private void Sync_RBtn_Checked(object sender, RoutedEventArgs e)
        {
            prompterOpen.IsEnabled = true;
            Color1.IsEnabled = true;
            Color2.IsEnabled = false;
            Color3.IsEnabled = false;
        }

        private void Async_RBtn_Checked(object sender, RoutedEventArgs e)
        {
            prompterOpen.IsEnabled = true;
            Color1.IsEnabled = true;
            Color2.IsEnabled = true;
            Color3.IsEnabled = true;
        }
        private void SelfPaced_RBtn_Checked(object sender, RoutedEventArgs e)
        {
            prompterOpen.IsEnabled = true;
            Color1.IsEnabled = false;
            Color2.IsEnabled = false;
            Color3.IsEnabled = false;
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = pathTxt.Text;


            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "TXT Files (.txt)|*.txt";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                pathTxt.Text = filename;
            }
        }
        #endregion

        #region WiiMenuCode

        private void WM1_Detect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                service.ConnectWiimoteToInfo(0);
                WM1_Detect.IsEnabled = false;
                WM1_Disconect.IsEnabled = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private void WM1_Disconect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                service.DisconnectWiimoteFromInfo(0);
                Console.WriteLine("Wiimote 1 has been disconected ;");
                WM1_Detect.IsEnabled = true;
                WM1_Disconect.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void WM2_Detect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                service.ConnectWiimoteToInfo(1);
                WM2_Detect.IsEnabled = false;
                WM2_Disconect.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void WM2_Disconect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                service.DisconnectWiimoteFromInfo(1);
                Console.WriteLine("Wiimote 2 has been disconected ;");
                WM2_Detect.IsEnabled = true;
                WM2_Disconect.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ConnectAll_OnClick(object sender, RoutedEventArgs e)
        {
            service.ConnectAllWiimotes();
        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            Console_TextBox.Inlines.Add("Searching for wii remotes.....");
            service.DetectWiimotes();
            WM1_Detect.IsEnabled = true;
            WM2_Detect.IsEnabled = true;
            Console_TextBox.Inlines.Add(string.Format("Found {0} wiimotes", service.GetRemoteCount()));
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            Console_TextBox.Inlines.Add("Disconecting wii remotes .....");
            service.DisconnectAllWiimotes();
            Console_TextBox.Inlines.Add("Wiiremotes disconected;");
        }

        public void WriteToRemoteMenu(int index, string message)
        {

            Console_TextBox.Inlines.Add(string.Format("Wiimote {0}:{1}; \r\n", index, message));// append text to console log 1


        }

        #endregion

        #region MicrophoneCode

        MicrophoneConstruct mic1PrevVal = null;
        MicrophoneConstruct mic2PrevVal = null;



        private void Microphone1_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                if (((ComboBox)sender).SelectedIndex > -1)
                {
                    if (mic1PrevVal != null) service.stopRecording(mic1PrevVal);

                    bool active = service.ListenToMicrophone(((ComboBox)sender).SelectedIndex, 1);
                    Mic1_Rec.IsEnabled = true;
                    mic1PrevVal = (MicrophoneConstruct)((ComboBox)sender).SelectedValue;
                }

            }
            catch (Exception Ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Ex.Message);
                Microphone1_ComboBox.SelectedIndex = -1;
            }
        }

        private void Microphone2_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (((ComboBox)sender).SelectedIndex > -1)
                {
                    if (mic2PrevVal != null) service.stopRecording(mic2PrevVal);
                    bool active = service.ListenToMicrophone(((ComboBox)sender).SelectedIndex, 2);
                    Mic2_Rec.IsEnabled = true;
                }

                mic2PrevVal = (MicrophoneConstruct)((ComboBox)sender).SelectedValue;
            }
            catch (Exception Ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(Ex.Message);
                Microphone2_ComboBox.SelectedIndex = -1;
            }
        }




        private void Mic1_Rec_Click(object sender, RoutedEventArgs e)
        {

            MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone1_ComboBox.SelectedItem);
            service.startRecording(mic);
            Mic1_Rec.IsEnabled = false;
            Mic1_Stop.IsEnabled = true;
        }

        private void Mic2_Rec_Click(object sender, RoutedEventArgs e)
        {
            MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone2_ComboBox.SelectedItem);
            service.startRecording(mic);
            Mic2_Rec.IsEnabled = false;
            Mic2_Stop.IsEnabled = true;
        }



        private void Mic1_Stop_Click(object sender, RoutedEventArgs e)
        {
            MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone1_ComboBox.SelectedItem);
            service.stopRecording(mic);
            Mic1_Rec.IsEnabled = true;
            Mic1_Stop.IsEnabled = false;
        }


        private void Mic2_Stop_Click(object sender, RoutedEventArgs e)
        {
            MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone1_ComboBox.SelectedItem);
            service.stopRecording(mic);
            Mic2_Rec.IsEnabled = true;
            Mic2_Stop.IsEnabled = false;
        }

        private void StartFullRecording()
        {
            if (Microphone1_ComboBox.SelectedIndex != -1)
            {
                MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone1_ComboBox.SelectedItem);
                service.startRecording(mic);
                Mic1_Rec.IsEnabled = false;
                Mic1_Stop.IsEnabled = true;
            }
            if (Microphone2_ComboBox.SelectedIndex != -1)
            {
                MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone2_ComboBox.SelectedItem);
                service.startRecording(mic);
                Mic2_Rec.IsEnabled = false;
                Mic2_Stop.IsEnabled = true;
            }
        }

        public void StopFullRecording()
        {
            if (Microphone1_ComboBox.SelectedIndex != -1)
            {
                MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone1_ComboBox.SelectedItem);
                service.stopRecording(mic);
                Mic1_Rec.IsEnabled = true;
                Mic1_Stop.IsEnabled = false;
            }
            if (Microphone2_ComboBox.SelectedIndex != -1)
            {
                MicrophoneConstruct mic = (MicrophoneConstruct)(Microphone2_ComboBox.SelectedItem);
                service.stopRecording(mic);
                Mic2_Rec.IsEnabled = true;
                Mic2_Stop.IsEnabled = false;
            }
        }

        private void Microphone_refresh_btn_Click(object sender, RoutedEventArgs e)
        {

            service.getMicrophoneList();
        }


        private void VolumeSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            service.setVolume((int)e.NewValue, 1);
        }

        private void VolumeSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            service.setVolume((int)e.NewValue, 2);
        }
        #endregion

        #region SequenceManagement
        #region ComboBoxesSwitchFeature

        private void MovementCombo_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (ReadingCombo.SelectedIndex != -1 && ((ComboBox)sender).SelectedIndex != -1)
                if (MovementCombo.SelectedValue.ToString().Equals(ReadingCombo.SelectedValue.ToString()))
                {
                    System.Windows.MessageBox.Show("Movement and reading can not have the same values");
                    MovementCombo.SelectedIndex = -1;
                }
        }

        private void ReadingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MovementCombo.SelectedIndex != -1 && ((ComboBox)sender).SelectedIndex != -1)
                if (ReadingCombo.SelectedValue.ToString().Equals(MovementCombo.SelectedValue.ToString()))
                {
                    System.Windows.MessageBox.Show("Movement and reading can not have the same values");
                    ReadingCombo.SelectedIndex = -1;
                }

        }

        private void AsyncCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (((ComboBox)sender).SelectedIndex != -1)
            {
                if (SyncCombo.SelectedIndex != -1)
                {
                    if (AsyncCombo.SelectedValue.ToString().Equals(SyncCombo.SelectedValue.ToString()))
                    {
                        System.Windows.MessageBox.Show("Asynchronous , Synchronous and Self-Paced can not have the same values");
                        AsyncCombo.SelectedIndex = -1;
                    }
                }
            }
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                if (SelfPacedCombo.SelectedIndex != -1)
                {
                    if (AsyncCombo.SelectedValue.ToString().Equals(SelfPacedCombo.SelectedValue.ToString()))
                    {
                        System.Windows.MessageBox.Show("Asynchronous , Synchronous and Self-Paced can not have the same values");
                        AsyncCombo.SelectedIndex = -1;
                    }
                }
            }

        }

        private void SyncCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                if (AsyncCombo.SelectedIndex != -1)
                {
                    if (SyncCombo.SelectedValue.ToString().Equals(AsyncCombo.SelectedValue.ToString()))
                    {
                        System.Windows.MessageBox.Show("Asynchronous , Synchronous and Self-Paced can not have the same values");
                        SyncCombo.SelectedIndex = -1;
                    }
                }
            }
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                if (SelfPacedCombo.SelectedIndex != -1)
                {
                    if (SyncCombo.SelectedValue.ToString().Equals(SelfPacedCombo.SelectedValue.ToString()))
                    {
                        System.Windows.MessageBox.Show("Asynchronous , Synchronous and Self-Paced can not have the same values");
                        SyncCombo.SelectedIndex = -1;
                    }
                }
            }

        }

        private void SelfPacedCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                if (SyncCombo.SelectedIndex != -1)
                {
                    if (SelfPacedCombo.SelectedValue.ToString().Equals(SyncCombo.SelectedValue.ToString()))
                    {
                        System.Windows.MessageBox.Show("Asynchronous , Synchronous and Self-Paced can not have the same values");
                        SelfPacedCombo.SelectedIndex = -1;
                    }
                }
                if (((ComboBox)sender).SelectedIndex != -1)
                {
                    if (AsyncCombo.SelectedIndex != -1)
                    {
                        if (SelfPacedCombo.SelectedValue.ToString().Equals(AsyncCombo.SelectedValue.ToString()))
                        {
                            System.Windows.MessageBox.Show("Asynchronous , Synchronous and Self-Paced can not have the same values");
                            SelfPacedCombo.SelectedIndex = -1;
                        }
                    }

                }
            }
        }
        #endregion

        #endregion

        private void Sequence_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                _mode = 0;
                _window = 0;
                sequence = new SequenceForm(MovementCombo.SelectedValue.ToString(), ReadingCombo.SelectedValue.ToString(), AsyncCombo.SelectedValue.ToString(), SyncCombo.SelectedValue.ToString(), SelfPacedCombo.SelectedValue.ToString());
                System.Timers.Timer t = new System.Timers.Timer();
                t.Interval = 100;
                t.Elapsed += Sequence_Timer_TickEvent;

                if (sequence != null)
                {
                    t.Start();
                    StartFullRecording();
                    service.PingExperimentStart();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Sequence_Timer_TickEvent(object sender, ElapsedEventArgs args)
        {
            #region CloseWindows
            if (stimulyWindow != null)
            {
                Action close_stimulyWindow_action = () =>
                {
                    stimulyWindow.Close();
                };
                this.Dispatcher.Invoke(close_stimulyWindow_action);
                stimulyWindow = null;
            }

            if (prompter != null)
            {
                Action close_prompter_action = () =>
                {
                    prompter.Close();
                };
                this.Dispatcher.Invoke(close_prompter_action);
                prompter = null;
            }
            #endregion
            if (_window == 2)
            {
                _window = 0;
                ((Timer)sender).Dispose();
            }
            else
            {
                var t = (System.Timers.Timer)sender;
                if (t.Interval <= 1000)
                {
                    Action action = () =>
                    {
                        t.Interval = 1000 * 60 * ((int)Sequence_Duration_Slider.Value + 1);
                    };
                    this.Dispatcher.Invoke(action);
                }
                Mode++;
            }
        }

        private void Sequence_next()
        {
            try
            {

                service.PingStartNewPhase();
            }

            catch (Exception Ex)
            {
                throw Ex;
            }
            if (sequence.mode[_mode] == "asynchronous")
            {
                if (sequence.window[_window] == "movement")
                {
                    AsyncMovement();
                    Action action = () =>
                    {
                        stimulyWindow.Show();
                        stimulyWindow.Focus();
                        stimulyWindow.buildCourse();
                        InstructionsPopUp w = new InstructionsPopUp(Instructions.ASYNCHRONOUS_MOVEMENT);
                        w.Show();
                        System.Timers.Timer t = new System.Timers.Timer();
                        t.Interval = 1000 * 60;
                        t.Elapsed += (s, e) =>
                        {
                            this.Dispatcher.Invoke(startCourse);
                            t.Dispose();
                        };
                        t.Start();
                    };

                    if (stimulyWindow != null)
                        this.Dispatcher.Invoke(action);
                    else System.Windows.MessageBox.Show("Movement stimuly could not be generated");
                }

                else if (sequence.window[_window] == "reading")
                {
                    AsyncReading();
                    Action action = () =>
                    {
                        prompter.Show();

                        InstructionsPopUp w = new InstructionsPopUp(Instructions.ASYNCHRONOUS_READING);
                        w.Show();
                        System.Timers.Timer t = new System.Timers.Timer();
                        t.Interval = 1000 * 60;
                        t.Elapsed += (s, e) =>
                        {

                            prompter.play();
                            t.Dispose();
                        };
                        t.Start();

                    };
                    if (prompter != null)
                        this.Dispatcher.Invoke(action);
                    else System.Windows.MessageBox.Show("Reading stimuly could not be generated");
                }
            }

            if (sequence.mode[_mode] == "synchronous")
            {


                if (sequence.window[_window] == "movement")
                {
                    SyncMovement();
                    Action action = () =>
                    {
                        InstructionsPopUp w = new InstructionsPopUp(Instructions.SYNCHRONOUS_MOVEMENT);
                        w.Show();
                        stimulyWindow.Show();
                        stimulyWindow.Focus();
                        stimulyWindow.buildCourse();

                        System.Timers.Timer t = new System.Timers.Timer();
                        t.Interval = 1000 * 60;
                        t.Elapsed += (s, e) =>
                        {
                            this.Dispatcher.Invoke(startCourse);
                            t.Dispose();
                        };
                        t.Start();
                    };
                    this.Dispatcher.Invoke(action);
                }

                else if (sequence.window[_window] == "reading")
                {
                    SyncReading();
                    Action action = () =>
                    {
                        InstructionsPopUp w = new InstructionsPopUp(Instructions.SYNCHRONOUS_READING);
                        prompter.Show();
                        w.Show();

                        System.Timers.Timer t = new System.Timers.Timer();
                        t.Interval = 1000 * 60;
                        t.Elapsed += (s, e) =>
                        {

                            prompter.play();
                            t.Dispose();
                        };
                        t.Start();
                    };

                    this.Dispatcher.Invoke(action);
                }
            }

            if (sequence.mode[_mode] == "selfpaced")
            {

                if (sequence.window[_window] == "movement")
                {

                    SelfPacedMovement();
                    Action action = () =>
                    {
                        InstructionsPopUp w = new InstructionsPopUp(Instructions.SELFPACED_MOVEMENT);
                        w.Show();
                        stimulyWindow.Show();
                        stimulyWindow.Focus();
                        stimulyWindow.buildCourse();
                        System.Timers.Timer t = new System.Timers.Timer();
                        t.Interval = 1000 * 60;
                        t.Elapsed += (s, e) =>
                        {

                            this.Dispatcher.Invoke(startCourse);
                            t.Dispose();
                        };
                        t.Start();
                    };
                    this.Dispatcher.Invoke(action);
                }
                else if (sequence.window[_window] == "reading")
                {
                    SelfPacedReading();
                    Action action = () =>
                    {
                        InstructionsPopUp w = new InstructionsPopUp(Instructions.SELFPACED_READING);
                        w.Show();
                        prompter.Show();


                        System.Timers.Timer t = new System.Timers.Timer();
                        t.Interval = 1000 * 60;
                        t.Elapsed += (s, e) =>
                        {

                            prompter.play();
                            t.Dispose();
                        };
                        t.Start();
                    };
                    this.Dispatcher.Invoke(action);

                }
            }
        }

        private void AsyncMovement()
        {
            Action action = () =>
            {
                System.Windows.Media.Color color1 = (System.Windows.Media.Color)Subject1.SelectedColor;
                System.Windows.Media.Color color2 = (System.Windows.Media.Color)Subject2.SelectedColor;
                System.Windows.Media.Color color3 = (System.Windows.Media.Color)CourseColorPicker.SelectedColor;
                stimulyWindow = new MovementWindow(this, "Asynchronous", (int)complexitySlider2.Value, (int)SpeedSlider2.Value, color1, color2, color3);

            };
            this.Dispatcher.Invoke(action);
        }

        private void SyncMovement()
        {
            Action action = () =>
            {
                System.Windows.Media.Color color1 = (System.Windows.Media.Color)Subject1.SelectedColor;
                System.Windows.Media.Color color2 = (System.Windows.Media.Color)Subject2.SelectedColor;
                System.Windows.Media.Color color3 = (System.Windows.Media.Color)CourseColorPicker.SelectedColor;
                stimulyWindow = new MovementWindow(this, "Synchronous", (int)complexitySlider.Value, (int)SpeedSlider.Value, color1, color2, color3);
            };
            this.Dispatcher.Invoke(action);
        }

        private void SelfPacedMovement()
        {
            Action action = () =>
            {
                System.Windows.Media.Color color1 = (System.Windows.Media.Color)Subject1.SelectedColor;
                System.Windows.Media.Color color2 = (System.Windows.Media.Color)Subject2.SelectedColor;
                System.Windows.Media.Color color3 = (System.Windows.Media.Color)CourseColorPicker.SelectedColor;
                stimulyWindow = new MovementWindow(this, color1, color2, color3, (int)LineThicknessPicker.Value);
            };
            this.Dispatcher.Invoke(action);
        }

        private void AsyncReading()
        {
            Action action = () =>
            {
                System.Windows.Media.Color color1 = (System.Windows.Media.Color)Color1.SelectedColor;
                System.Windows.Media.Color color2 = (System.Windows.Media.Color)Color2.SelectedColor;
                System.Windows.Media.Color color3 = (System.Windows.Media.Color)Color3.SelectedColor;
                prompter = new ReadingWIndow((int)TextSizeSlider.Value, pathTxt.Text, (int)AsyncTraversalSpeed_Slider.Value, (int)Turn_duration_Slider.Value, (int)Switch_Frequency_Slider.Value, color1, color2, color3);
            };

            this.Dispatcher.Invoke(action);
        }

        private void SyncReading()
        {
            Action action = () =>
            {
                System.Windows.Media.Color color1 = (System.Windows.Media.Color)Color1.SelectedColor;
                prompter = new ReadingWIndow((int)TextSizeSlider.Value, pathTxt.Text, (int)SyncTraversalSpeed_Slider.Value, color1);

            };
            this.Dispatcher.Invoke(action);
        }
        private void SelfPacedReading()
        {
            Action action = () =>
            {
                prompter = new ReadingWIndow((int)TextSizeSlider.Value, pathTxt.Text, (int)prompterSpeed.Value);
            };
            this.Dispatcher.Invoke(action);
        }



        private int _window = 0;
        private int _mode = 0;

        public int Window { get { return _window; } set { if (value == 2) _window = 0; else _window = value; } }
        public int Mode
        {
            get { return _mode; }
            set
            {
                Sequence_next();
                _mode = value;
                if (_mode == 3)
                {
                    _mode = 0;
                    Window++;
                }
            }
        }

        internal class SequenceForm
        {
            public string[] window = new string[2];
            public string[] mode = new string[3];

            public SequenceForm(string movement, string reading, string asynchronous, string synchronous, string selfpaced)
            {
                if (movement == "First")
                {
                    window[0] = "movement";
                    window[1] = "reading";
                }
                if (reading == "First")
                {
                    window[0] = "reading";
                    window[1] = "movement";
                }

                if (asynchronous == "First")
                {
                    mode[0] = "asynchronous";
                }
                else if (asynchronous == "Second")
                {
                    mode[1] = "asynchronous";
                }
                else if (asynchronous == "Third")
                {
                    mode[2] = "asynchronous";
                }

                if (synchronous == "First")
                {
                    mode[0] = "synchronous";
                }
                else if (synchronous == "Second")
                {
                    mode[1] = "synchronous";
                }
                else if (synchronous == "Third")
                {
                    mode[2] = "synchronous";
                }

                if (selfpaced == "First")
                {
                    mode[0] = "selfpaced";
                }
                if (selfpaced == "Second")
                {
                    mode[1] = "selfpaced";
                }
                if (selfpaced == "Third")
                {
                    mode[2] = "selfpaced";
                }

            }

        }

        private void PrompterCloseBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Sequence_Stop_Click(object sender, RoutedEventArgs e)
        {
            StopFullRecording();
            #region CloseWindows
            if (stimulyWindow != null)
            {
                Action close_stimulyWindow_action = () =>
                {
                    stimulyWindow.Close();
                };
                this.Dispatcher.Invoke(close_stimulyWindow_action);
                stimulyWindow = null;
            }

            if (prompter != null)
            {
                Action close_prompter_action = () =>
                {
                    prompter.Close();
                };
                this.Dispatcher.Invoke(close_prompter_action);
                prompter = null;
            }
            #endregion
        }
    }
}
